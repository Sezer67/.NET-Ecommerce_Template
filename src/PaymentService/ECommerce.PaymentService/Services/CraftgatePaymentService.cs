using ECommerce.PaymentService.Data;
using ECommerce.PaymentService.Dto;
using ECommerce.PaymentService.Models;
using Craftgate;
using Craftgate.Model;
using Craftgate.Request;
using Microsoft.EntityFrameworkCore;
using Craftgate.Request.Dto;
using Craftgate.Response;
using System.Text.Json;

namespace ECommerce.PaymentService.Services
{
    public class CraftgatePaymentService : ICraftgatePaymentService
    {
        private readonly PaymentDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CraftgateClient _craftgateClient;
        private readonly HttpClient _httpClient;

        public CraftgatePaymentService(
            PaymentDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient();
            _craftgateClient = new CraftgateClient(
                configuration["Craftgate:ApiKey"] ?? throw new ArgumentNullException("Craftgate:ApiKey"),
                configuration["Craftgate:SecretKey"] ?? throw new ArgumentNullException("Craftgate:SecretKey"),
                configuration["Craftgate:BaseUrl"] // Sandbox veya Production URL
            );
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto paymentDto)
        {
            var cartResponse = await _httpClient.GetAsync($"http://localhost:5295/api/cart/{paymentDto.CartId}");
            
            if (!cartResponse.IsSuccessStatusCode)
            {
                throw new Exception("Cart not found");
            }

            var cartContent = await cartResponse.Content.ReadFromJsonAsync<CartResponse>(new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            if (cartContent == null)
            {
                throw new Exception("Invalid cart data");
            }

            decimal totalAmount = 0;
            List<PaymentItem> paymentItems = new List<PaymentItem>();
            foreach (var item in cartContent.CartItems)
            {
                totalAmount += (decimal)item.Product.Price * (int)item.Quantity;
                paymentItems.Add(new PaymentItem
                {
                    Name = item.Product.Name,
                    Price = (decimal)item.Product.Price * (int)item.Quantity,
                    ExternalId = item.Product.Id.ToString(),
                });
            }

            var paymentRequest = new CreatePaymentRequest
            {
                Price = totalAmount,
                PaidPrice = totalAmount,
                Currency = Currency.TRY,
                PaymentGroup = PaymentGroup.PRODUCT,
                PaymentPhase = PaymentPhase.AUTH,
                Card = new Card
                {
                    CardHolderName = paymentDto.CardHolderName,
                    CardNumber = paymentDto.CardNumber,
                    ExpireYear = paymentDto.ExpireYear,
                    ExpireMonth = paymentDto.ExpireMonth,
                    Cvc = paymentDto.Cvc
                },
                Items = paymentItems,
                Installment = 1,
            };
            
            PaymentResponse? paymentResponse = null;
            string errorMessage = "";
            try
            {
                paymentResponse = await _craftgateClient.Payment().CreatePaymentAsync(paymentRequest);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            
            var payment = new Payment
            {
                UserId = paymentDto.UserId,
                CartId = paymentDto.CartId,
                Amount = totalAmount,
                Currency = "TRY",
                PaymentIntentId = paymentResponse?.Id.ToString() ?? Math.Round((decimal)DateTime.UtcNow.Ticks / 10000).ToString(),
                Status = Models.PaymentStatus.Processing
            };
            // todo: sepeti tamamlandı olarak işaretle;
            await _httpClient.PostAsync($"http://localhost:5295/api/cart/payment/complete/{paymentDto.CartId}", null);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentResponseDto
            {
                Id = payment.Id,
                PaymentIntentId = paymentResponse?.Id.ToString() ?? errorMessage,
                Amount = totalAmount,
                Currency = "TRY",
                Status = paymentResponse?.PaymentStatus.ToString() ?? Models.PaymentStatus.Pending.ToString()
            };
        }

        public async Task<InstallmentInfoDto> GetInstallmentInfoAsync(decimal amount, string binNumber)
        {
            var request = new SearchInstallmentsRequest
            {
                BinNumber = binNumber,
                Price = amount,
                Currency = Currency.TRY
            };

            var response = await _craftgateClient.Installment().SearchInstallmentsAsync(request);

            var installmentInfo = response.Items.FirstOrDefault();
            if (installmentInfo == null)
            {
                throw new Exception("Installment info not found");
            }

            return new InstallmentInfoDto
            {
                BinNumber = binNumber,
                Price = amount,
                InstallmentOptions = installmentInfo.InstallmentPrices.Select(i => new InstallmentOption
                {
                    InstallmentNumber = i.InstallmentNumber,
                    TotalPrice = i.TotalPrice,
                    InstallmentPrice = i.InstallmentPrice
                }).ToList()
            };
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment> UpdatePaymentStatusAsync(string paymentId, Models.PaymentStatus status)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentId)
                ?? throw new Exception("Payment not found");

            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return payment;
        }
    }
} 