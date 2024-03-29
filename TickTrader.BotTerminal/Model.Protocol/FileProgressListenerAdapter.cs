﻿using TickTrader.Algo.Core.Lib;

using AlgoServerPublicApi = TickTrader.Algo.Server.PublicAPI;


namespace TickTrader.BotTerminal
{
    public class FileProgressListenerAdapter : AlgoServerPublicApi.IFileProgressListener
    {
        private IActionObserver _progressObserver;


        public long FileSize { get; }

        public long CurrentProgress { get; private set; }


        public FileProgressListenerAdapter(IActionObserver progressObserver, long fileSize)
        {
            _progressObserver = progressObserver;
            FileSize = fileSize;
        }


        public void Init(long initialProgress)
        {
            CurrentProgress = initialProgress;
            _progressObserver.StartProgress(0, 100);
            UpdateProgress();
        }

        public void IncrementProgress(long progressValue)
        {
            CurrentProgress += progressValue;
            UpdateProgress();
        }


        private void UpdateProgress()
        {
            _progressObserver.SetProgress(100.0 * CurrentProgress / FileSize);
        }
    }
}
