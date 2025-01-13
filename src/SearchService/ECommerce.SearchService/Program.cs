using ECommerce.SearchService.Service;
using Nest;
using Elasticsearch.Net;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ElasticSearch client'ını konfigure edin
var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("products") // Varsayılan indeks adı
    .EnableApiVersioningHeader()
    .PrettyJson()
    .RequestTimeout(TimeSpan.FromSeconds(50));

var client = new ElasticClient(settings);

builder.Services.AddSingleton<IElasticClient>(client);

// ElasticSearchService'i ekleyin
builder.Services.AddScoped<IElasticSearchService, ElasticSearchService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce Search API", Version = "v1" });
    c.DocInclusionPredicate((version, desc) => true);
    c.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce Search API v1"));
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// elasticsearch index kontrolü eğer yoksa oluşturur.
using (var scope = app.Services.CreateScope())
{
    var elasticService = scope.ServiceProvider.GetRequiredService<IElasticSearchService>();
    await elasticService.EnsureIndexAsync();
}

app.Run();
