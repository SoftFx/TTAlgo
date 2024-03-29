﻿using System.Collections.Generic;

namespace TickTrader.Algo.AppCommon.Update
{
    public class UpdateParams
    {
        public int AppTypeCode { get; set; }

        public string InstallPath { get; set; }

        public string UpdatePath { get; set; }

        public string FromVersion { get; set; }

        public string ToVersion { get; set; }


        public UpdateAppTypes AppType => (UpdateAppTypes)AppTypeCode;
    }

    public class UpdateState
    {
        public int Version { get; set; } = 1;

        public UpdateParams Params { get; set; } = new UpdateParams();

        public int StatusCode { get; set; }

        public int InitErrorCode { get; set; }

        public List<string> UpdateErrors { get; set; } = new List<string>();


        public UpdateStatusCodes Status => (UpdateStatusCodes)StatusCode;

        public UpdateErrorCodes InitError => (UpdateErrorCodes)InitErrorCode;

        public bool HasErrors => InitErrorCode != 0 || UpdateErrors.Count > 0;


        // Adding property setter will cause serialization to json
        // Don't want JsonIgnore attribute as it adds a dependency
        public void SetStatus(UpdateStatusCodes status) => StatusCode = (int)status;

        public void SetError(string errMsg)
        {
            SetStatus(UpdateStatusCodes.UpdateError);
            UpdateErrors.Add(errMsg);
        }
    }
}
