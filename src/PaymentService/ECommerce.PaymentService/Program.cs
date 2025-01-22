using ECommerce.PaymentService.Data;
using ECommerce.PaymentService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce Payment API", Version = "v1" });
    c.DocInclusionPredicate((version, desc) => true);
    c.EnableAnnotations();
});

// HttpClient Factory
builder.Services.AddHttpClient();

// Database Context
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Craftgate Service
builder.Services.AddScoped<ICraftgatePaymentService, CraftgatePaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce Payment API v1"));
}

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
