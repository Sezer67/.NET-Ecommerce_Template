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
        Task<bool> IndexProductAsync(Product product);
        Task<IEnumerable<Product>> SearchAsync(string query);
        Task<bool> BulkIndexAsync(IEnumerable<Product> products);
        Task EnsureIndexAsync();
    }
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly IElasticClient _elasticClient;
        private const string IndexName = "products";

        public ElasticSearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<bool> IndexProductAsync(Product product)
        {
            try
            {
                var response = await _elasticClient.IndexAsync(product, idx => idx.Index(IndexName));
                if (!response.IsValid)
                {
                    throw new Exception($"Failed to index product: {response.ServerError}");
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to index product: {ex.Message}");
            }
        }
        public async Task<IEnumerable<Product>> SearchAsync(string query)
        {
            /*
            Query içerisinde bu şekilde arama da yapılaiblir. Gayet basit ilerler
            .MultiMatch(m => m
                        .Query(query)
                        .Fields(f => f
                            .Field(p => p.Name)
                            .Field(p => p.Description)
                            .Field(p => p.Currency)
                        )
                    )
            */
            var searchResponse = await _elasticClient.SearchAsync<Product>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            // Fuzzy arama - yazım hatalarına toleranslı
                            sh => sh.Fuzzy(fz => fz
                                .Field(f => f.Name)
                                .Value(query)
                                .Fuzziness(Fuzziness.Auto)
                            ),
                            // Wildcard arama - içinde geçen
                            sh => sh.Wildcard(w => w
                                .Field(f => f.Name)
                                .Value($"*{query}*")
                            ),
                            // Description'da da ara
                            sh => sh.Fuzzy(fz => fz
                                .Field(f => f.Description)
                                .Value(query)
                                .Fuzziness(Fuzziness.Auto)
                            ),
                            sh => sh.Wildcard(w => w
                                .Field(f => f.Description)
                                .Value($"*{query}*")
                            ),
                            // Currency'de tam eşleşme ile ara
                            sh => sh.Term(t => t
                                .Field(f => f.Currency)
                                .Value(query)
                            )
                        )
                    )
                )
                .Sort(sort => sort
                    .Descending(SortSpecialField.Score)
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new Exception($"Failed to search products: {searchResponse.ServerError}");
            }

            return searchResponse.Documents.ToList();
        }

        public async Task EnsureIndexAsync()
        {
            try 
            {
                var existsResponse = await _elasticClient.Indices.ExistsAsync("products");
                if (!existsResponse.Exists)
                {
                    var createIndexResponse = await _elasticClient.Indices.CreateAsync("products", c => c
                        .Settings(s => s
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)
                            .Analysis(a => a
                                .Normalizers(n => n
                                    .Custom("lowercase_normalizer", cn => cn
                                        .Filters("lowercase")
                                    )
                                )
                            ))
                        .Map<Product>(m => m
                            .AutoMap()
                            .Properties(ps => ps
                                .Text(t => t
                                    .Name(n => n.Name)
                                    .Analyzer("standard"))
                                .Text(t => t
                                    .Name(n => n.Description)
                                    .Analyzer("standard"))
                                .Keyword(k => k
                                    .Name(n => n.Currency)
                                    .Normalizer("lowercase_normalizer"))
                                .Number(n => n
                                    .Name(p => p.Price)
                                    .Type(NumberType.Double))
                            )
                        ));

                    if (!createIndexResponse.IsValid)
                    {
                        throw new Exception($"Error creating elasticsearch index: {createIndexResponse.ServerError?.Error?.Reason ?? "Unknown error"}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to ensure elasticsearch index: {ex.Message}");
            }
        }
        public async Task<bool> BulkIndexAsync(IEnumerable<Product> products)
        {
            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index("products")
                .IndexMany(products)
            );
            return bulkResponse.IsValid;
        }
    }
}