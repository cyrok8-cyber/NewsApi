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
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Basic";

        private readonly IUserRepository _userRepository;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IUserRepository userRepository)
            : base(options, logger, encoder)
        {
            _userRepository = userRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var cachedUserId = Context.Session.GetInt32("UserId");
            var cachedUsername = Context.Session.GetString("Username");
            var cachedRole = Context.Session.GetString("Role");

            if (cachedUserId.HasValue && cachedUsername is not null && cachedRole is not null)
            {
                var cachedClaims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, cachedUserId.Value.ToString()),
                    new Claim(ClaimTypes.Name, cachedUsername),
                    new Claim(ClaimTypes.Role, cachedRole)
                };

                var cachedIdentity = new ClaimsIdentity(cachedClaims, Scheme.Name);
                var cachedPrincipal = new ClaimsPrincipal(cachedIdentity);
                var cachedTicket = new AuthenticationTicket(cachedPrincipal, Scheme.Name);

                return AuthenticateResult.Success(cachedTicket);
            }

            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Отсутствует заголовок Authorization");

            string username, password;
            try
            {
                var header = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"].ToString());
                if (!"Basic".Equals(header.Scheme, StringComparison.OrdinalIgnoreCase) || header.Parameter is null)
                    return AuthenticateResult.Fail("Неверная схема авторизации");

                var bytes = Convert.FromBase64String(header.Parameter);
                var decoded = Encoding.UTF8.GetString(bytes);
                var parts = decoded.Split(':', 2);
                if (parts.Length != 2)
                    return AuthenticateResult.Fail("Неверный формат заголовка Authorization");

                username = parts[0];
                password = parts[1];
            }
            catch
            {
                return AuthenticateResult.Fail("Не удалось разобрать заголовок Authorization");
            }

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user is null)
                return AuthenticateResult.Fail("Неверный логин или пароль");

            if (user.BlockedUntil.HasValue && user.BlockedUntil.Value > DateTime.UtcNow)
                return AuthenticateResult.Fail($"Пользователь заблокирован до {user.BlockedUntil.Value:O}");

            var passwordOk = PasswordHasher.Verify(password, user.Password);

            if (!passwordOk)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= BlockingPolicy.MaxFailedLoginAttempts)
                {
                    user.BlockedUntil = DateTime.UtcNow.Add(BlockingPolicy.LoginBlockDuration);
                    user.FailedLoginAttempts = 0;
                }
                await _userRepository.UpdateAsync(user);
                return AuthenticateResult.Fail("Неверный логин или пароль");
            }

            if (user.FailedLoginAttempts != 0 || user.BlockedUntil.HasValue)
            {
                user.FailedLoginAttempts = 0;
                user.BlockedUntil = null;
                await _userRepository.UpdateAsync(user);
            }

            Context.Session.SetInt32("UserId", user.Id);
            Context.Session.SetString("Username", user.Username);
            Context.Session.SetString("Role", user.Role.Name);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic";
            return base.HandleChallengeAsync(properties);
        }
    }
}