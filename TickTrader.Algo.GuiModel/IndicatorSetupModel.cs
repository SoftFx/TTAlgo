using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.GuiModel
{
    public class IndicatorSetupModel : ObservableObject
    {
        private List<AlgoParameter> parameters;

        public IndicatorSetupModel(AlgoInfo descriptor)
        {
            this.Descriptor = descriptor;

            parameters = descriptor.Parameters.Select(AlgoParameter.Create).ToList();
            parameters.ForEach(p => p.ErrorChanged += s => Validate());
            Validate();
            
            //Inputs = new List<AlgoInputSetup>();
            //Outputs = new List<AlgoOutputSetup>();
        }

        public void Reset()
        {
            foreach (var p in Parameters)
                p.Reset();
        }

        public void CopyParametersFrom(IndicatorSetupModel otherSetup)
        {
        }

        private void Validate()
        {
            IsValid = Descriptor.Error == null && Parameters.All(p => !p.HasError);
            ValidityChanged();
        }

        //public IList<AlgoInputSetup> Inputs { get; private set; }
        //public IList<AlgoOutputSetup> Outputs { get; private set; }
        public IEnumerable<AlgoParameter> Parameters { get { return parameters; } }
        public AlgoInfo Descriptor { get; private set; }

        public bool IsValid {get; private set;}
        public event Action ValidityChanged = delegate { };

        //public AlgoParameter GetParameter(string id)
        //{
        //    return Parameters.FirstOrDefault(sp => id == sp.Id);
        //}
    }
}
