using System.Text.Json.Serialization;

namespace ECommerce.CartService.Model;

public class CartItem {
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    [JsonIgnore]
    public required Cart Cart { get; set; }
}