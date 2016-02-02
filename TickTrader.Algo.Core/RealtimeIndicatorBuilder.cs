using StateMachinarium;
using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class RealtimeIndicatorBuilder<TRow>
    {
        private enum States { Created, Initializing, Building, Updating, Idle, Stopping, Disposed }
        private enum Events { Started, DoneInit, DoneBuilding, DoneUpdating }

        private StateMachine<States> stateControl = new StateMachine<States>();
        private AlgoContext<TRow> context = new AlgoContext<TRow>();
        private bool stopRequested;
        private IndicatorProxy insatnceProxy;
        private Func<IAlgoContext, IndicatorProxy> factory;
        private int readIndex;
        private int availableIndex;
        private bool isUpdated;

        public RealtimeIndicatorBuilder(Func<IAlgoContext, IndicatorProxy> factory, IObservableDataReader<TRow> reader, IAlgoDataWriter<TRow> writer)
        {
            this.factory = factory;

            reader.Updated += Reader_Updated;

            context.Reader = reader;
            context.Writer = writer;

            stateControl.AddTransition(States.Created, Events.Started, States.Initializing);
            stateControl.AddTransition(States.Initializing, Events.DoneInit, States.Building);
            stateControl.AddTransition(States.Building, () => stopRequested, States.Stopping);
            stateControl.AddTransition(States.Building, Events.DoneBuilding, States.Idle);
            stateControl.AddTransition(States.Idle, () => stopRequested, States.Disposed);
            stateControl.AddTransition(States.Idle, () => readIndex < availableIndex, States.Building);
            stateControl.AddTransition(States.Idle, () => isUpdated, States.Updating);

            stateControl.OnEnter(States.Initializing, () => Task.Factory.StartNew(Init));
            stateControl.OnEnter(States.Building, Build);
            stateControl.OnEnter(States.Updating, Update);
        }

        public void Start()
        {
            stateControl.PushEvent(Events.Started);
        }

        public Task Stop()
        {
            stateControl.ModifyConditions(() => stopRequested = true);
            return stateControl.AsyncWait(States.Disposed);
        }

        private async void Build()
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        int count = 0;

                        stateControl.ModifyConditions(()=>
                        {
                            count = context.Read();
                            readIndex += count;
                        });

                        if (count == 0)
                            break;

                        for (int i = 0; i < count; i++)
                        {
                            context.MoveNext();
                            insatnceProxy.InvokeCalculate();
                        }
                    }
                });

            }
            catch (Exception)
            {
            }
        }

        private async void Update()
        {
            isUpdated = false;

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    context.ReRead();
                    insatnceProxy.InvokeCalculate();
                });
            }
            catch (Exception)
            {
            }
        }

        private void Init()
        {
            try
            {
                insatnceProxy = factory(context);

                context.Init();
            }
            catch (Exception)
            {
            }

            stateControl.PushEvent(Events.DoneInit);
        }

        private void Reader_Updated(int index)
        {
            stateControl.ModifyConditions(() =>
            {
                if (availableIndex < index)
                    availableIndex = index;
                if (readIndex == index)
                    isUpdated = true;
            });
        }
    }
}
