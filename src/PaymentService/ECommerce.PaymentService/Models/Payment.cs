using System.ComponentModel.DataAnnotations;

namespace ECommerce.PaymentService.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required int CartId { get; set; }
        public required decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string PaymentIntentId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 