using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;

namespace OpenMap
{
	public partial class FileCacheBase
	{
        private long cacheSize = 0;
		private ManualResetEvent processRequest = new ManualResetEvent(false);
		private Queue<CacheRecord> queue = new Queue<CacheRecord>();
		private object queueSynchronizer = new object();
		private List<ICacheRecordMetadata> cacheMetadata = new List<ICacheRecordMetadata>();
		private Dictionary<string, ICacheRecordMetadata> cacheMetadataDictionary =
			new Dictionary<string, ICacheRecordMetadata>();

		private delegate ICacheRecordMetadata GetMetadataDelegate(string fileName);

        /// <summary>
        /// Dispose all resources are used by the FileCacheBase.
        /// </summary>
        public void Dispose()
        {
            this.StopThread();
            this.queue.Clear();
            this.cacheMetadata.Clear();
            this.cacheMetadataDictionary.Clear();
        }

        private void StopThread()
        {
            if (this.thread != null)
            {
                if (this.thread.IsAlive)
                {
                    this.thread.Abort();
                }

                this.thread = null;
            }
        }

        private void StartThread()
        {
            if (this.thread == null && !string.IsNullOrEmpty(this.threadSafetyCachePath))
            {
                this.thread = new Thread(new ThreadStart(this.SaveFilesThread));
                this.thread.IsBackground = true;
                this.thread.Start();
            }
        }
        
        private void SaveFilesThread()
		{
			this.GetMetadata();

            while (!this.Dispatcher.HasShutdownStarted)
			{
				this.CheckCacheSize();

				WaitHandle[] waitHandles = new WaitHandle[1];
				waitHandles[0] = this.processRequest;
				WaitHandle.WaitAll(waitHandles);

				while (true)
				{
					CacheRecord record = null;
					lock (this.queueSynchronizer)
					{
						if (this.queue.Count > 0)
						{
							record = this.queue.Dequeue();
						}
						else
						{
							this.processRequest.Reset();
						}
					}

					if (record != null)
					{
						this.SaveFile(record);
						Thread.Sleep(10);
					}
					else
					{
						break;
					}
				}
			}
		}

        private void GetMetadata()
		{
			string[] files = this.GetFiles(this.threadSafetyCachePath, "*");

			Regex fileNameRegex = new Regex(@"\\([^\\]+)$");
			foreach (string file in files)
			{
				Match match = fileNameRegex.Match(file);
				if (match.Success)
				{
					ICacheRecordMetadata metadata = this.GetFileMetadataSafety(match.Groups[0].Value);
					if (metadata != null)
					{
						this.AddMetadata(metadata);
					}
				}
			}

			this.cacheMetadata.Sort(this.CompareMetadata);
		}

		private void AddMetadata(ICacheRecordMetadata metadata)
		{
			this.cacheMetadataDictionary.Add(metadata.FileName, metadata);
			this.cacheMetadata.Add(metadata);
			this.cacheSize += metadata.FileStorageLength;
		}

		private void CheckCacheSize()
		{
			while (this.maxSize > 0
			&& this.cacheSize >= this.maxSize
			&& this.cacheMetadata.Count > 0)
			{
				ICacheRecordMetadata metadata = this.cacheMetadata[0];
				this.RemoveFile(metadata.FileName);
				this.cacheMetadataDictionary.Remove(metadata.FileName);
				this.cacheMetadata.Remove(metadata);
				this.cacheSize -= metadata.FileStorageLength;
			}
		}

		private int CompareMetadata(ICacheRecordMetadata a, ICacheRecordMetadata b)
		{
			DateTime now = DateTime.Now;
			if (a == null && b == null)
			{
				return 0;
			}

			if (a == null)
			{
				return -1;
			}

			if (a == b)
			{
				return 0;
			}

			if (a.Expires < now && b.Expires < now)
			{
				return 0;
			}

			if (a.Expires < now)
			{
				return -1;
			}

			if (b.Expires < now)
			{
				return 1;
			}

			if (a.LastAccess < b.LastAccess)
				return -1;

			return 1;
		}

		private ICacheRecordMetadata GetFileMetadataSafety(string fileName)
		{
			ICacheRecordMetadata metaData;
			if (this is IsolatedStorageCache)
			{
				metaData = (ICacheRecordMetadata)this.Dispatcher.Invoke(DispatcherPriority.Normal,
					new GetMetadataDelegate(this.GetFileMetadata),
					fileName);
			}
			else
			{
				metaData = this.GetFileMetadata(fileName);
			}

			return metaData;
		}

		private ICacheRecordMetadata GetFileMetadata(string fileName)
		{
			return this.LoadFileMetadata(fileName);
		}

		private void SaveFile(CacheRecord cacheRecord)
		{
			ICacheRecordMetadata metadata = new CacheRecordMetadata(cacheRecord.FileName,
				cacheRecord.Expires,
				cacheRecord.FileStorageLength,
				cacheRecord.LastAccess);

			if (this.cacheMetadataDictionary.ContainsKey(cacheRecord.FileName))
			{
				this.cacheSize += cacheRecord.FileStorageLength - metadata.FileStorageLength;
				this.cacheMetadata[this.cacheMetadata.IndexOf(this.cacheMetadataDictionary[cacheRecord.FileName])] = metadata;
				this.cacheMetadataDictionary[cacheRecord.FileName] = metadata;
			}
			else
			{
				this.AddMetadata(metadata);
			}

			using (ManualResetEvent complete = new ManualResetEvent(false))
			{
				cacheRecord.CompeleEvent = complete;

				this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
					new Action<CacheRecord>(this.SaveCacheFile),
					cacheRecord);

				complete.WaitOne();
			}

			this.CheckCacheSize();
		}

		private void SaveCacheFile(CacheRecord cacheRecord)
		{
			Stream cacheStream = null;
			try
			{
				cacheStream = this.CreateCacheStream(cacheRecord.FileName);
			}
			catch
			{
			}

			if (cacheStream != null)
			{
				AsyncStreamOperations.SaveAsync(cacheStream, cacheRecord.TileBody, this.SaveFileMetadataCallback, cacheRecord);
			}
			else
			{
				cacheRecord.CompeleEvent.Set();
			}
		}

		private void SaveFileMetadataCallback(object userState)
		{
			CacheRecord cacheRecord = userState as CacheRecord;
			if (cacheRecord != null)
			{
				this.SaveFileMetadata(cacheRecord);
				cacheRecord.CompeleEvent.Set();
			}
		}
    }
}
