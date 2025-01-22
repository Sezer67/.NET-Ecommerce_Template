using ECommerce.PaymentService.Dto;
using ECommerce.PaymentService.Models;
using ECommerce.PaymentService.Services;
using Microsoft.AspNetCore.Mvc;

/*
Akış
A[Client] --> B[Sepet Onay]
B --> C[Kart Bilgileri/Kayıtlı Kart]
C --> D[Taksit Seçimi]
D --> E[Ödeme]
E --> F[3D Doğrulama]
F --> G[Sonuç]

Best Practices
Kart bilgilerini asla veritabanında saklamayın
Tüm tutarları kuruş cinsinden işleyin
3D Secure'u zorunlu tutun
Webhook URL'lerini SSL ile koruyun
İşlem loglarını detaylı tutun
*/

namespace ECommerce.PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ICraftgatePaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ICraftgatePaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> CreatePayment([FromBody] CreatePaymentDto paymentDto)
        {
            try
            {
                var response = await _paymentService.CreatePaymentAsync(paymentDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, "An error occurred while processing your payment");
            }
        }

        [HttpGet("installments")]
        public async Task<ActionResult<InstallmentInfoDto>> GetInstallmentInfo([FromQuery] decimal amount, [FromQuery] string binNumber)
        {
            try
            {
                var info = await _paymentService.GetInstallmentInfoAsync(amount, binNumber);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving installment info");
                return StatusCode(500, "An error occurred while retrieving installment information");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return Ok(payment);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetUserPayments(int userId)
        {
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(payments);
        }
    }
} 