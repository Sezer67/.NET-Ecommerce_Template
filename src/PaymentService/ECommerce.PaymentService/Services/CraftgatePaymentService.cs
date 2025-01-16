// using ECommerce.PaymentService.Data;
// using ECommerce.PaymentService.Dto;
// using ECommerce.PaymentService.Models;
// using Craftgate;
// using Craftgate.Model;
// using Craftgate.Request;
// using Microsoft.EntityFrameworkCore;
// using Craftgate.Request.Dto;

// namespace ECommerce.PaymentService.Services
// {
//     public class CraftgatePaymentService : ICraftgatePaymentService
//     {
//         private readonly PaymentDbContext _context;
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly CraftgateClient _craftgateClient;

//         public CraftgatePaymentService(
//             PaymentDbContext context,
//             IHttpClientFactory httpClientFactory,
//             IConfiguration configuration)
//         {
//             _context = context;
//             _httpClientFactory = httpClientFactory;
//             _craftgateClient = new CraftgateClient(
//                 configuration["Craftgate:ApiKey"] ?? throw new ArgumentNullException("Craftgate:ApiKey"),
//                 configuration["Craftgate:SecretKey"] ?? throw new ArgumentNullException("Craftgate:SecretKey"),
//                 configuration["Craftgate:BaseUrl"] // Sandbox veya Production URL
//             );
//         }

//         public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto paymentDto)
//         {
//             var httpClient = _httpClientFactory.CreateClient();
//             var cartResponse = await httpClient.GetAsync($"http://localhost:5033/api/cart/{paymentDto.CartId}");
            
//             if (!cartResponse.IsSuccessStatusCode)
//             {
//                 throw new Exception("Cart not found");
//             }

//             var cartContent = await cartResponse.Content.ReadFromJsonAsync<dynamic>();
//             if (cartContent == null)
//             {
//                 throw new Exception("Invalid cart data");
//             }

//             decimal totalAmount = 0;
//             foreach (var item in cartContent.cartItems)
//             {
//                 totalAmount += (decimal)item.product.price * (int)item.quantity;
//             }

//             var paymentRequest = new CreatePaymentRequest
//             {
//                 Price = totalAmount,
//                 PaidPrice = totalAmount,
//                 Currency = Currency.TRY,
//                 PaymentGroup = PaymentGroup.PRODUCT,
//                 PaymentPhase = PaymentPhase.AUTH,
//                 Card = new Card
//                 {
//                     CardHolderName = paymentDto.CardHolderName,
//                     CardNumber = paymentDto.CardNumber,
//                     ExpireYear = paymentDto.ExpireYear,
//                     ExpireMonth = paymentDto.ExpireMonth,
//                     Cvc = paymentDto.Cvc
//                 },
//                 Items = new List<PaymentItem>
//                 {
//                     new PaymentItem
//                     {
//                         Name = "Sipariş",
//                         Price = totalAmount,
//                         ExternalId = paymentDto.CartId.ToString()
//                     }
//                 }
//             };

//             var paymentResponse = await _craftgateClient.Payment().CreatePaymentAsync(paymentRequest);

//             var payment = new Payment
//             {
//                 UserId = paymentDto.UserId,
//                 CartId = paymentDto.CartId,
//                 Amount = totalAmount,
//                 Currency = "TRY",
//                 PaymentIntentId = paymentResponse.Id.ToString(),
//                 Status = Models.PaymentStatus.Processing
//             };

//             _context.Payments.Add(payment);
//             await _context.SaveChangesAsync();

//             return new PaymentResponseDto
//             {
//                 Id = payment.Id,
//                 PaymentIntentId = paymentResponse.Id.ToString(),
//                 Amount = totalAmount,
//                 Currency = "TRY",
//                 Status = paymentResponse.PaymentStatus.ToString()
//             };
//         }

//         public async Task<PaymentResponseDto> CreateWalletPaymentAsync(CreateWalletPaymentDto paymentDto)
//         {
//             var httpClient = _httpClientFactory.CreateClient();
//             var cartResponse = await httpClient.GetAsync($"http://localhost:5033/api/cart/{paymentDto.CartId}");
            
//             if (!cartResponse.IsSuccessStatusCode)
//             {
//                 throw new Exception("Cart not found");
//             }

//             var cartContent = await cartResponse.Content.ReadFromJsonAsync<dynamic>();
//             decimal totalAmount = 0;
//             foreach (var item in cartContent.cartItems)
//             {
//                 totalAmount += (decimal)item.product.price * (int)item.quantity;
//             }

//             var storedCard = await _craftgateClient.Wallet().RetrieveMemberWalletAsync(paymentDto.WalletCardId);

//             var paymentRequest = new CreatePaymentRequest
//             {
//                 Price = totalAmount,
//                 PaidPrice = totalAmount,
//                 Currency = Currency.TRY,
//                 PaymentGroup = PaymentGroup.PRODUCT,
//                 PaymentPhase = PaymentPhase.AUTH,
//                 CardUserKey = storedCard.CardUserKey,
//                 CardToken = storedCard.CardToken,
//                 Installment = paymentDto.Installment,
//                 Items = new List<CreatePaymentItem>
//                 {
//                     new CreatePaymentItem
//                     {
//                         Name = "Sipariş",
//                         Price = totalAmount,
//                         ExternalId = paymentDto.CartId.ToString()
//                     }
//                 }
//             };

//             var paymentResponse = _craftgate.Payment().CreatePayment(paymentRequest);

//             var payment = new Payment
//             {
//                 UserId = paymentDto.UserId,
//                 CartId = paymentDto.CartId,
//                 Amount = totalAmount,
//                 Currency = "TRY",
//                 PaymentIntentId = paymentResponse.Id.ToString(),
//                 Status = Models.PaymentStatus.Processing
//             };

//             _context.Payments.Add(payment);
//             await _context.SaveChangesAsync();

//             return new PaymentResponseDto
//             {
//                 Id = payment.Id,
//                 PaymentIntentId = paymentResponse.Id.ToString(),
//                 Amount = totalAmount,
//                 Currency = "TRY",
//                 Status = paymentResponse.PaymentStatus.ToString()
//             };
//         }

//         public async Task<WalletResponseDto> SaveCardToWalletAsync(SaveCardRequest request)
//         {
//             var storeCardRequest = new StoreCardRequest
//             {
//                 CardHolderName = request.CardHolderName,
//                 CardNumber = request.CardNumber,
//                 ExpireMonth = request.ExpireMonth,
//                 ExpireYear = request.ExpireYear,
//                 CardAlias = $"Card_{request.UserId}"
//             };

//             var response = _craftgate.Wallet().StoreCard(storeCardRequest);

//             return new WalletResponseDto
//             {
//                 CardId = response.CardId,
//                 CardUserKey = response.CardUserKey
//             };
//         }

//         public async Task<IEnumerable<WalletCardDto>> GetWalletCardsAsync(int userId)
//         {
//             var searchRequest = new SearchStoredCardsRequest
//             {
//                 CardAlias = $"Card_{userId}"
//             };

//             var cards = _craftgate.Wallet().SearchStoredCards(searchRequest);

//             return cards.Items.Select(card => new WalletCardDto
//             {
//                 CardId = card.CardId,
//                 CardHolderName = card.CardHolderName,
//                 BinNumber = card.BinNumber,
//                 LastFourDigits = card.LastFourDigits,
//                 CardAssociation = card.CardAssociation,
//                 CardBrand = card.CardBrand
//             });
//         }

//         public async Task<InstallmentInfoDto> GetInstallmentInfoAsync(decimal amount, string binNumber)
//         {
//             var request = new SearchInstallmentsRequest
//             {
//                 BinNumber = binNumber,
//                 Price = amount,
//                 Currency = Currency.TRY
//             };

//             var response = _craftgate.Installment().SearchInstallments(request);

//             var installmentInfo = response.Items.FirstOrDefault();
//             if (installmentInfo == null)
//             {
//                 throw new Exception("Installment info not found");
//             }

//             return new InstallmentInfoDto
//             {
//                 BinNumber = binNumber,
//                 Price = amount,
//                 InstallmentOptions = installmentInfo.InstallmentPrices.Select(i => new InstallmentOption
//                 {
//                     InstallmentNumber = i.InstallmentNumber,
//                     TotalPrice = i.TotalPrice,
//                     InstallmentPrice = i.InstallmentPrice
//                 }).ToList()
//             };
//         }

//         public async Task<Payment?> GetPaymentByIdAsync(int id)
//         {
//             return await _context.Payments.FindAsync(id);
//         }

//         public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
//         {
//             return await _context.Payments
//                 .Where(p => p.UserId == userId)
//                 .OrderByDescending(p => p.CreatedAt)
//                 .ToListAsync();
//         }

//         public async Task<Payment> UpdatePaymentStatusAsync(string paymentId, Models.PaymentStatus status)
//         {
//             var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentId)
//                 ?? throw new Exception("Payment not found");

//             payment.Status = status;
//             payment.UpdatedAt = DateTime.UtcNow;

//             await _context.SaveChangesAsync();
//             return payment;
//         }
//     }
// } 