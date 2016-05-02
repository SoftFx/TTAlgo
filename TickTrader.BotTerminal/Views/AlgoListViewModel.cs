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
        public ObservableCollection<FakeAlgo> Algos { get; private set; }
        public AlgoListViewModel()
        {
            List<FakeAlgo> fakes = new List<FakeAlgo>();

            Enumerable.Range(0, 23).ToList().ForEach(i =>
             {
                 if (i % 3 == 0)
                     fakes.Add(new FakeAlgo("FakeAlgo " + i, "Group 3"));
                 else if(i%2 ==0)
                     fakes.Add(new FakeAlgo("FakeAlgo " + i, "Group 2"));
                 else
                     fakes.Add(new FakeAlgo("FakeAlgo " + i, "Group 1"));
             });

            Algos = new ObservableCollection<FakeAlgo>(fakes);
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
