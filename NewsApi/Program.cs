using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NewsApi.Auth;
using NewsApi.Data;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services;
using NewsApi.Utils;
using NewsApi.Services.Facades;
using NewsApi.Services.Censorship;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<SqliteAppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<PostgresAppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresConnection"),
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public")));

builder.Services.AddScoped<AppDbContext>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var databaseProvider = config.GetValue<string>("DatabaseProvider") ?? "Sqlite";

    return databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
        ? sp.GetRequiredService<PostgresAppDbContext>()
        : sp.GetRequiredService<SqliteAppDbContext>();
});

// Репозитории
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IBannedWordRepository, BannedWordRepository>();
builder.Services.AddScoped<IEditorialOfficeRepository, EditorialOfficeRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// Сервисы
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEditorialOfficeService, EditorialOfficeService>();
builder.Services.AddScoped<IChatService, ChatService>();

// Цензура чата - цепочка обработчиков (Chain of Responsibility).
// Обработчики хранят ссылку на следующий, поэтому создаются заново на каждый запрос (Scoped)
builder.Services.AddScoped<EmptyMessageHandler>();
builder.Services.AddScoped<MaxLengthHandler>();
builder.Services.AddScoped<LinkFilterHandler>();
builder.Services.AddScoped<BannedWordsHandler>();
builder.Services.AddScoped<CapsLockHandler>();
builder.Services.AddScoped<IChatCensorshipChain, ChatCensorshipChain>();
builder.Services.AddScoped<INewsCommentsFacade, NewsCommentsFacade>();

// Session — хранение данных пользователя после успешной Basic Auth проверки
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Авторизация: Basic Authentication
builder.Services.AddAuthentication(BasicAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

// Swagger с кнопкой Authorize для ввода логина/пароля (Basic)
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Введите логин и пароль (Basic Authentication)"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("basic", document)] = []
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

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

            foreach (var user in db.Users.ToList())
            {
                user.Password = PasswordHasher.Hash(user.Password);
            }
            db.SaveChanges();
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }