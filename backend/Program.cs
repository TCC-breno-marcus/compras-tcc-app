using Database;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ComprasTccApp.Services.Interfaces;
using ComprasTccApp.Backend.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Controllers and Swagger setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 
// Authentication setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    { 
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Database setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Dependency Injection setup ---
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleService, RoleService>();
//builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/seed-database", async (AppDbContext context, ILogger<Program> logger) =>
    {
        try
        {
            logger.LogInformation("Iniciando o seeding do banco de dados via endpoint...");
            await DataSeeder.SeedUsers(context);
            return Results.Ok("Banco de dados populado com sucesso!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao popular o banco de dados.");
            return Results.Problem("Ocorreu um erro durante o seeding do banco de dados.");
        }
    })
    .WithTags("Database")
    .RequireAuthorization("RequireAdminRole");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();