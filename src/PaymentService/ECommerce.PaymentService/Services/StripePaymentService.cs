/*
using ECommerce.PaymentService.Data;
using ECommerce.PaymentService.Dto;
using ECommerce.PaymentService.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace ECommerce.PaymentService.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly PaymentDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _stripeSecretKey;

        public StripePaymentService(
            PaymentDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _stripeSecretKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey");
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        public async Task<PaymentResponseDto> CreatePaymentIntentAsync(CreatePaymentDto paymentDto)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var cartResponse = await httpClient.GetAsync($"http://localhost:5033/api/cart/{paymentDto.CartId}");
            
            if (!cartResponse.IsSuccessStatusCode)
            {
                throw new Exception("Cart not found");
            }

            var cartContent = await cartResponse.Content.ReadFromJsonAsync<dynamic>();
            if (cartContent == null)
            {
                throw new Exception("Invalid cart data");
            }

            decimal totalAmount = 0;
            foreach (var item in cartContent.cartItems)
            {
                totalAmount += (decimal)item.product.price * (int)item.quantity;
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(totalAmount * 100), // Stripe uses cents
                Currency = paymentDto.Currency.ToLower(),
                PaymentMethod = paymentDto.PaymentMethodId,
                Confirm = true,
                ReturnUrl = "http://localhost:3000/payment/success" // Frontend success URL
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            var payment = new Payment
            {
                UserId = paymentDto.UserId,
                CartId = paymentDto.CartId,
                Amount = totalAmount,
                Currency = paymentDto.Currency,
                PaymentIntentId = intent.Id,
                Status = PaymentStatus.Processing
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentResponseDto
            {
                Id = payment.Id,
                PaymentIntentId = intent.Id,
                ClientSecret = intent.ClientSecret,
                Amount = totalAmount,
                Currency = paymentDto.Currency,
                Status = intent.Status
            };
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task<Payment?> GetPaymentByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment> UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId)
                ?? throw new Exception("Payment not found");

            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return payment;
        }
    }
} 
*/