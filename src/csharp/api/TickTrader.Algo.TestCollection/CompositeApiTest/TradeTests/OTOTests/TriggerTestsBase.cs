using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed partial class OTOTests
    {
        private abstract class TriggerTestsBase
        {
            protected readonly OTOTests _base;


            protected TriggerTestsBase(OTOTests @base)
            {
                _base = @base;
            }


            protected abstract Task RunOpenTests(OrderBaseSet set);

            protected abstract Task RunModifyTests(OrderBaseSet set);

            protected abstract Task RunCancelTests(OrderBaseSet set);

            protected abstract Task RunOCOTests(OrderBaseSet set);

            protected abstract Task RunOpenRejectTests(OrderBaseSet set);

            protected abstract Task RunModifyRejectTests(OrderBaseSet set);



            protected abstract void UpdateOCOOrders(OrderStateTemplate order, OrderStateTemplate oco);


            internal async Task RunTests(OrderBaseSet set)
            {
                await RunOpenTests(set);
                await RunModifyTests(set);
                await RunCancelTests(set);

                await RunOpenRejectTests(set);
                await RunModifyRejectTests(set);

                if (StartOCO(set))
                    await RunOCOTestsEnviroment(set);
            }


            private async Task RunOCOTestsEnviroment(OrderBaseSet set)
            {
                _base._updateOCOTemplates += UpdateOCOOrders;

                await RunOCOTests(set);

                _base._updateOCOTemplates -= UpdateOCOOrders;
            }

            private bool StartOCO(OrderBaseSet set) => _base._useOCO && set.IsSupportedOCO;
        }
    }
}