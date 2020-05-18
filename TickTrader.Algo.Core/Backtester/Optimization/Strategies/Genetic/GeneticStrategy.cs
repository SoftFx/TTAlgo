using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class GeneticStrategy : ParamSeekStrategy
    {
        private GenConfig _config;
        private int _currentCaseNumber = 0;
        private int _pointer = 0;
        //private long _casesLeft;
        private List<Params> _container = new List<Params>();
        private int _receivedCount = 0;

        public int SurvivingSize => _config?.CountSurvivingGen ?? 0;

        public int PopulationSize => _config?.CountGenInPopulations ?? 0;

        public override long CaseCount => _config.CountGenInPopulations * _config.CountGeneration;


        public GeneticStrategy(GenConfig config)
        {
            _config = config;
        }

        public override void Start(IBacktestQueue queue, int degreeOfParallelism)
        {
            SetQueue(queue);
            //_casesLeft = CaseCount;

            GenerateFirstSet();
        }

        public override long OnCaseCompleted(OptCaseReport report)
        {
            if (++_receivedCount == PopulationSize)
            {
                do
                {
                    GenerateNewGeneration();

                    _receivedCount = 0;

                    foreach (var g in _container)
                    {
                        if (!SendParams(g))
                            _receivedCount++;
                    }
                }
                while (_receivedCount == PopulationSize);
            }
            //_casesLeft--;

            //if (_pointer < _container.Count)
            //    //_container[_pointer].Result = report.MetricVal;
            //else


            //if (_casesLeft > 0)
            //    ;
            //    //queue.Enqueue(_container[_pointer++]);

            //return SendParams(_container[_pointer++]);
            return _casesLeft;
        }

        private void GenerateNewGeneration()
        {
            var gens = SurvivingGen();

            while (gens.Count < PopulationSize)
            {
                (int x, int y) = generator.GetRandomPair(gens.Count);

                RepropuctionGen(_container[x], _container[y], gens);
            }

            _pointer = 0;
            _container = gens;

            MutationGen();
        }

        private List<Params> SurvivingGen()
        {
            var generation = new List<Params>(PopulationSize);

            switch (_config.SurvivingMode)
            {
                //case SurvivingMode.Roulette:
                //    Roulette(generation);
                //    break;
                case SurvivingMode.Uniform:
                    UniformSurviving(generation);
                    break;
                    //case SurvivingMode.SigmaClipping:
                    //    SigmaClipping(generation);
                    //    break;
            }

            return generation;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RepropuctionGen(Params first, Params second, List<Params> gens)
        {
            var crossover = generator.GetInt(1, first.Count);

            var gen = new Params(++_currentCaseNumber);

            switch (_config.ReproductionMode)
            {
                //case RepropuctionMode.CommonGen:
                //    gen.AddRange(first.Take(crossover));
                //    gen.AddRange(second.Skip(crossover));
                //    break;
                case RepropuctionMode.IndividualGen:
                    foreach (var p in first.Parameters.Take(crossover))
                        gen.Add(p.Key, p.Value);
                    foreach (var p in second.Parameters.Skip(crossover))
                        gen.Add(p.Key, p.Value);
                    //gen.AddCopyRange(first.Take(crossover));
                    //gen.AddCopyRange(second.Skip(crossover));
                    break;
            }

            gens.Add(gen);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MutationGen()
        {
            int i = 0;

            while (i++ < _config.CountMutationGen)
            {
                int index = generator.GetInt(_container.Count);
                int indexParam = generator.GetInt(_container[index].Count);
                //var gen = _container[index][indexParam];
                string key = _container[index][indexParam];

                switch (_config.MutationMode)
                {
                    case MutationMode.Step:
                        StepMutation(index, key);
                        break;
                        //case MutationMode.Jump:
                        //    JumpMutation(gen);
                        //    break;
                        //case MutationMode.AlphaGen:
                        //    AlphaMutation(index, indexParam);
                        //    break;
                }
            }
        }

        private void UniformSurviving(List<Params> gens)
        {
            var a = _container.OrderBy(_ => generator.GetInt()).ToList();

            for (int i = 0; i < SurvivingSize; ++i, ++_currentCaseNumber)
            {
                var gen = new Params(_currentCaseNumber);

                foreach (var v in a[i])
                    gen.Add(v.Key, v.Value);

                gens.Add(gen);
            }
        }

        private void GenerateFirstSet()
        {
            for (int i = 0; i < PopulationSize; ++i, ++_currentCaseNumber)
            {
                var gen = new Params(_currentCaseNumber);

                foreach (var v in InitParams)
                {
                    var value = v.Value;
                    gen.Add(v.Key, v.Value.GetParamValue(generator.GetInt(value.Size)));
                }

                SendParams(gen);
                _container.Add(gen);
            }

            //_container.Foreach(u => );
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StepMutation(int index, string key)
        {
            //_container[index].Parameters[key] = Params[key].GetParamValue(generator.GetBool() ? 1 : -1);
            _container[index].Parameters[key] = InitParams[key].GetParamValue(generator.GetInt(InitParams[key].Size));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void JumpMutation(Parameter gen)
        //{
        //    var jump = (int)((gen.Max - gen.Min) / gen.Step);

        //    if (generator.GetBool())
        //        gen.Up(jump);
        //    else
        //        gen.Down(jump);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void AlphaMutation(int index, int indexParam) => _container[index][indexParam] = BestSet[indexParam].FullCopy();
    }
}
