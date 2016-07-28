using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.EntityModel
{
    public class FieldGroup
    {
        private Dictionary<string, Field> fields = new Dictionary<string, Field>();
        private int invalidFieldsCount;

        public Field this[string key] { get { return fields[key]; } }
        public bool IsValid { get { return invalidFieldsCount == 0; } }

        public event Action<FieldGroup> IsValidChanged = delegate { };

        public Field<T> AddField<T>(string fieldName)
        {
            var newField = new Field<T>(fieldName);
            fields.Add(fieldName, newField);
            newField.ValueChanged += f => PropertyChanged(f.Name);
            newField.HasErrorChanged += Field_HasErrorChanged;
            return newField;
        }

        private void Field_HasErrorChanged(Field field)
        {
            if (field.HasError)
            {
                invalidFieldsCount++;
                if (invalidFieldsCount == 1)
                    IsValidChanged(this);
            }
            else
            {
                invalidFieldsCount--;
                if (invalidFieldsCount == 0)
                    IsValidChanged(this);
            }
        }

        public Field<T> GetField<T>(string name)
        {
            return (Field<T>)this[name];
        }

        public FieldProxy<TSrc> AddConverter<TSrc, TDst>(string propertyName, PropertyConverter<TSrc, TDst> converter)
        {
            return GetField<TDst>(propertyName).AddConverter<TSrc>(converter);
        }

        public event Action<string> PropertyChanged = delegate { };
    }
}
