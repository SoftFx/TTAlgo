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
        private DynamicList<FakeAlgo> fakeList = new DynamicList<FakeAlgo>();

        public IObservableList<FakeAlgo> Algos { get; private set; }
        public AlgoListViewModel()
        {
            Algos = fakeList.DynamicWhere(f => f.Group != "Group 3").ToObservableList();

            UpdateLoop();
        }

        private async void UpdateLoop()
        {
            for (int i = 0; i < 23; i++)
            {
                await Task.Delay(1000);

                if (i % 3 == 0)
                    fakeList.Add(new FakeAlgo("FakeAlgo " + i, "Group 3"));
                else if (i % 2 == 0)
                    fakeList.Add(new FakeAlgo("FakeAlgo " + i, "Group 2"));
                else
                    fakeList.Add(new FakeAlgo("FakeAlgo " + i, "Group 1"));
            }

            fakeList.Clear();

            for (int i = 0; i < 23; i++)
            {
                await Task.Delay(1000);

                if (i % 3 == 0)
                    fakeList.Add(new FakeAlgo("FakeAlgo " + i, "Group 3"));
                else if (i % 2 == 0)
                    fakeList.Add(new FakeAlgo("FakeAlgo " + i, "Group 2"));
                else
                    fakeList.Add(new FakeAlgo("FakeAlgo " + i, "Group 1"));
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
