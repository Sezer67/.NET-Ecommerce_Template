namespace ECommerce.PaymentService.Dto
{
    public class CreatePaymentDto
    {
        public required int UserId { get; set; }
        public required int CartId { get; set; }
        public required string CardHolderName { get; set; }
        public required string CardNumber { get; set; }
        public required string ExpireMonth { get; set; }
        public required string ExpireYear { get; set; }
        public required string Cvc { get; set; }
        public int Installment { get; set; } = 1;
    }

    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
} 