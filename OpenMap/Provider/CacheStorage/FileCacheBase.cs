using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;

namespace OpenMap
{
	/// <summary>
	/// The base file cache class.
	/// </summary>
	public abstract partial class FileCacheBase : MemoryCache
	{
		/// <summary>
		/// Identifies the CachePath dependency property.
		/// </summary>
		public static readonly DependencyProperty CachePathProperty = DependencyProperty.Register(
			"CachePath",
			typeof(string),
			typeof(FileCacheBase),
			new PropertyMetadata(CachePathChangedHandler));

		/// <summary>
		/// Identifies the MaxSize dependency property.
		/// </summary>
		public static readonly DependencyProperty MaxSizeProperty = DependencyProperty.Register(
			"MaxSize",
			typeof(long),
			typeof(FileCacheBase),
			new PropertyMetadata(MaxSizeChangedHandler));

		/// <summary>
		/// Identifies the MinExpiresTime dependency property.
		/// </summary>
		public static readonly DependencyProperty MinExpirationTimeProperty = DependencyProperty.Register(
			"MinExpirationTime",
			typeof(TimeSpan),
			typeof(FileCacheBase),
			new PropertyMetadata(MinExpirationTimeChangedHandler));

		/// <summary>
		/// Identifies the MaxExpiresTime dependency property.
		/// </summary>
		public static readonly DependencyProperty MaxExpirationTimeProperty = DependencyProperty.Register(
			"MaxExpirationTime",
			typeof(TimeSpan),
			typeof(FileCacheBase),
			new PropertyMetadata(MaxExpirationTimeChangedHandler));

		private long maxSize;
		private string threadSafetyCachePath;
		private Thread thread;
		private TimeSpan minExpirationTime;
		private TimeSpan maxExpirationTime;

		/// <summary>
		/// Initializes a new instance of the FileCacheBase class.
		/// </summary>
		protected FileCacheBase()
		{
			this.MaxSize = 50000000L;
			this.MinExpirationTime = TimeSpan.FromDays(7);
			this.MaxExpirationTime = TimeSpan.FromDays(3650);
        }

		/// <summary>
		/// Gets or sets the path to files for a storage.
		/// This is a dependency property.
		/// </summary>
		public string CachePath
		{
			get
			{
				return (string)this.GetValue(CachePathProperty);
			}
			set
			{
				this.SetValue(CachePathProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the maximal cache size for a storage.
		/// Zero value specifies unlimited cache.
		/// This is a dependency property.
		/// </summary>
		public long MaxSize
		{
			get
			{
				return (long)this.GetValue(MaxSizeProperty);
			}
			set
			{
				this.SetValue(MaxSizeProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the minimal expiration timespan for a file in cache.
		/// This is a dependency property.
		/// </summary>
		public TimeSpan MinExpirationTime
		{
			get
			{
				return (TimeSpan)this.GetValue(MinExpirationTimeProperty);
			}
			set
			{
				this.SetValue(MinExpirationTimeProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the maximal expiration timespan for a file in cache.
		/// This is a dependency property.
		/// </summary>
		public TimeSpan MaxExpirationTime
		{
			get
			{
				return (TimeSpan)this.GetValue(MaxExpirationTimeProperty);
			}
			set
			{
				this.SetValue(MaxExpirationTimeProperty, value);
			}
		}

		/// <summary>
		/// Loads the file from the storage.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream if the file exists and does not expired or null.</returns>
		public override Stream Load(string fileName)
		{
			Stream stream = null;
			byte[] tile = this.GetTile(fileName);
			if (tile == null)
			{
				stream = this.GetStorageStream(fileName);
			}
			else
			{
				stream = new MemoryStream(tile);
			}

			return stream;
		}

		/// <summary>
		/// Loaded file from a cache to event arguments.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="callback">Callback which should be called to return tile if it is available or null.</param>
		public override void LoadAsync(string fileName, Action<byte[]> callback)
		{
			CacheRecord record = this.GetCacheRecord(fileName);
			byte[] tile = null;
			if (record == null)
			{
				ICacheRecordMetadata metaData = this.LoadFileMetadata(fileName);
				if (metaData != null && metaData.Expires >= DateTime.Now)
				{
					Stream stream = this.OpenCacheStream(fileName);
					if (stream.Length == 0)
					{
						stream.Dispose();
					}
					else
					{
						AsyncStreamOperations.LoadAsync(stream, callback);
						return;
					}
				}
			}
			else
			{
				tile = record.TileBody;
			}

			callback(tile);
		}

		/// <summary>
		/// Saves the file to the storage.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="expirationDate">Expiration date.</param>
		/// <param name="tile">Byte array which is saved to the file.</param>
		public override void Save(string fileName, DateTime expirationDate, byte[] tile)
		{
			base.Save(fileName, expirationDate, tile);

			expirationDate = this.CoerceExpirationDate(expirationDate);
			CacheRecord cacheRecord = new CacheRecord(fileName, tile, expirationDate, this is IsolatedStorageCache);

			lock (this.queueSynchronizer)
			{
				this.queue.Enqueue(cacheRecord);
				this.processRequest.Set();
			}
		}

		/// <summary>
		/// Obtains files.
		/// </summary>
		/// <param name="cachePath">Cache path.</param>
		/// <param name="fileMask">File mask.</param>
		/// <returns>Array of file names.</returns>
		protected abstract string[] GetFiles(string cachePath, string fileMask);

		/// <summary>
		/// Removes file from cache storage.
		/// </summary>
		/// <param name="fileName">File name.</param>
		protected abstract void RemoveFile(string fileName);

		/// <summary>
		/// Creates cache stream.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream to saving into cache.</returns>
		protected abstract Stream CreateCacheStream(string fileName);

		/// <summary>
		/// Gets cache stream.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream to loading from cache.</returns>
		protected abstract Stream OpenCacheStream(string fileName);

		/// <summary>
		/// Loads file metadata.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>ICacheRecordMetadata implementation instance.</returns>
		protected abstract ICacheRecordMetadata LoadFileMetadata(string fileName);

		/// <summary>
		/// Saves file metadata.
		/// </summary>
		/// <param name="cacheRecord">Cache record metadata.</param>
		protected abstract void SaveFileMetadata(ICacheRecordMetadata cacheRecord);

		/// <summary>
		/// Returns full file path.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Full file path.</returns>
		protected string GetFullFilePath(string fileName)
		{
			return this.threadSafetyCachePath + "\\" + fileName;
		}

		/// <summary>
		/// Calls when the cache path is changed.
		/// </summary>
		/// <param name="oldValue">Old path.</param>
		/// <param name="newValue">New path.</param>
		protected virtual void OnCachePathChanged(string oldValue, string newValue)
		{
			this.threadSafetyCachePath = newValue;

            this.StartThread();
		}

		/// <summary>
		/// Sets thread safety max size value.
		/// </summary>
		protected virtual void OnMaxSizeChanged()
		{
			this.maxSize = this.MaxSize;
		}

		private static void CachePathChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if(!System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
			{
				((FileCacheBase)d).OnCachePathChanged((string)e.OldValue, (string)e.NewValue);
			}
		}

		private static void MaxSizeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FileCacheBase)d).OnMaxSizeChanged();
		}

		private static void MinExpirationTimeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FileCacheBase)d).minExpirationTime = (TimeSpan)e.NewValue;
		}

		private static void MaxExpirationTimeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FileCacheBase)d).maxExpirationTime = (TimeSpan)e.NewValue;
		}

		private DateTime CoerceExpirationDate(DateTime expirationDate)
		{
			DateTime now = DateTime.Now;
			DateTime min = now;
			DateTime max = now;
			TimeSpan minSpan = this.minExpirationTime;
			TimeSpan maxSpan = this.maxExpirationTime;
			TimeSpan maxAllowedSpan = DateTime.MaxValue - now;

			if (maxAllowedSpan < minSpan)
			{
				min = DateTime.MaxValue;
			}
			else
			{
				min += minSpan;
			}

			if (maxAllowedSpan < maxSpan)
			{
				max = DateTime.MaxValue;
			}
			else
			{
				max += maxSpan;
			}

			if (expirationDate < min)
			{
				expirationDate = min;
			}
			else if (expirationDate > max)
			{
				expirationDate = max;
			}

			return expirationDate;
		}

		private Stream GetStorageStream(string fileName)
		{
			ICacheRecordMetadata metaData = this.LoadFileMetadata(fileName);
			if (metaData != null && metaData.Expires >= DateTime.Now)
			{
				Stream stream = this.OpenCacheStream(fileName);
				if (stream.Length == 0)
				{
					stream.Dispose();
					return null;
				}

				this.SaveFileMetadata(metaData);

				return stream;
			}

			return null;
		}
	}
}
