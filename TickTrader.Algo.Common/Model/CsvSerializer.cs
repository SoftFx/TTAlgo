using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class CsvSerializer<TEntity>
    {
        private List<IColumn> _columns = new List<IColumn>();

        public void AddColumn(string name, Func<TEntity, string> get, Action<TEntity, string> set = null)
        {
            AddColumn(name, new StringFieldSerializer(), get, set);
        }

        public void AddEnumColumn<T>(string name, Func<TEntity, T> get, Action<TEntity, T> set = null)
            where T : struct
        {
            AddColumn(name, new EnumFieldSerializer<T>(), get, set);
        }

        public void AddColumn(string name, Func<TEntity, DateTime> get, Action<TEntity, DateTime> set = null)
        {
            AddColumn(name, new DateTimeFieldSerializer(), get, set);
        }

        public void AddColumn(string name, Func<TEntity, int> get, Action<TEntity, int> set = null)
        {
            AddColumn(name, new Int32FieldSerializer(), get, set);
        }

        public void AddColumn(string name, Func<TEntity, double> get, Action<TEntity, double> set = null)
        {
            AddColumn(name, new DoubleFieldSerializer(), get, set);
        }

        public void AddColumn(string name, Func<TEntity, double?> get, Action<TEntity, double?> set = null)
        {
            AddColumn(name, new NullableDoubleFieldSerializer(), get, set);
        }

        public void AddColumn<T>(string name, IFieldSerializer<T> fieldSerializer, Func<TEntity, T> get, Action<TEntity, T> set = null)
        {
            _columns.Add(new Column<T>(name, fieldSerializer, get, set));
        }

        public void Serialize(IEnumerable<TEntity> entities, Stream toStream)
        {
            using (var writer = new StreamWriter(toStream))
            {
                WriteHeader(writer);

                foreach (var rep in entities)
                    WriteRow(writer, rep);
            }
        }

        public void Serialize(IEnumerable<TEntity> entities, Stream toStream, Action<long> progressCallback)
        {
            long i = 0;

            using (var writer = new StreamWriter(toStream))
            {
                WriteHeader(writer);

                foreach (var rep in entities)
                    WriteRow(writer, rep);

                if ((++i) % 10 == 0)
                    progressCallback(i);
            }

            progressCallback(i);
        }

        private void WriteHeader(TextWriter writer)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (i != 0)
                    writer.Write(",");

                writer.Write(_columns[i].Name);
            }
        }

        private void WriteRow(TextWriter writer, TEntity entity)
        {
            writer.WriteLine();

            for (int i = 0; i < _columns.Count; i++)
            {
                if (i != 0)
                    writer.Write(",");

                var field = _columns[i].Serialize(entity);
                writer.Write(field);
            }
        }

        private interface IColumn
        {
            string Name { get; }
            string Serialize(TEntity report);
        }

        private class Column<T> : IColumn
        {
            private Func<TEntity, T> _getter;
            private Action<TEntity, T> _setter;
            private IFieldSerializer<T> _typeSerializer;

            public Column(string name, IFieldSerializer<T> serializer, Func<TEntity, T> getter, Action<TEntity, T> setter)
            {
                Name = name;
                _getter = getter;
                _setter = setter;
                _typeSerializer = serializer;
            }

            public string Name { get; }

            public string Serialize(TEntity entity)
            {
                var fieldVal = _getter(entity);
                return _typeSerializer.ToString(fieldVal);
            }
        }

        public interface IFieldSerializer<T>
        {
            string ToString(T val);
            bool Parse(string str, out T val);
        }

        private class StringFieldSerializer : IFieldSerializer<string>
        {
            public bool Parse(string str, out string val)
            {
                val = str;
                return true;
            }

            public string ToString(string val)
            {
                return val;
            }
        }

        private class DoubleFieldSerializer : IFieldSerializer<double>
        {
            public bool Parse(string str, out double val)
            {
                return double.TryParse(str, NumberStyles.None, CultureInfo.InvariantCulture, out val);
            }

            public string ToString(double val)
            {
                return val.ToString("G", CultureInfo.InvariantCulture);
            }
        }

        private class NullableDoubleFieldSerializer : IFieldSerializer<double?>
        {
            public bool Parse(string str, out double? val)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    val = null;
                    return true;
                }

                double parsedVal;
                var result = double.TryParse(str, NumberStyles.None, CultureInfo.InvariantCulture, out parsedVal);
                val = parsedVal;
                return result;
            }

            public string ToString(double? val)
            {
                if (val == null)
                    return string.Empty;
                return val.Value.ToString("G", CultureInfo.InvariantCulture);
            }
        }

        private class Int32FieldSerializer : IFieldSerializer<int>
        {
            public bool Parse(string str, out int val)
            {
                return int.TryParse(str, NumberStyles.None, CultureInfo.InvariantCulture, out val);
            }

            public string ToString(int val)
            {
                return val.ToString("N", CultureInfo.InvariantCulture);
            }
        }

        private class EnumFieldSerializer<T> : IFieldSerializer<T>
            where T : struct
        {
            public bool Parse(string str, out T val)
            {
                return Enum.TryParse<T>(str, true, out val);
            }

            public string ToString(T val)
            {
                return val.ToString();
            }
        }

        private class DateTimeFieldSerializer : IFieldSerializer<DateTime>
        {
            public bool Parse(string str, out DateTime val)
            {
                return DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out val);
            }

            public string ToString(DateTime val)
            {
                return InvariantFormat.CsvFormat(val);
            }
        }
    }
}
