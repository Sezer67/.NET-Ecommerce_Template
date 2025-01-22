using ECommerce.PaymentService.Dto;
using ECommerce.PaymentService.Models;

namespace ECommerce.PaymentService.Services
{
    public interface ICraftgatePaymentService
    {
        Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto paymentDto);
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
        Task<Payment> UpdatePaymentStatusAsync(string paymentId, PaymentStatus status);
        Task<InstallmentInfoDto> GetInstallmentInfoAsync(decimal amount, string binNumber);
    }
} 