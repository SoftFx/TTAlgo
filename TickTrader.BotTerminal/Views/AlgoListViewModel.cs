using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class AlgoListViewModel : PropertyChangedBase
    {
        private DynamicList<int> fakeList = new DynamicList<int>();

        public IObservableListSource<FakeAlgo> Algos { get; private set; }

        public AlgoListViewModel()
        {
            Algos = fakeList.Select(GenerateFake).Where(f => f.Group != "Group 3").ToList().AsObservable();

            //UpdateLoop();
        }

        private FakeAlgo GenerateFake(int i)
        {
            if (i % 3 == 0)
                return new FakeAlgo("FakeAlgo " + i, "Group 3");
            else if (i % 2 == 0)
                return new FakeAlgo("FakeAlgo " + i, "Group 2");
            else
                return new FakeAlgo("FakeAlgo " + i, "Group 1");
        }

        private async void UpdateLoop()
        {
            for (int i = 0; i < 25; i++)
            {
                await Task.Delay(1000);

                fakeList.Values.Add(i);
            }

            await Task.Delay(1000);

            fakeList.Values.Clear();

            for (int i = 0; i < 25; i++)
            {
                await Task.Delay(1000);

                fakeList.Values.Add(i);
            }
        }
    }

    public class FakeAlgo
    {
        public FakeAlgo(string name, string group)
        {
            Name = name;
            Group = group;
        }
        public string Name { get; set; }
        public string Group { get; set; }
    }
}
