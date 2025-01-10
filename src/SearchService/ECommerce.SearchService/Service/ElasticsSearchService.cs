using Elasticsearch.Net;
using Nest;

namespace ECommerce.SearchService.Service
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = null!;
    }
    public interface IElasticSearchService
    {
        Task IndexProductAsync(Product product);
        Task<IEnumerable<Product>> SearchAsync(string query);
    }
    public class ElasticsSearchService : IElasticSearchService
    {
        private readonly ElasticClient _elasticClient;
        private const string IndexName = "products";

        public ElasticsSearchService(ElasticClient elasticClient)
        {
            // var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            //     .DefaultIndex("products")
            //     .DefaultMappingFor<Product>(m => m
            //         .IdProperty(p => p.Id)
            //     );

            _elasticClient = elasticClient;
        }

        public async Task IndexProductAsync(Product product)
        {
            var response = await _elasticClient.IndexAsync(product, idx => idx.Index(IndexName));
            if (!response.IsValid)
            {
                throw new Exception($"Failed to index product: {response.ServerError}");
            }
        }
        public async Task<IEnumerable<Product>> SearchAsync(string query)
        {
            var searchResponse = await _elasticClient.SearchAsync<Product>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(query)
                        .Fields(f => f
                            .Field(p => p.Name)
                            .Field(p => p.Description)
                        )
                )
            ));

            if(!searchResponse.IsValid) {
                throw new Exception($"Failed to search products: {searchResponse.ServerError}");
            }

            return searchResponse.Documents.ToList();
        }
    }
}