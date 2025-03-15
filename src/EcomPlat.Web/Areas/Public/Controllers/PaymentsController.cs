﻿using System.Text;
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
    public class PaymentsController : Controller
    {
        private readonly ILogger<PaymentsController> logger;
        private readonly INowPaymentsService paymentService;
        private readonly IOrderRepository orderRepository;
        private readonly ApplicationDbContext context;

        public PaymentsController(
            ApplicationDbContext context,
            INowPaymentsService paymentService,
            IOrderRepository orderRepository,
            ILogger<PaymentsController> logger)
        {
            this.context = context;
            this.paymentService = paymentService;
            this.orderRepository = orderRepository;
            this.logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("payments/confirmnowpayments")]
        public async Task<IActionResult> ConfirmedNowPaymentsAsync()
        {
            string sessionId = this.HttpContext.Session.Id;

            // Load the shopping cart.
            var cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null || !cart.Items.Any())
            {
                return this.RedirectToAction("Index", "Products", new { area = "Public" });
            }

            var paymentRequest = new PaymentRequest()
            {
                // todo
            };


            var invoiceFromProcessor = await this.paymentService.CreateInvoice(paymentRequest);

            return this.Redirect(invoiceFromProcessor.InvoiceUrl);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("payments/nowpaymentscallback")]
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

            //await this.CreateNewSponsoredListing(invoice);

            return this.Ok();
        }

        //private PaymentRequest GetInvoiceRequest(Order order)
        //{
        //    return new PaymentRequest
        //    {
        //        IsFeePaidByUser = true,
        //        PriceAmount = sponsoredListingOffer.Price,
        //        PriceCurrency = this.paymentService.PriceCurrency,
        //        PayCurrency = this.paymentService.PayCurrency,
        //        OrderId = invoice.InvoiceId.ToString(),
        //        OrderDescription = sponsoredListingOffer.Description
        //    };
        //}

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
