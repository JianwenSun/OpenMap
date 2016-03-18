using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;

namespace OpenMap
{
	/// <summary>
	/// Represents the MemoryCache class.
	/// It is used for caching tiles from map providers.
	/// </summary>
	public partial class MemoryCache : DependencyObject, ICacheStorage
	{
		/// <summary>
		/// Identifies the MemoryMaxSize dependency property.
		/// </summary>
		public static readonly DependencyProperty MemoryMaxSizeProperty = DependencyProperty.Register(
			"MemoryMaxSize",
			typeof(long),
			typeof(MemoryCache),
			new PropertyMetadata(MemoryMaxSizeChangedHandler));

		private const long DefaultMaxCacheSize = 10000000L;
		private Dictionary<string, CacheRecord> cacheRecords = new Dictionary<string, CacheRecord>();
		private object cacheRecordsSynchronizer = new object();
		private long cacheRecordsSize = 0;
		private long memoryMaxSize;
		private Collection<string> cacheRecordsOrder = new Collection<string>();

		/// <summary>
		/// Initializes a new instance of the MemoryCache class.
		/// </summary>
		public MemoryCache()
		{
			this.MemoryMaxSize = DefaultMaxCacheSize;
		}

		/// <summary>
		/// Gets or sets the maximal memory cache size for a storage.
		/// This is a dependency property.
		/// </summary>
		public long MemoryMaxSize
		{
			get
			{
				return (long)this.GetValue(MemoryMaxSizeProperty);
			}
			set
			{
				this.SetValue(MemoryMaxSizeProperty, value);
			}
		}

		/// <summary>
		/// Loads the file from the memory cache.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream if the file exists and does not expired or null.</returns>
		public virtual Stream Load(string fileName)
		{
			MemoryStream stream = null;
			byte[] tile = this.GetTile(fileName);
			if (tile != null)
			{
				stream = new MemoryStream(tile);
			}

			return stream;
		}

		/// <summary>
		/// Loaded file from a cache.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="callback">TileAvailable event arguments.</param>
		public virtual void LoadAsync(string fileName, Action<byte[]> callback)
		{
			byte[] tile = this.GetTile(fileName);
			callback(tile);
		}

		/// <summary>
		/// Saves the file to the memory.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="expirationDate">Expiration date.</param>
		/// <param name="tile">Byte array which is saved to the file.</param>
		public virtual void Save(string fileName, DateTime expirationDate, byte[] tile)
		{
			CacheRecord record = new CacheRecord(fileName, tile, expirationDate, false);
			this.AddToCacheRecords(record);
		}

		/// <summary>
		/// Copies one stream to another.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="streamTo">Stream.</param>
		internal static void CopyStream(System.IO.Stream stream, System.IO.Stream streamTo)
		{
			const int BufferSize = 32 * 1024;
			byte[] buffer = new byte[BufferSize];
			int readed;
			stream.Position = 0;
			while ((readed = stream.Read(buffer, 0, BufferSize)) > 0)
			{
				streamTo.Write(buffer, 0, readed);
			}
		}

		/// <summary>
		/// Reads tile.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Byte array of tile image.</returns>
		protected byte[] GetTile(string fileName)
		{
			CacheRecord record = this.GetCacheRecord(fileName);
			if (record != null)
			{
				return record.TileBody;
			}

			return null;
		}

		/// <summary>
		/// Gets the CacheRecord.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>CacheRecord.</returns>
		protected CacheRecord GetCacheRecord(string fileName)
		{
			CacheRecord record = null;
			lock (this.cacheRecordsSynchronizer)
			{
				if (this.cacheRecords.ContainsKey(fileName))
				{
					this.cacheRecordsOrder.Remove(fileName);
					this.cacheRecordsOrder.Add(fileName);

					record = this.cacheRecords[fileName];
				}
			}

			return record;
		}

		/// <summary>
		/// Adds cache record to the memory cache container.
		/// </summary>
		/// <param name="cacheRecord">Cache record instance.</param>
		protected void AddToCacheRecords(CacheRecord cacheRecord)
		{
			ThreadPool.QueueUserWorkItem(this.AddToCacheRecordsThread, cacheRecord);
		}

		private static void MemoryMaxSizeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MemoryCache cache = d as MemoryCache;
			if (cache != null)
			{
				cache.memoryMaxSize = (long)e.NewValue;
			}
		}

		private void AddToCacheRecordsThread(object cacheRecordObject)
		{
			CacheRecord cacheRecord = cacheRecordObject as CacheRecord;
			if (cacheRecord != null)
			{
				lock (this.cacheRecordsSynchronizer)
				{
					if (this.cacheRecords.ContainsKey(cacheRecord.FileName))
					{
						this.RemoveFromCacheRecords(cacheRecord.FileName);
					}

					this.cacheRecords.Add(cacheRecord.FileName, cacheRecord);
					this.cacheRecordsSize += cacheRecord.FileLength;
					this.cacheRecordsOrder.Add(cacheRecord.FileName);

					while (this.memoryMaxSize > 0
						&& this.cacheRecordsSize > this.memoryMaxSize
						&& this.cacheRecordsOrder.Count > 0)
					{
						this.RemoveFromCacheRecords(this.cacheRecordsOrder[0]);
					}
				}
			}
		}

		private void RemoveFromCacheRecords(string fileName)
		{
			CacheRecord removedCacheRecord = this.cacheRecords[fileName];
			this.cacheRecordsSize -= removedCacheRecord.FileLength;
			this.cacheRecords.Remove(fileName);
			this.cacheRecordsOrder.Remove(fileName);
		}
	}
}
