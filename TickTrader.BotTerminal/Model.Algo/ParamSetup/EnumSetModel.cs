using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class EnumSetModel : ParamSeekSetModel
    {
        private readonly List<Item> _items;

        public EnumSetModel(List<string> values)
        {
            _items = values.Select(v => new Item(this, v)).ToList();
            UpdateDescription();
        }

        protected EnumSetModel(EnumSetModel src)
        {
            _items = src.Items.Select(i => new Item(this, i.Name, i.IsChecked.Value)).ToList();
            UpdateDescription();
        }

        public IReadOnlyList<Item> Items => _items;
        public override BoolVar IsValid { get; } = Var.Const(true);

        public override void Apply(Optimizer optimizer)
        {
            //optimizer.SetupParameterSeek(new setSe
        }

        public override ParamSeekSetModel Clone()
        {
            return new EnumSetModel(this);
        }

        protected override void Reset(object defaultValue)
        {
        }

        private void UpdateDescription()
        {
            if (_items != null)
                DescriptionProp.Value = string.Join(",", _items.Where(i => i.IsChecked.Value).Select(i => i.Name));
        }

        public class Item : EntityBase
        {
            //private ChecklistSeekSet _parent;

            public Item(EnumSetModel parent, string name, bool isChecked = false)
            {
                //_parent = parent;
                Name = name;
                IsChecked = AddBoolProperty(isChecked);

                TriggerOnChange(IsChecked, a =>
                {
                    if (a.New)
                        parent.SizeProp.Increase();
                    else
                        parent.SizeProp.Decrease();

                    parent.UpdateDescription();
                });
            }

            public string Name { get; }
            public BoolProperty IsChecked { get; }
        }
    }
}
