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
        private List<Item> _items;
        private bool _initializing = true;
        private IntProperty _size;
        private BoolVar _isValid;

        public EnumSetModel(IEnumerable<string> values)
        {
            Init(values.Select(v => new Item(this, v)));
        }

        protected EnumSetModel(EnumSetModel src)
        {
            Init(src.Items.Select(i => new Item(this, i.Name, i.IsChecked.Value)));
        }

        private void Init(IEnumerable<Item> items)
        {
            _size = AddIntProperty();
            _items = items.ToList();
            _isValid = _size.Var > 0;
            _initializing = false;
        }

        public IReadOnlyList<Item> Items => _items;
        public override BoolVar IsValid => _isValid;
        public override string Description => string.Join(", ", Items.Where(i => i.IsChecked.Value).Select(i => i.Name));
        public override string EditorType => "Checklist";
        public override int Size => _size.Value;

        public override ParamSeekSet GetSeekSet()
        {
            return new EnumParamSet<string>(Items.Where(i => i.IsChecked.Value).Select(i => i.Name));
        }

        public override ParamSeekSetModel Clone()
        {
            return new EnumSetModel(this);
        }

        protected override void Reset(object defaultValue)
        {
            var enumDefValue = defaultValue as string;

            if (enumDefValue != null)
            {
                var defItem = Items.FirstOrDefault(i => i.Name == enumDefValue);
                if (defItem != null)
                {
                    defItem.IsChecked.Set();
                    return;
                }
            }

            if (Items.Count > 0)
                Items[0].IsChecked.Set();
        }

        public class Item : EntityBase
        {
            public Item(EnumSetModel parent, string name, bool isChecked = false)
            {
                //_parent = parent;
                Name = name;
                IsChecked = AddBoolProperty(isChecked);

                TriggerOnChange(IsChecked, a =>
                {
                    if (a.New)
                        parent._size.Increase();
                    else if (!parent._initializing)
                        parent._size.Decrease();
                });
            }

            public string Name { get; }
            public BoolProperty IsChecked { get; }
        }
    }
}
