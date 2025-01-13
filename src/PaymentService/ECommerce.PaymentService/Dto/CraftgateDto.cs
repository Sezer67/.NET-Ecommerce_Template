namespace ECommerce.PaymentService.Dto
{
    public class CreateWalletPaymentDto
    {
        public required int UserId { get; set; }
        public required int CartId { get; set; }
        public required string WalletCardId { get; set; }
        public required int Installment { get; set; } = 1;
    }

    public class SaveCardRequest
    {
        public required int UserId { get; set; }
        public required string CardHolderName { get; set; }
        public required string CardNumber { get; set; }
        public required string ExpireMonth { get; set; }
        public required string ExpireYear { get; set; }
        public required string Cvc { get; set; }
    }

    public class WalletCardDto
    {
        public string CardId { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string BinNumber { get; set; } = string.Empty;
        public string LastFourDigits { get; set; } = string.Empty;
        public string CardAssociation { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
    }

    public class WalletResponseDto
    {
        public string CardId { get; set; } = string.Empty;
        public string CardUserKey { get; set; } = string.Empty;
    }

    public class InstallmentInfoDto
    {
        public string BinNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public List<InstallmentOption> InstallmentOptions { get; set; } = new();
    }

    public class InstallmentOption
    {
        public int InstallmentNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal InstallmentPrice { get; set; }
    }
} 