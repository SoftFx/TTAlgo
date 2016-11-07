using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace NLP_Ind
{
    internal class RemoteCalculation
    {
        string ClientId {get;set;}
        HttpClient httpClient;
        readonly string BasicAddress;
        const string InitSuffix = "/init?clientId={0}&symbol={1}";
        const string QuatilesSuffix = "/estimate?clientId={0}&pa={1}&pd={2}&ca={3}&cd={4}";

        public RemoteCalculation(string clientId, string basicAddress = "http://10.2.18.126:8000") 
        {
            this.ClientId = clientId;
            httpClient = new HttpClient();
            this.BasicAddress = basicAddress;

        }

        public int Init(string symbol, int sensitivity, double[] requestedQuantiles)
        {
            string response = httpClient.GetStringAsync(BasicAddress + string.Format(InitSuffix,ClientId, symbol) ).Result;
            response.Replace("[", "");
            response.Replace("]", "");
            return int.Parse(response);
        }

        public double[] RequestQuantiles(int pa, int pd, int ca, int cd, bool cTrendUp)
        {
            try
            {

                string response = httpClient.GetStringAsync(BasicAddress + string.Format(QuatilesSuffix,
                    ClientId, cTrendUp ? -pa : pa, pd, cTrendUp ? ca : -ca, cd)).Result;
                response = response.Replace("[", "").Replace("]", "");
                return response.Split(new char[] { ',' }).Select(p => double.Parse(p)).ToArray();
            }
            catch(Exception exc)
            {
                return null;
            }
        }
    }
}
