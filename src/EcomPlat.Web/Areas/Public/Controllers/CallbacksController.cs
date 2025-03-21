using System.Text;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.Data.Repositories.Interfaces;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NowPayments.API.Helpers;
using NowPayments.API.Interfaces;
using NowPayments.API.Models;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    public class CallbacksController : Controller
    {
        private readonly ILogger<CallbacksController> logger;
        private readonly INowPaymentsService paymentService;
        private readonly IOrderRepository orderRepository;
        private readonly ApplicationDbContext context;

        public CallbacksController(
            ApplicationDbContext context,
            INowPaymentsService paymentService,
            IOrderRepository orderRepository,
            ILogger<CallbacksController> logger)
        {
            this.context = context;
            this.paymentService = paymentService;
            this.orderRepository = orderRepository;
            this.logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("callbacks/nowpaymentscallback")]
        public async Task<IActionResult> NowPaymentsCallBackAsync()
        {
            using var reader = new StreamReader(this.Request.Body, Encoding.UTF8);
            var callbackPayload = await reader.ReadToEndAsync();

            this.logger.LogError(callbackPayload);

            IpnPaymentMessage? ipnMessage = null;

            try
            {
                ipnMessage = JsonConvert.DeserializeObject<IpnPaymentMessage>(callbackPayload);
            }
            catch (JsonException)
            {
                return this.BadRequest(new { Error = "Failed to parse the request body." });
            }

            if (ipnMessage == null)
            {
                return this.BadRequest(new { Error = StringConstants.DeserializationObjectIsNull });
            }

            var nowPaymentsSig = this.Request
                                     .Headers[NowPayments.API.Constants.StringConstants.HeaderNameAuthCallBack]
                                     .FirstOrDefault() ?? string.Empty;

            bool isValidRequest = this.paymentService.IsIpnRequestValid(
                callbackPayload,
                nowPaymentsSig,
                out string errorMsg);

            if (!isValidRequest)
            {
                return this.BadRequest(new { Error = errorMsg });
            }

            if (ipnMessage == null)
            {
                return this.BadRequest(new { Error = StringConstants.DeserializationObjectIsNull });
            }

            if (ipnMessage.OrderId == null)
            {
                return this.BadRequest(new { Error = "Order ID is null." });
            }

            var order = await this.orderRepository.FindByCustomerOrderIdAsync(ipnMessage.OrderId);

            if (order == null)
            {
                return this.BadRequest(new { Error = StringConstants.InvoiceNotFound });
            }

            order.PaymentResponse = JsonConvert.SerializeObject(ipnMessage);
            order.PaidAmount = ipnMessage.PayAmount;
            order.OutcomeAmount = ipnMessage.OutcomeAmount;

            if (ipnMessage == null)
            {
                return this.BadRequest(new { Error = StringConstants.DeserializationObjectIsNull });
            }

            if (ipnMessage.PaymentStatus == null)
            {
                return this.BadRequest(new { Error = "Payment status is null." });
            }

            var processorPaymentStatus = EnumHelper.ParseStringToEnum<NowPayments.API.Enums.PaymentStatus>(ipnMessage.PaymentStatus);
            var translatedValue = ConvertToInternalStatus(processorPaymentStatus);
            order.PaymentStatus = translatedValue;

            if (ipnMessage.PayCurrency == null)
            {
                return this.BadRequest(new { Error = "Pay currency is null." });
            }

            var processorCurrency = EnumHelper.ParseStringToEnum<Data.Enums.Currency>(ipnMessage.PayCurrency);
            order.PaymentCurrency = processorCurrency;

            await this.orderRepository.UpdateAsync(order);

            await this.UpdateInventory(order);

            return this.Ok();
        }

        private async Task UpdateInventory(Order order)
        {
            
        }

        private static Data.Enums.PaymentStatus ConvertToInternalStatus(
          NowPayments.API.Enums.PaymentStatus externalStatus)
        {
            return externalStatus switch
            {
                NowPayments.API.Enums.PaymentStatus.Unknown => Data.Enums.PaymentStatus.Unknown,
                NowPayments.API.Enums.PaymentStatus.Waiting => Data.Enums.PaymentStatus.InvoiceCreated,
                NowPayments.API.Enums.PaymentStatus.Sending or
                    NowPayments.API.Enums.PaymentStatus.Confirming or
                    NowPayments.API.Enums.PaymentStatus.Confirmed => Data.Enums.PaymentStatus.Pending,
                NowPayments.API.Enums.PaymentStatus.Finished => Data.Enums.PaymentStatus.Paid,
                NowPayments.API.Enums.PaymentStatus.PartiallyPaid => Data.Enums.PaymentStatus.UnderPayment,
                NowPayments.API.Enums.PaymentStatus.Failed or
                    NowPayments.API.Enums.PaymentStatus.Refunded => Data.Enums.PaymentStatus.Failed,
                NowPayments.API.Enums.PaymentStatus.Expired => Data.Enums.PaymentStatus.Expired,
                _ => throw new ArgumentOutOfRangeException(nameof(externalStatus), externalStatus, null),
            };
        }
    }
}
