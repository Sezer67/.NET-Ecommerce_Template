using ECommerce.PaymentService.Dto;
using ECommerce.PaymentService.Models;

namespace ECommerce.PaymentService.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentIntentAsync(CreatePaymentDto paymentDto);
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
        Task<Payment> UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status);
        Task<Payment?> GetPaymentByPaymentIntentIdAsync(string paymentIntentId);
    }
} 