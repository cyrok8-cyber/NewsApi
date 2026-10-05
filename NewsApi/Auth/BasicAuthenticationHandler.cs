using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using NewsApi.Repositories;
using NewsApi.Utils;

namespace NewsApi.Auth
{
    // Проверяет заголовок Authorization: Basic ... в каждом запросе
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Basic";

        private readonly IUserRepository _userRepository;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IUserRepository userRepository) : base(options, logger, encoder)
        {
            _userRepository = userRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Заголовка нет - значит, пришёл незарегистрированный пользователь (гость)
            if (!Request.Headers.TryGetValue("Authorization", out var headerValue))
                return AuthenticateResult.NoResult();

            if (!AuthenticationHeaderValue.TryParse(headerValue, out var header) ||
                !SchemeName.Equals(header.Scheme, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(header.Parameter))
            {
                return AuthenticateResult.NoResult();
            }

            // Раскодируем Base64 и делим строку "логин:пароль" по первому двоеточию
            // (в самом пароле двоеточие может встречаться)
            string login, password;
            try
            {
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
                var separatorIndex = credentials.IndexOf(':');
                if (separatorIndex < 0)
                    return AuthenticateResult.Fail("Неверный формат учётных данных");

                login = credentials[..separatorIndex];
                password = credentials[(separatorIndex + 1)..];
            }
            catch (FormatException)
            {
                return AuthenticateResult.Fail("Неверный формат учётных данных");
            }

            // Ищем пользователя и сверяем пароль с хешем из базы
            var user = await _userRepository.GetByUsernameAsync(login);
            if (user == null || !PasswordHasher.Verify(password, user.Password))
                return AuthenticateResult.Fail("Неверный логин или пароль");

            // Сохраняем в "удостоверении" id, логин и роль - дальше они доступны через User в контроллерах
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        // При ответе 401 сообщаем клиенту, что ждём именно Basic (так требует стандарт HTTP)
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers.WWWAuthenticate = "Basic realm=\"NewsApi\", charset=\"UTF-8\"";
            return base.HandleChallengeAsync(properties);
        }
    }
}