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
        public string Currency { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Kategori bilgileri
        public List<CategoryInfo> Categories { get; set; } = new();
        public List<string> CategoryPaths { get; set; } = new(); // Filtreleme için
        public List<string> Tags { get; set; } = new();
    }

    public class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int Level { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class SearchRequest
    {
        public string? Query { get; set; }
        public List<string>? CategoryPaths { get; set; }
        public List<string>? Tags { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Currency { get; set; }
        public bool? InStock { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public interface IElasticSearchService
    {
        Task<bool> IndexProductAsync(Product product);
        Task<(IEnumerable<Product> Products, long Total)> SearchAsync(SearchRequest request);
        Task<bool> BulkIndexAsync(IEnumerable<Product> products);
        Task EnsureIndexAsync();
        Task ResetIndexAsync();
    }

    public class ElasticSearchService : IElasticSearchService
    {
        private readonly IElasticClient _elasticClient;
        private const string IndexName = "products";

        public ElasticSearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task EnsureIndexAsync()
        {
            try 
            {
                var existsResponse = await _elasticClient.Indices.ExistsAsync(IndexName);
                if (!existsResponse.Exists)
                {
                    var createIndexResponse = await _elasticClient.Indices.CreateAsync(IndexName, c => c
                        .Settings(s => s
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)
                            .Analysis(a => a
                                .Analyzers(an => an
                                    .Custom("path_hierarchy", ca => ca
                                        .Tokenizer("path_hierarchy")
                                        .Filters("lowercase")
                                    )
                                    /*
                                    Orijinal: "Erkek/Giyim/Tişört"
                                    Tokenizer sonucu: ["Erkek", "Erkek/Giyim", "Erkek/Giyim/Tişört"]
                                    Lowercase filter sonrası: ["erkek", "erkek/giyim", "erkek/giyim/tişört"]
                                    */
                                )
                                .Normalizers(n => n
                                    .Custom("lowercase_normalizer", cn => cn
                                        .Filters("lowercase")
                                    ) // MEtin tokenlara bölünmez, olduğu gibi alınır ve küçültülür.
                                )
                            ))
                        .Map<Product>(m => m
                            .Properties(ps => ps
                                .Text(t => t
                                    .Name(n => n.Name)
                                    .Analyzer("standard"))
                                .Text(t => t
                                    .Name(n => n.Description)
                                    .Analyzer("standard"))
                                .Number(n => n
                                    .Name(p => p.Price)
                                    .Type(NumberType.Double))
                                .Keyword(k => k
                                    .Name(n => n.Currency)
                                    .Normalizer("lowercase_normalizer"))
                                .Boolean(b => b
                                    .Name(n => n.IsActive))
                                .Number(n => n
                                    .Name(p => p.StockQuantity)
                                    .Type(NumberType.Integer))
                                .Date(d => d
                                    .Name(p => p.CreatedAt))
                                .Nested<CategoryInfo>(n => n
                                    .Name(p => p.Categories)
                                    .Properties(cp => cp
                                        .Number(cn => cn.Name(c => c.Id))
                                        .Keyword(ck => ck.Name(c => c.Name))
                                        .Keyword(ck => ck.Name(c => c.Path))
                                        .Number(cn => cn.Name(c => c.Level))
                                        .Boolean(cb => cb.Name(c => c.IsPrimary))
                                    ))
                                .Keyword(k => k
                                    .Name(p => p.CategoryPaths)
                                    .Fields(f => f
                                        .Text(t => t
                                            .Name("hierarchy")
                                            .Analyzer("path_hierarchy")
                                        )
                                    ))
                                .Keyword(k => k
                                    .Name(p => p.Tags))
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

        public async Task<(IEnumerable<Product> Products, long Total)> SearchAsync(SearchRequest request)
        {
            var searchResponse = await _elasticClient.SearchAsync<Product>(s => s
                .Index(IndexName)
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q
                    .Bool(b =>
                    {
                        // must ve filter olarak iki liste kullanmamızın nedeni, ElasticSearch sorgularında
                        // "must" ile belirtilen koşulların hepsinin eşleşmesi gerektiği, "filter" ile belirtilen
                        // koşulların ise önbelleğe alınarak performansın artırılmasıdır.
                        var must = new List<QueryContainer>();
                        var filter = new List<QueryContainer>();

                        // Text search
                        if (!string.IsNullOrEmpty(request.Query))
                        {
                            must.Add(q.MultiMatch(mm => mm
                                .Fields(f => f
                                    .Field(p => p.Name, 2.0) // Name alanını boostlama
                                    .Field(p => p.Description)
                                )
                                .Query(request.Query)
                                .Type(TextQueryType.MostFields) // DB sorgusundaki like gibi arama
                                .Fuzziness(Fuzziness.EditDistance(2))
                                .Operator(Operator.Or)
                            ));
                        }

                        // Category filter
                        if (request.CategoryPaths?.Any() == true)
                        {
                            filter.Add(q.Terms(t => t
                                .Field(f => f.CategoryPaths)
                                .Terms(request.CategoryPaths)
                            ));
                        }

                        // Tag filter
                        if (request.Tags?.Any() == true)
                        {
                            filter.Add(q.Terms(t => t
                                .Field(f => f.Tags)
                                .Terms(request.Tags)
                            ));
                        }

                        // Price range
                        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
                        {
                            filter.Add(q.Range(r => r
                                .Field(f => f.Price)
                                .GreaterThanOrEquals(request.MinPrice.HasValue ? (double?)request.MinPrice.Value : null)
                                .LessThanOrEquals(request.MaxPrice.HasValue ? (double?)request.MaxPrice.Value : null)
                            ));
                        }

                        // Currency filter
                        if (!string.IsNullOrEmpty(request.Currency))
                        {
                            filter.Add(q.Term(t => t
                                .Field(f => f.Currency)
                                .Value(request.Currency.ToLowerInvariant())
                            ));
                        }

                        // Stock filter
                        if (request.InStock.HasValue)
                        {
                            if (request.InStock.Value)
                            {
                                filter.Add(q.Range(r => r
                                    .Field(f => f.StockQuantity)
                                    .GreaterThan(0)
                                ));
                            }
                            else
                            {
                                filter.Add(q.Term(t => t
                                    .Field(f => f.StockQuantity)
                                    .Value(0)
                                ));
                            }
                        }

                        // Active products only
                        filter.Add(q.Term(t => t
                            .Field(f => f.IsActive)
                            .Value(true)
                        ));

                        return b
                            .Must(must.ToArray())
                            .Filter(filter.ToArray());
                    })
                )
                .Sort(sort =>
                {
                    if (string.IsNullOrEmpty(request.SortBy))
                        return sort.Descending(SortSpecialField.Score);

                    return request.SortBy.ToLower() switch
                    {
                        "price" => request.SortDirection?.ToLower() == "desc" 
                            ? sort.Descending(f => f.Price) 
                            : sort.Ascending(f => f.Price),
                        "name" => request.SortDirection?.ToLower() == "desc"
                            ? sort.Descending(f => f.Name)
                            : sort.Ascending(f => f.Name),
                        "created" => request.SortDirection?.ToLower() == "desc"
                            ? sort.Descending(f => f.CreatedAt)
                            : sort.Ascending(f => f.CreatedAt),
                        _ => sort.Descending(SortSpecialField.Score)
                    };
                }));

            if (!searchResponse.IsValid)
            {
                throw new Exception($"Failed to search products: {searchResponse.ServerError}");
            }

            return (searchResponse.Documents, searchResponse.Total);
        }

        public async Task<bool> BulkIndexAsync(IEnumerable<Product> products)
        {
            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(products)
            );
            return bulkResponse.IsValid;
        }

        public async Task ResetIndexAsync()
        {
            // Önce indeksi sil
            await _elasticClient.Indices.DeleteAsync(IndexName);
            // Sonra yeniden oluştur
            await EnsureIndexAsync();
        }
    }
}