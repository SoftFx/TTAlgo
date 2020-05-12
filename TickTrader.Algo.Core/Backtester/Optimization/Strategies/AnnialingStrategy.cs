using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AnnialingStrategy : ParamSeekStrategy
    {
        private AnnConfig _config;
        private Params _currentBestSet;
        private Params _currentSet;
        private int _currentStepInnerCycle = 0;
        public double _currentTemp;
        public bool _updateSet;
        private int _currentStep;

        private int _currentCaseNumber = 0;
        private long _casesLeft;

        public override long CaseCount => 100;

        public override void Start(IBacktestQueue queue, int degreeOfParallelism)
        {
            _casesLeft = CaseCount;

            _config = new AnnConfig()
            {
                InitialTemperature = 100, // boltzman: 1
                DeltaTemparature = 0.1,
                InnerIterationCount = 1,
                OutherIterationCount = 1000000,
                VeryFastTempDecrement = 0.1,
                DecreaseConditionMode = DecreaseConditionMode.ImproveAnswer,
                MethodForT = SimulatedAnnialingMethod.VeryFast,
                MethodForG = SimulatedAnnialingMethod.VeryFast,
            };

            _currentStepInnerCycle = 0;
            _currentStep = 1;

            _currentBestSet = new Params(0);

            foreach (var p in Params)
                _currentBestSet.Add(p.Key, p.Value);

            _currentSet = _currentBestSet;
            queue.Enqueue(_currentBestSet);

            _currentTemp = _config.InitialTemperature;
        }

        public override long OnCaseCompleted(OptCaseReport report, IBacktestQueue queue)
        {
            _casesLeft--;

            if (!_currentBestSet.Result.HasValue || _currentBestSet.Result < report.MetricVal || AcceptState())
            {
                _updateSet = true;
                _currentBestSet = (Params)_currentSet.Clone();
                _currentBestSet.Result = report.MetricVal;
            }

            if (_currentTemp <= 1e-5 || _currentStep > _config.OutherIterationCount)
                return 0;

            if (_config.DecreaseConditionMode == DecreaseConditionMode.FullCycle && _currentStepInnerCycle++ >= _config.InnerIterationCount)
            {
                _currentStepInnerCycle = 0;
                DecreaseTemperature();
            }

            if (_config.DecreaseConditionMode == DecreaseConditionMode.ImproveAnswer && _updateSet)
                DecreaseTemperature();

            CalculateSet();

            queue.Enqueue(_currentSet);

            return _casesLeft;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateSet()
        {
            _updateSet = false;

            var set = new Params(++_currentCaseNumber);

            foreach (var state in _currentSet)
            {
                var iteration = 0;

                while (iteration++ < 10)
                {
                    if (state.Value is ConstParam)
                    {
                        set.Add(state.Key, state.Value);
                        break;
                    }

                    double up = 0;

                    switch (_config.MethodForG)
                    {
                        case SimulatedAnnialingMethod.Custom:
                            up = (Math.Pow(1 + 1 / _currentTemp, 2 * generator.GetPart() - 1) - 1) * _currentTemp;
                            break;
                        case SimulatedAnnialingMethod.Boltzmann:
                            up = _currentTemp * generator.GetNormalNumber();
                            break;
                        case SimulatedAnnialingMethod.Cauchy:
                            up = _currentTemp * generator.GetCauchyNumber();
                            break;
                        case SimulatedAnnialingMethod.VeryFast:
                            var alpha = generator.GetPart();
                            up = (Math.Pow(1 + 1 / _currentTemp, 2 * alpha - 1) - 1) * _currentTemp; // * Math.Sign(alpha - 0.5)
                            break;
                    }

                    if (double.IsNaN(up))
                        continue;

                    var current = state.Value;

                    var parameter = (ParamSeekSet)state.Value;

                    var newValue = parameter.GetParamValue((int)(up / parameter.Size));

                    if (_config.MethodForG == SimulatedAnnialingMethod.VeryFast && current == newValue)
                        continue;

                    set.Add(state.Key, newValue);

                    break;
                }
            }

            _currentSet = set;
        }

        private void DecreaseTemperature()
        {
            switch (_config.MethodForT)
            {
                case SimulatedAnnialingMethod.Custom:
                    _currentTemp -= _config.DeltaTemparature;
                    break;
                case SimulatedAnnialingMethod.Boltzmann:
                    _currentTemp = _config.InitialTemperature / Math.Log(1 + _currentStep);
                    break;
                case SimulatedAnnialingMethod.Cauchy:
                    _currentTemp = _config.InitialTemperature / _currentStep;
                    break;
                case SimulatedAnnialingMethod.VeryFast:
                    _currentTemp = _config.InitialTemperature * Math.Exp(-_config.VeryFastTempDecrement * Math.Pow(_currentStep, 1.0 / _currentSet.Count));
                    break;
            }

            _currentStep++;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AcceptState()
        {
            //P = exp(-dE / T);
            if (!_currentBestSet.Result.HasValue)
                return true;
            else
                return Math.Exp((double)(_currentBestSet.Result - _currentSet.Result) / _currentTemp) >= generator.GetPart(); // h = e^(-deltaE/T) - при поиске минимума
        }
    }

    public class AnnConfig
    {
        public double InitialTemperature { get; set; }

        public double DeltaTemparature { get; set; }

        public long InnerIterationCount { get; set; }

        public long OutherIterationCount { get; set; }

        public double VeryFastTempDecrement { get; set; }

        public DecreaseConditionMode DecreaseConditionMode { get; set; }

        public SimulatedAnnialingMethod MethodForT { get; set; }

        public SimulatedAnnialingMethod MethodForG { get; set; }
    }

    public enum DecreaseConditionMode
    {
        ImproveAnswer,
        FullCycle,
    }

    public enum SimulatedAnnialingMethod
    {
        Custom,
        Boltzmann,
        Cauchy,
        VeryFast,
    }
}
