using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Cors settings
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration.AddJsonFile("Ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
// .AddJwtBearer("Bearer", options =>
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

var app = builder.Build();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerForOcelotUI(options => {
    options.PathToSwaggerGenerator = "/swagger/docs";
    options.ReConfigureUpstreamSwaggerJson = (context, swaggerJson) => {
        var swagger = JObject.Parse(swaggerJson);
        swagger["servers"] = new JArray(new JObject {
            ["url"] = $"{context.Request.Scheme}://{context.Request.Host.Value}"
        });
        return swagger?.ToString() ?? "";
    };
}).UseOcelot().Wait();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();
// app.UseAuthentication();
// app.UseAuthorization();
// app.UseOcelot().Wait();

app.MapControllers();

app.Run();
