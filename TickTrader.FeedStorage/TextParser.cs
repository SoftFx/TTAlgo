namespace TickTrader.FeedStorage
{
    public class TextParser
    {
        private int _fIndex;
        private int _bIndex;
        private string _text;

        public TextParser(string text)
        {
            _text = text;
            _fIndex = 0;
            _bIndex = text.Length -1;
        }

        public string ReadNext(char delimiter)
        {
            if (_fIndex > _bIndex)
                return null;

            var start = _fIndex;

            while (_fIndex <= _bIndex && _text[_fIndex] != delimiter)
                _fIndex++;

            var result =  _text.Substring(start, _fIndex - start - 1);
            _fIndex++;
            return result;
        }

        public string ReadNextFromEnd(char delimiter)
        {
            if (_fIndex > _bIndex)
                return null;

            var end = _bIndex;

            while (_fIndex <= _bIndex && _text[_bIndex] != delimiter)
                _bIndex--;

            var result =  _text.Substring(_bIndex + 1, end - _bIndex);
            _bIndex--;
            return result;
        }

        public string GetRemainingText()
        {
            var result = _text.Substring(_fIndex, _bIndex - _fIndex + 1);
            return result;
        }
    }
}
