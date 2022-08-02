using Caliburn.Micro;
using NLog;
using System;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal interface IPersistableObject<T>
    {
        T GetCopyToSave();
    }


    internal interface IChangableObject
    {
        event System.Action Changed;
    }


    internal interface IPersistController
    {
        Task Close();
    }


    internal class ObjectPersistController<T> : IPersistController
        where T : class, IPersistableObject<T>, new()
    {
        private bool _isChanged;
        private bool _isSaving;
        private bool _isClosed;
        private Task _backgroundTask;
        private IObjectStorage _storage;
        private string _fileName;
        private Logger _logger;


        public T Value { get; private set; }

        public TimeSpan SaveDelay { get; set; }

        public bool CanSave => !_isClosed;


        public Func<string, bool> TryResolveFormatError = f => false;


        public ObjectPersistController(string fileName, IObjectStorage storage)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _storage = storage;
            _fileName = fileName;
            Load();
        }


        public void SetChanged()
        {
            Execute.OnUIThread(Save);
        }

        public Task Close()
        {
            Execute.OnUIThread(() => _isClosed = true);

            if (_backgroundTask != null)
                return _backgroundTask;
            else
                return Task.FromResult<T>(null);
        }

        public void Reopen()
        {
            if (Value is IChangableObject)
                ((IChangableObject)Value).Changed -= SetChanged;

            Value = null;

            Load();

            _isClosed = false;
        }

        public void SetFilename(string newFilename)
        {
            if (!_isClosed)
                throw new InvalidOperationException("Controller is not closed!");

            _fileName = newFilename;
        }

        public T LoadOnce(string fileName)
        {
            try
            {
                return _storage.Load<T>(fileName);
            }
            catch (Exception ex)
            {
                if (TryResolveFormatError(_fileName))
                {
                    return LoadOnce(fileName);
                }
                _logger.Error("ObjectPersistController.LoadOnce() FAILED " + ex.Message);
            }

            return default;
        }


        private void Load()
        {
            try
            {
                Value = _storage.Load<T>(_fileName);
            }
            catch (Exception ex)
            {
                if (TryResolveFormatError(_fileName))
                {
                    Load();
                    return;
                }
                _logger.Error("ObjectPersistController.Load() FAILED " + ex.Message);
            }

            if (Value == null)
                Value = new T();

            if (Value is IChangableObject)
                ((IChangableObject)Value).Changed += SetChanged;
        }

        private void Save()
        {
            if (_isClosed)
                throw new InvalidOperationException("Controller is closed!");

            if (_isSaving)
            {
                _isChanged = true;
            }

            _backgroundTask = SaveLoop();
        }

        private async Task SaveLoop()
        {
            _isSaving = true;
            T cloneToSave = Value.GetCopyToSave();
            _isChanged = false;

            do
            {
                try
                {
                    await Task.Factory.StartNew(() => _storage.Save(_fileName, cloneToSave));
                }
                catch (Exception ex)
                {
                    _logger.Error("ObjectPersistController.Save() FAILED " + ex.Message);
                }

                if (SaveDelay != TimeSpan.Zero)
                    await Task.Delay(SaveDelay);

                Execute.OnUIThread(() =>
                {
                    if (_isChanged)
                    {
                        _isChanged = false;
                        cloneToSave = Value.GetCopyToSave();
                    }
                    else
                        cloneToSave = null;
                });
            }
            while (cloneToSave != null);

            Execute.OnUIThread(() => _isSaving = false);
        }
    }
}
