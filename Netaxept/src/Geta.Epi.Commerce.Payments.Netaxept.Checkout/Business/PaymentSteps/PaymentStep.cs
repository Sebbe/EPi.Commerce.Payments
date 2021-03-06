﻿using System;
using EPiServer.Security;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment step 
    /// </summary>
    public abstract class PaymentStep
    {
        protected NetaxeptServiceClient Client;
        protected PaymentStep Successor;

        /// <summary>
        /// Constructor. Get the merchantId and token and create a connection object
        /// </summary>
        /// <param name="payment"></param>
        protected PaymentStep(Payment payment)
        {
            var paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            var merchantId = paymentMethodDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
            var token = paymentMethodDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);
            var isProduction = bool.Parse(paymentMethodDto.GetParameter(NetaxeptConstants.IsProductionField, "false"));

            var connection = new ClientConnection(merchantId, token, isProduction);
            Client = new NetaxeptServiceClient(connection);
        }

        public void SetSuccessor(PaymentStep successor)
        {
            this.Successor = successor;
        }

        /// <summary>
        /// Process payment step
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool Process(Payment payment, ref string message);

        /// <summary>
        /// Add note to order
        /// </summary>
        /// <param name="orderForm"></param>
        /// <param name="title"></param>
        /// <param name="detail"></param>
        public void AddNote(OrderForm orderForm, string title, string detail)
        {
            AddNote(orderForm, title, detail, false);
        }

        /// <summary>
        /// Add note to order
        /// </summary>
        /// <param name="orderForm"></param>
        /// <param name="title"></param>
        /// <param name="detail"></param>
        /// <param name="save"></param>
        public void AddNote(OrderForm orderForm, string title, string detail, bool save)
        {
            OrderNote on = orderForm.Parent.OrderNotes.AddNew();
            on.Detail = detail;
            on.Title = title;
            on.Type = OrderNoteTypes.System.ToString();
            on.Created = DateTime.Now;
            on.CustomerId = PrincipalInfo.CurrentPrincipal.GetContactId();

            if (save)
            {
                orderForm.Parent.AcceptChanges();
            }
        }
    }
}
