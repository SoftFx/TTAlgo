﻿using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class AccountModelUpdateEntity : UpdateEntity<AccountModelEntity>
    {
    }


    internal static class AccountModelUpdateEntityExtensions
    {
        internal static AccountModelUpdateEntity ToEntity(this AccountModelUpdate update)
        {
            var res = new AccountModelUpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type) };
            res.Item.UpdateSelf(update.Item);
            return res;
        }

        internal static AccountModelUpdate ToMessage(this AccountModelUpdateEntity update)
        {
            var res = new AccountModelUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
            update.Item.UpdateModel(res.Item);
            return res;
        }

        internal static AccountModelUpdateEntity ToEntity(this AccountModelUpdate_1 update)
        {
            var res = new AccountModelUpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type) };
            res.Item.UpdateSelf(update.Item);
            return res;
        }

        internal static AccountModelUpdate_1 ToMessage_1(this AccountModelUpdateEntity update)
        {
            var res = new AccountModelUpdate_1(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
            update.Item.UpdateModel(res.Item);
            return res;
        }
    }
}