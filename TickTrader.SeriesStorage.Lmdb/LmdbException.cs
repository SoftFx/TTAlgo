using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.Lmdb
{
    public static class LmdbException
    {
        public static SeriesDbException Convert(Exception ex)
        {
            var lmdbEx = ex as LightningDB.LightningException;
            if (lmdbEx != null)
            {
                var innerLmdbEx = lmdbEx.InnerException as LightningDB.LightningException;
                if (innerLmdbEx != null && innerLmdbEx.StatusCode == 2)
                    return new DbMissingException(innerLmdbEx.Message);
            }
            return new SeriesDbException(ex.Message, SeriesDbErrorCodes.Unknown, ex);
        }
    }
}
