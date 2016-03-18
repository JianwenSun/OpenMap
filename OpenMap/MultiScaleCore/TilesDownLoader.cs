using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMap
{
    internal static class TilesDownloader
    {
        private static readonly object problematicTilesSynchronizer = new object();
        private static readonly object activeQueueSynchronizer = new object();
        private static readonly object startedDownloadersSynchronizer = new object();

        private static Queue<TileDownloader> activeQueue = new Queue<TileDownloader>();
        private static Queue<TileDownloader> problematicTilesQueue = new Queue<TileDownloader>();

        private static bool downloadingComplete;

        private static ManualResetEvent processRequest = new ManualResetEvent(true);
        private static Dictionary<TileDownloader, bool> startedDownloaders = new Dictionary<TileDownloader, bool>();

        static TilesDownloader()
        {
            Thread thread = new Thread(new ThreadStart(DownloaderThread));
            thread.IsBackground = true;
            thread.Start();
        }

        internal static bool DownloadingComplete
        {
            get
            {
                return TilesDownloader.downloadingComplete;
            }

            set
            {
                TilesDownloader.downloadingComplete = value;
            }
        }

        internal static void EnqueueDownload(TileDownloader downloader, bool active)
        {
            lock (startedDownloadersSynchronizer)
            {
                startedDownloaders.Remove(downloader);
            }

            if (active)
            {
                lock (activeQueueSynchronizer)
                {
                    activeQueue.Enqueue(downloader);
                }
            }
            else
            {
                lock (problematicTilesSynchronizer)
                {
                    downloadingComplete = false;
                    problematicTilesQueue.Enqueue(downloader);

                    Thread.Sleep(100);
                }
            }

            processRequest.Set();
        }

        internal static void RemoveDownloader(TileDownloader downloader)
        {
            lock (startedDownloadersSynchronizer)
            {
                startedDownloaders.Remove(downloader);
            }
        }

        private static void DownloaderThread()
        {
            while (true)
            {
                WaitHandle[] waitHandles = new WaitHandle[1];
                waitHandles[0] = processRequest;
                WaitHandle.WaitAll(waitHandles);

                Process();
            }
        }

        private static void Process()
        {
            bool maxDownloadersExceed = false;
            lock (startedDownloadersSynchronizer)
            {
               
            }

            if (QueueCountEmpty() || maxDownloadersExceed)
            {
                Thread.Sleep(50);
                return;
            }

            TileDownloader downloader = GetDownloader();
            if (downloader != null)
            {
                lock (startedDownloadersSynchronizer)
                {
                    if (!startedDownloaders.ContainsKey(downloader))
                    {
                        startedDownloaders.Add(downloader, true);
                    }
                }

                ThreadPool.QueueUserWorkItem(new WaitCallback(StartDownloadQueue), downloader);
            }
        }

        private static TileDownloader GetDownloader()
        {
            TileDownloader downloader = null;
            lock (activeQueueSynchronizer)
            {
                if (activeQueue.Count > 0)
                {
                    downloader = activeQueue.Dequeue();
                }
            }

            if (downloader == null)
            {
                lock (problematicTilesSynchronizer)
                {
                    if (problematicTilesQueue.Count > 0)
                    {
                        downloader = problematicTilesQueue.Dequeue();
                    }
                }
            }

            return downloader;
        }

        private static bool QueueCountEmpty()
        {
            bool empty = false;
            lock (problematicTilesSynchronizer)
            {
                lock (activeQueueSynchronizer)
                {
                    empty = problematicTilesQueue.Count == 0 && activeQueue.Count == 0;
                    if (empty)
                    {
                        processRequest.Reset();
                    }
                }
            }

            return empty;
        }

        private static void StartDownloadQueue(object downloaderObject)
        {
            var downloader = downloaderObject as TileDownloader;
            if (downloader != null)
            {
                lock (startedDownloadersSynchronizer)
                {
                    if (!startedDownloaders.ContainsKey(downloader))
                    {
                        startedDownloaders.Add(downloader, true);
                    }
                }

                downloader.StartReload();
            }
        }
    }
}
