using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Backtester
{
    public class GeneticStrategy : OptimizationAlgorithm
    {
        private List<Params> _container = new List<Params>();
        private GenConfig _config;
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
            GenerateFirstSet();
        }

        private void GenerateFirstSet()
        {
            for (int i = 0; i < PopulationSize; ++i)
            {
                var gen = Set.Copy();

                gen.ForEach(u => u.Up(generator.GetInt(u.StepsCount)));

                _container.Add(gen);
                SendParams(gen, ref _receivedCount);
            }
        }

        public override long OnCaseCompleted(OptCaseReport report)
        {
            SetResult(report.Config.Id, report.MetricVal);

            if (++_receivedCount == PopulationSize)
            {
                do
                {
                    GenerateNewGeneration();

                    _receivedCount = 0;
                    _container.ForEach(u => SendParams(u, ref _receivedCount));

                    if (AlgoCompleate)
                        return 0;
                }
                while (_receivedCount == PopulationSize);
            }

            return _casesLeft;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateNewGeneration()
        {
            var gens = SurvivingGen();

            while (gens.Count < PopulationSize)
            {
                (int x, int y) = generator.GetRandomPair(gens.Count);

                gens.Add(RepropuctionGen(_container[x], _container[y]));
            }

            _container = gens;

            MutationGen();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<Params> SurvivingGen()
        {
            var generation = new List<Params>(PopulationSize);

            switch (_config.SurvivingMode)
            {
                case SurvivingMode.Roulette:
                    Roulette(generation);
                    break;
                case SurvivingMode.Uniform:
                    UniformSurviving(generation);
                    break;
                case SurvivingMode.SigmaClipping:
                    SigmaClipping(generation);
                    break;
            }

            return generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Params RepropuctionGen(Params first, Params second)
        {
            var crossover = generator.GetInt(1, first.Count);

            var reproduction = new Params();

            switch (_config.ReproductionMode)
            {
                case RepropuctionMode.CommonGen:
                    reproduction.AddRange(first.Take(crossover));
                    reproduction.AddRange(second.Skip(crossover));
                    break;
                case RepropuctionMode.IndividualGen:
                    reproduction.AddCopyRange(first.Take(crossover));
                    reproduction.AddCopyRange(second.Skip(crossover));
                    break;
            }

            return reproduction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MutationGen()
        {
            int i = 0;

            while (i++ < _config.CountMutationGen)
            {
                int index = generator.GetInt(_container.Count);
                int indexParam = generator.GetInt(_container[index].Count);
                var gen = _container[index][indexParam];

                switch (_config.MutationMode)
                {
                    case MutationMode.Step:
                        StepMutation(gen);
                        break;
                    case MutationMode.Jump:
                        JumpMutation(gen);
                        break;
                    //case MutationMode.AlphaGen:
                    //    AlphaMutation(index, indexParam);
                    //    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UniformSurviving(List<Params> gens)
        {
            gens.AddRange(_container.OrderBy(_ => generator.GetInt()).Take(SurvivingSize));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Roulette(List<Params> gens)
        {
            var normalizeDelta = -Math.Min(-0.0, _container.Min(u => u.Result.Value));

            GiveChance(gens, (x) => x + normalizeDelta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SigmaClipping(List<Params> gens)
        {
            var normalizeDelta = -Math.Min(-0.0, _container.Min(u => u.Result.Value));
            var mean = _container.Sum(u => u.Result.Value + normalizeDelta) / PopulationSize;
            var err = _container.Sum(u => Math.Pow(u.Result.Value + normalizeDelta - mean, 2)) / (PopulationSize - 1);
            err = 2 * Math.Sqrt(err);

            GiveChance(gens, (x) => 1.0 + (x + normalizeDelta - mean) / err);
        }

        private void GiveChance(List<Params> gens, Func<double, double> calculateF)
        {
            var sorted = _container.OrderByDescending(u => calculateF(u.Result.Value)).ToList();
            var totalResult = _container.Sum(u => calculateF(u.Result.Value));

            gens.AddRange(sorted.Take(SurvivingSize));

            for (int i = SurvivingSize; i < PopulationSize; ++i)
            {
                var chance = generator.GetPart();

                if ((calculateF(sorted[i].Result.Value) / totalResult).Gte(chance))
                    gens[(i - SurvivingSize) % gens.Count] = sorted[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StepMutation(AlgoParameter gen)
        {
            if (generator.GetBool())
                gen.Up();
            else
                gen.Down();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JumpMutation(AlgoParameter gen)
        {
            var jump = (int)((gen.Max - gen.Min) / gen.Step);

            if (generator.GetBool())
                gen.Up(jump);
            else
                gen.Down(jump);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void AlphaMutation(int index, int indexParam)
        //{
        //    _container[index][indexParam] = BestSet[indexParam].FullCopy();

        //    if (generator.GetBool())
        //        _container[index][indexParam].Up();
        //    else
        //        _container[index][indexParam].Down();
        //}
    }
}
