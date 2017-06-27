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


        public ObjectPersistController(string fileName, IObjectStorage storage)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _storage = storage;
            _fileName = fileName;
            Load();
        }


        public void SetChanged()
        {
            Execute.OnUIThreadAsync(Save);
        }

        public Task Close()
        {
            Execute.OnUIThread(() => _isClosed = true);

            if (_backgroundTask != null)
                return _backgroundTask;
            else
                return Task.FromResult<T>(null);
        }


        private void Load()
        {
            try
            {
                Value = _storage.Load<T>(_fileName);
            }
            catch (System.IO.FileNotFoundException) { /* normal case */ }
            catch (Exception ex)
            {
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
                return;
            }

            _backgroundTask = SaveLoop();           
        }

        private async Task SaveLoop()
        {
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

                await Execute.OnUIThreadAsync(() =>
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
        }
    }
}
