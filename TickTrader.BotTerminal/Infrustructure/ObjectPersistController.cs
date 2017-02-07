using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal interface IPersistController
    {
        Task Close();
    }

    internal class ObjectPersistController<T> : IPersistController
        where T : class, IPersistableObject<T>, new()
    {
        private bool isChanged;
        private bool isSaving;
        private bool isClosed;
        private Task backgroundTask;
        private IObjectStorage storage;
        private string fileName;
        private Logger logger;

        public T Value { get; private set; }

        public ObjectPersistController(string fileName, IObjectStorage storage)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.storage = storage;
            this.fileName = fileName;
            Load();
        }

        public TimeSpan SaveDelay { get; set; }

        public void SetChanged()
        {
            Execute.OnUIThreadAsync(Save);
        }

        public Task Close()
        {
            Execute.OnUIThread(() => isClosed = true);

            if (backgroundTask != null)
                return backgroundTask;
            else
                return Task.FromResult<T>(null);
        }

        private void Load()
        {
            try
            {
                Value = storage.Load<T>(fileName);
            }
            catch (System.IO.FileNotFoundException) { /* normal case */ }
            catch (Exception ex)
            {
                logger.Error("ObjectPersistController.Load() FAILED " + ex);
            }

            if (Value == null)
                Value = new T();

            if (Value is IChangableObject)
                ((IChangableObject)Value).Changed += SetChanged;
        }

        private void Save()
        {
            if (isClosed)
                throw new InvalidOperationException("Controller is closed!");

            if (isSaving)
            {
                isChanged = true;
                return;
            }

            backgroundTask = SaveLoop();           
        }

        private async Task SaveLoop()
        {
            T cloneToSave = Value.GetCopyToSave();
            isChanged = false;

            do
            {
                try
                {
                    await Task.Factory.StartNew(() => storage.Save(fileName, cloneToSave));
                }
                catch (Exception ex)
                {
                    logger.Error("ObjectPersistController.Save() FAILED " + ex.Message);
                }

                if (SaveDelay != TimeSpan.Zero)
                    await Task.Delay(SaveDelay);

                await Execute.OnUIThreadAsync(() =>
                {
                    if (isChanged)
                    {
                        isChanged = false;
                        cloneToSave = Value.GetCopyToSave();
                    }
                    else
                        cloneToSave = null;
                });
            }
            while (cloneToSave != null);
        }
    }

    interface IPersistableObject<T>
    {
        T GetCopyToSave();
    }

    interface IChangableObject
    {
        event System.Action Changed;
    }
}
