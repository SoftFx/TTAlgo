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
        private readonly Random _random = new Random();
        private GenConfig _config;
        private int _currentCaseNumber = 0;
        private int _pointer = 0;
        private long _casesLeft;
        private List<OptCaseConfig> _container = new List<OptCaseConfig>();


        public int SurvivingSize => _config?.CountSurvivingGen ?? 0;

        public int PopulationSize => _config?.CountGenInPopulations ?? 0;

        public override long CaseCount => 100;

        public override void Start(IBacktestQueue queue, int degreeOfParallelism)
        {
            _casesLeft = CaseCount;

            _config = new GenConfig()
            {
                CountGenInPopulations = 10,
                CountSurvivingGen = 5,
                CountMutationGen = 10,
                CountGeneration = 100,

                MutationMode = MutationMode.Step,
                SurvivingMode = SurvivingMode.Uniform,
                ReproductionMode = RepropuctionMode.IndividualGen,
            };

            GenerateFirstSet();

            _container.Foreach(u => queue.Enqueue(u));
        }

        public override long OnCaseCompleted(OptCaseReport report, IBacktestQueue queue)
        {
            _casesLeft--;

            if (_pointer < _container.Count)
                _container[_pointer].Result = report.MetricVal;
            else
                GenerateNewGeneration();

            if (_casesLeft > 0)
                queue.Enqueue(_container[_pointer++]);

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

        private List<OptCaseConfig> SurvivingGen()
        {
            var generation = new List<OptCaseConfig>(PopulationSize);

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
        private void RepropuctionGen(OptCaseConfig first, OptCaseConfig second, List<OptCaseConfig> gens)
        {
            var crossover = generator.GetInt(1, first.Size);

            var gen = new OptCaseConfig(++_currentCaseNumber);

            switch (_config.ReproductionMode)
            {
                //case RepropuctionMode.CommonGen:
                //    gen.AddRange(first.Take(crossover));
                //    gen.AddRange(second.Skip(crossover));
                //    break;
                case RepropuctionMode.IndividualGen:
                    foreach (var p in first.Params.Take(crossover))
                        gen.Add(p.Key, p.Value);
                    foreach (var p in second.Params.Skip(crossover))
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
            //int i = 0;

            //while (i++ < _config.CountMutationGen)
            //{
            //    int index = generator.GetInt(_container.Count);
            //    int indexParam = generator.GetInt(_container[index].Count);
            //    var gen = _container[index][indexParam];

            //    switch (_config.MutationMode)
            //    {
            //        case MutationMode.Step:
            //            StepMutation(gen);
            //            break;
            //        case MutationMode.Jump:
            //            JumpMutation(gen);
            //            break;
            //        case MutationMode.AlphaGen:
            //            AlphaMutation(index, indexParam);
            //            break;
            //    }
            //}
        }

        private void UniformSurviving(List<OptCaseConfig> gens)
        {
            var a = _container.OrderBy(_ => _random.Next()).ToList();

            for (int i = 0; i < SurvivingSize; ++i, ++_currentCaseNumber)
            {
                var gen = new OptCaseConfig(_currentCaseNumber);

                foreach (var v in a[i])
                    gen.Add(v.Key, v.Value);

                gens.Add(gen);
            }
        }

        private void GenerateFirstSet()
        {
            for (int i = 0; i < PopulationSize; ++i, ++_currentCaseNumber)
            {
                var gen = new OptCaseConfig(_currentCaseNumber);

                foreach (var v in Params)
                {
                    var value = v.Value;
                    gen.Add(v.Key, v.Value.GetParamValue(_random.Next(value.Size)));
                }

                _container.Add(gen);
            }
        }
    }

    public class GenConfig
    {
        public int CountGenInPopulations { get; set; }

        public int CountSurvivingGen { get; set; }

        public int CountMutationGen { get; set; }

        public int CountGeneration { get; set; }

        public MutationMode MutationMode { get; set; }

        public SurvivingMode SurvivingMode { get; set; }

        public RepropuctionMode ReproductionMode { get; set; }
    }

    public enum MutationMode
    {
        Step,
        Jump,
        AlphaGen,
    }

    public enum SurvivingMode
    {
        Roulette,
        Uniform,
        SigmaClipping,
    }

    public enum RepropuctionMode
    {
        IndividualGen,
        CommonGen,
    }
}
