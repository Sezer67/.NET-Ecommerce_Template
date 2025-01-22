using ECommerce.ProductService.Data;
using ECommerce.ProductService.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// HTTP Client for Search Service
builder.Services.AddHttpClient("SearchService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5256/"); // Search Service'in portu
});

// Redis Configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<ICategoryCacheService, CategoryCacheService>();

// Domain Services
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Dependencyy
// CategoryController
//     └── ICategoryService (CategoryService)
//         └── ICategoryCacheService (CategoryCacheService)
//             └── IRedisCacheService (RedisCacheService)
//                 └── IConnectionMultiplexer

// Register Search Service
builder.Services.AddScoped<ISearchService, SearchService>();

// Database Configuration
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite("Data Source=products.db"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce Product API", Version = "v1" });
    c.DocInclusionPredicate((version, desc) => true);
    c.EnableAnnotations();
});

// CORS ayarları
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce Product API v1"));
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

