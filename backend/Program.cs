using Database;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//Controllers and Swagger setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


// Dependency Injection setup
builder.Services.AddScoped<ICatalogoService, CatalogoService>();

var app = builder.Build();


// Middleware setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(MyAllowSpecificOrigins);
}

app.MapControllers();

app.Run();