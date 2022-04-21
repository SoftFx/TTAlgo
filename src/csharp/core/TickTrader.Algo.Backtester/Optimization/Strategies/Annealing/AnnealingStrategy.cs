using System;
using System.Runtime.CompilerServices;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.BacktesterApi;

namespace TickTrader.Algo.Backtester
{
    public class AnnealingStrategy : OptimizationAlgorithm
    {
        private const double Error = 1e-5;

        private readonly AnnConfig _config;
        private readonly long _caseCount;
        private double _currentTemp;
        private int _currentStep = 1;
        private int _currentStepInnerCycle = 0;

        public override long CaseCount => _caseCount;

        public AnnealingStrategy(AnnConfig config, int paramsCount)
        {
            _config = config;
            _currentTemp = _config.InitialTemperature;

            var caseCount = 0.0;

            switch (_config.MethodForT)
            {
                case SimulatedAnnealingMethod.Custom:
                    if (_config.DecreaseConditionMode == DecreaseConditionMode.ImproveAnswer)
                        caseCount = _config.InitialTemperature / _config.DeltaTemparature;
                    else
                    if (_config.DecreaseConditionMode == DecreaseConditionMode.FullCycle)
                        caseCount = _config.InitialTemperature * _config.InnerIterationCount / _config.DeltaTemparature;
                    break;
                case SimulatedAnnealingMethod.Boltzmann:
                    caseCount = Math.Exp(_config.InitialTemperature / Error - 1.0);
                    break;
                case SimulatedAnnealingMethod.Cauchy:
                    caseCount = _config.InitialTemperature / Error;
                    break;
                case SimulatedAnnealingMethod.VeryFast:
                    caseCount = Math.Pow(-Math.Log(Error / _config.InitialTemperature) / _config.VeryFastTempDecrement, paramsCount);
                    break;
            }

            _caseCount = _config.OutherIterationCount.HasValue
                ? (int)Math.Max(double.IsInfinity(caseCount) ? _config.OutherIterationCount.Value : caseCount, _config.OutherIterationCount.Value)
                : (int)caseCount;
        }

        public override void Start(IBacktestQueue queue, int degreeOfParallelism)
        {
            SetQueue(queue);
            SendParams(Set, ref _currentStep);
        }

        public override long OnCaseCompleted(OptCaseReport report)
        {
            SetResult(report.Config.Id, report.MetricVal);

            if (Set == BestSet || AcceptState())
            {
                if (Set != BestSet)
                    BestSet = Set.Copy();

                if (_config.DecreaseConditionMode == DecreaseConditionMode.ImproveAnswer)
                    DecreaseTemperature();
            }

            do
            {
                if (_currentTemp <= Error || _currentStep > _config.OutherIterationCount)
                    return 0;

                if (_config.DecreaseConditionMode == DecreaseConditionMode.FullCycle && _currentStepInnerCycle++ >= _config.InnerIterationCount)
                {
                    _currentStepInnerCycle = 0;
                    DecreaseTemperature();
                }

                int send = 0;
                CalculateSet();

                SendParams(Set, ref send);

                if (send == 0)
                    return _casesLeft;

                if (AlgoCompleate)
                    return 0;
            }
            while (true);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateSet()
        {
            const int maxIteration = 10;

            foreach (var state in Set)
            {
                var iteration = 0;

                while (iteration++ < maxIteration)
                {
                    double up = 0;

                    switch (_config.MethodForG)
                    {
                        case SimulatedAnnealingMethod.Custom:
                            up = (Math.Pow(1 + 1 / _currentTemp, 2 * generator.GetPart() - 1) - 1) * _currentTemp;
                            break;
                        case SimulatedAnnealingMethod.Boltzmann:
                            up = generator.GetNormalNumber(state.Current, _currentTemp);
                            break;
                        case SimulatedAnnealingMethod.Cauchy:
                            up = _currentTemp * generator.GetCauchyNumber();
                            break;
                        case SimulatedAnnealingMethod.VeryFast:
                            var alpha = generator.GetPart();
                            up = (Math.Pow(1 + 1 / _currentTemp, 2 * alpha - 1) - 1) * _currentTemp * Math.Sign(alpha - 0.5);
                            break;
                    }

                    if (double.IsNaN(up))
                        continue;

                    var current = state.Current;

                    state.Up(GetFirstNumberNotZero(up / state.Step));

                    if (_config.MethodForG == SimulatedAnnealingMethod.VeryFast && current == state.Current)
                        continue;

                    break;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetFirstNumberNotZero(double val)
        {
            while (Math.Abs(val).Lt(1.0))
                val *= 10;

            return (int)Math.Round(val);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecreaseTemperature()
        {
            switch (_config.MethodForT)
            {
                case SimulatedAnnealingMethod.Custom:
                    _currentTemp -= _config.DeltaTemparature;
                    break;
                case SimulatedAnnealingMethod.Boltzmann:
                    _currentTemp = _config.InitialTemperature / Math.Log(1 + _currentStep);
                    break;
                case SimulatedAnnealingMethod.Cauchy:
                    _currentTemp = _config.InitialTemperature / _currentStep;
                    break;
                case SimulatedAnnealingMethod.VeryFast:
                    _currentTemp = _config.InitialTemperature *
                                   Math.Exp(-_config.VeryFastTempDecrement * Math.Pow(_currentStep, 1.0 / Set.Count));
                    break;
            }

            _currentStep++;
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AcceptState()
        {
            //P = exp(-dE / T);
            if (!BestSet.Result.HasValue)
                return true;
            else
                return Math.Exp((double)(BestSet.Result - Set.Result) / _currentTemp) >= generator.GetPart(); // h = e^(-deltaE/T) - при поиске минимума
        }
    }
}
