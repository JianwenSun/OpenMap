using System;
using System.IO;
using System.Threading;

namespace OpenMap
{
	/// <summary>
	/// A generic map cache storage.
	/// </summary>
	public partial class MemoryCache
	{
		/// <summary>
		/// Represents the CacheRecord class.
		/// </summary>
		protected class CacheRecord : ICacheRecordMetadata
		{
			private string name;
			private DateTime expires;
			private DateTime lastAccess;
			private bool isUseMetaFile;

			/// <summary>
			/// Initializes a new instance of the CacheRecord class.
			/// </summary>
			/// <param name="fileName">File name.</param>
			/// <param name="tileBody">Byte array of the tile image.</param>
			/// <param name="fileExpires">Expires time.</param>
			/// <param name="useMetaFiles">Use metafiles (isolated storage only).</param>
			public CacheRecord(string fileName, byte[] tileBody, DateTime fileExpires, bool useMetaFiles)
			{
				this.name = fileName;
				this.TileBody = tileBody;
				this.expires = fileExpires;
				this.isUseMetaFile = useMetaFiles;
				this.lastAccess = DateTime.Now;
			}

			/// <summary>
			/// Length of file in storage.
			/// </summary>
			public long FileStorageLength
			{
				get
				{
					long size = this.TileBody.Length;

					if (this.isUseMetaFile)
					{
						string expiresTime = this.Expires.ToFileTimeUtc().ToString();
						string lastAccessTime = this.LastAccess.ToFileTimeUtc().ToString();

						size += 6L
							+ this.TileBody.Length.ToString().Length
							+ expiresTime.Length
							+ lastAccessTime.Length;
					}

					return size;
				}
			}

			/// <summary>
			/// File name.
			/// </summary>
			public string FileName
			{
				get
				{
					return this.name;
				}
			}

			/// <summary>
			/// Gets cache expiration date.
			/// </summary>
			public DateTime Expires
			{
				get
				{
					return this.expires;
				}
			}

			/// <summary>
			/// Gets date of the last access.
			/// </summary>
			public DateTime LastAccess
			{
				get
				{
					return this.lastAccess;
				}
			}

			internal byte[] TileBody
			{
				get;
				private set;
			}

			internal long FileLength
			{
				get
				{
					return this.TileBody.Length;
				}
			}

			internal ManualResetEvent CompeleEvent
			{
				get;
				set;
			}

			internal Stream GetStream()
			{
				MemoryStream memoryStream = new MemoryStream(this.TileBody);
				return memoryStream;
			}
		}
	}
}
