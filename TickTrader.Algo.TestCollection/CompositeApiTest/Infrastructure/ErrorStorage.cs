using System.Collections.Generic;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ErrorStorage
    {
        private readonly List<string> _errorMessages;


        internal int ErrorsCount => _errorMessages.Count;


        internal ErrorStorage()
        {
            _errorMessages = new List<string>();
        }

        internal void AddError(string message)
        {
            _errorMessages.Add(message);
        }

        internal void ClearStorage()
        {
            _errorMessages.Clear();
        }
    }
}
