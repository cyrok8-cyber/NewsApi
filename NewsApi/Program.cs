using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NewsApi.Auth;
using NewsApi.Data;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services;
using NewsApi.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Подключаем базу данных SQLite через Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Репозитории
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Сервисы
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserService, UserService>();

// Авторизация через Basic auth
builder.Services.AddAuthentication(BasicAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Swagger с кнопкой Authorize для ввода логина и пароля
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Введите логин и пароль пользователя"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("basic", document)] = []
    });
});

var app = builder.Build();

// Глобальная обработка ошибок - должна идти первой, чтобы ловить исключения из всего конвейера
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Применяем миграции и наполняем базу тестовыми данными при первом запуске
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        var sqlPath = Path.Combine(AppContext.BaseDirectory, "Data", "data.sql");
        if (File.Exists(sqlPath))
        {
            var sqlScript = File.ReadAllText(sqlPath);
            db.Database.ExecuteSqlRaw(sqlScript);

            // В data.sql пароли записаны открытым текстом для наглядности,
            // сразу после загрузки превращаем их в хеши
            foreach (var user in db.Users.ToList())
            {
                user.Password = PasswordHasher.Hash(user.Password);
            }
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Порядок важен: сначала выясняем, кто пришёл, потом решаем, что ему можно
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();