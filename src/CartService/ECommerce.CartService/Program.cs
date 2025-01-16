using System.Text;
using ECommerce.CartService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<CartDbContext>(options =>
{
    options.UseSqlite("Data Source=cart.db");
});
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce Cart API", Version = "v1" });
    c.DocInclusionPredicate((version, desc) => true);
    c.EnableAnnotations();
});

// jwt config
// var jwtSettings = builder.Configuration.GetSection("Jwt");
// var jwtKey = jwtSettings["Key"];
// if (string.IsNullOrEmpty(jwtKey))
// {
//     throw new InvalidOperationException("JWT Key is not configured.");
// }
// var key = Encoding.ASCII.GetBytes(jwtKey);

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtSettings["Issuer"],
//         ValidAudience = jwtSettings["Audience"],
//         IssuerSigningKey = new SymmetricSecurityKey(key)
//     };
// });

// builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce Cart API v1"));
    app.MapOpenApi();
}
app.MapControllers();
// app.UseHttpsRedirection();
// app.UseAuthentication();
// app.UseAuthorization();

app.Run();

