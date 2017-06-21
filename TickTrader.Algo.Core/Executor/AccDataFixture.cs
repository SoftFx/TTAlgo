﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class AccDataFixture : CrossDomainObject
    {
        private IFixtureContext context;
        private Dictionary<string, Currency> currencies;

        public AccDataFixture(IFixtureContext context)
        {
            this.context = context;
        }

        public IAccountInfoProvider DataProvider { get; set; }

        public void Start()
        {
            if (DataProvider != null)
                DataProvider.SyncInvoke(Init);
        }

        private void Init()
        {
            var builder = context.Builder;

            DataProvider.OrderUpdated += DataProvider_OrderUpdated;
            DataProvider.BalanceUpdated += DataProvider_BalanceUpdated;
            DataProvider.PositionUpdated += DataProvider_PositionUpdated;

            var accType = DataProvider.AccountType;

            builder.Account.Id = DataProvider.Account;
            builder.Account.Type = accType;

            currencies = builder.Currencies.CurrencyListImp.ToDictionary(c => c.Name);

            if (accType == Api.AccountTypes.Cash)
            {
                builder.Account.Balance = double.NaN;
                builder.Account.BalanceCurrency = "";
            }
            else
            {
                builder.Account.Balance = (double)DataProvider.Balance;
                builder.Account.BalanceCurrency = DataProvider.BalanceCurrency;
                builder.Account.Leverage = DataProvider.Leverage;
            }
            
            foreach (var order in DataProvider.GetOrders())
                builder.Account.Orders.Add(order);
            foreach (var asset in DataProvider.GetAssets())
                builder.Account.Assets.Update(asset);
        }

        public void Stop()
        {
            DataProvider.SyncInvoke(Deinit);
        }

        private void Deinit()
        {
            DataProvider.OrderUpdated -= DataProvider_OrderUpdated;
        }

        private static void ApplyOrderEntity(OrderExecReport eReport, OrdersCollection collection)
        {
            if (eReport.Action == OrderEntityAction.Added)
                collection.Add(eReport.OrderCopy);
            else if (eReport.Action == OrderEntityAction.Removed)
                collection.Remove(eReport.OrderId);
            else if (eReport.Action == OrderEntityAction.Updated)
                collection.Replace(eReport.OrderCopy);
        }

        private void DataProvider_BalanceUpdated(BalanceOperationReport report)
        {
            var accProxy = context.Builder.Account;

            if (accProxy.Type == Api.AccountTypes.Gross || accProxy.Type == Api.AccountTypes.Net)
            {
                accProxy.Balance = report.Balance;
                accProxy.FireBalanceUpdateEvent();
            }
            else if (accProxy.Type == Api.AccountTypes.Cash)
            {
                accProxy.Assets.Update(new AssetEntity(report.Balance, report.CurrencyCode, currencies));
                accProxy.Assets.FireModified(new AssetUpdateEventArgsImpl(new AssetEntity(report.Balance, report.CurrencyCode, currencies)));
            }
        }

        private void DataProvider_PositionUpdated(PositionExecReport report)
        {
        }

        private void DataProvider_OrderUpdated(OrderExecReport eReport)
        {
            context.Enqueue(b =>
            {
                UpdateOrders(b, eReport);
                UpdateBalance(b, eReport);
            });
        }

        private void UpdateOrders(PluginBuilder builder, OrderExecReport eReport)
        {
            var orderCollection = builder.Account.Orders;

            if (eReport.ExecAction == OrderExecAction.Opened)
            {
                ApplyOrderEntity(eReport, orderCollection);
                var newOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                orderCollection.FireOrderOpened(new OrderOpenedEventArgsImpl(newOrder));
            }
            else if (eReport.ExecAction == OrderExecAction.Closed)
            {
                var order = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (order != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderClosed(new OrderClosedEventArgsImpl(order));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Canceled)
            {
                var order = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (order != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderCanceled(new OrderCanceledEventArgsImpl(order));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Expired)
            {
                var order = orderCollection.GetOrderOrNull(eReport.OrderId);
                if (order != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderExpired(new OrderCanceledEventArgsImpl(order));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Modified)
            {
                var order = orderCollection.GetOrderOrNull(eReport.OrderId);
                var oldOrder = order.Clone();
                if (oldOrder != null && eReport.OrderCopy != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder, order));
                }
            }
            else if (eReport.ExecAction == OrderExecAction.Filled)
            {
                var order = orderCollection.GetOrderOrNull(eReport.OrderId);
                var oldOrder = order.Clone();
                if (order != null && eReport.OrderCopy != null)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder, order));
                }
            }
        }

        private void UpdateBalance(PluginBuilder builder, OrderExecReport eReport)
        {
            var acc = builder.Account;

            if (acc.Type == Api.AccountTypes.Gross || acc.Type == Api.AccountTypes.Net)
            {
                if (eReport.NewBalance != null && acc.Balance != eReport.NewBalance.Value)
                {
                    acc.Balance = eReport.NewBalance.Value;
                    acc.FireBalanceUpdateEvent();
                }
            }
            else if (acc.Type == Api.AccountTypes.Cash)
            {
                if (eReport.Assets != null)
                {
                    foreach (var asset in eReport.Assets)
                    {
                        acc.Assets.Update(new AssetEntity(asset.Volume, asset.Currency, currencies));
                        acc.Assets.FireModified(new AssetUpdateEventArgsImpl(new AssetEntity(asset.Volume, asset.Currency, currencies)));
                    }
                }
            }
        }
    }
}
