using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace OpenMap
{
	/// <summary>
	/// Represents the IsolatedCacheStorage class.
	/// </summary>
	public class IsolatedStorageCache : FileCacheBase
	{
		private IsolatedStorageFile isolatedStorageFile;

		/// <summary>
		/// Initializes a new instance of the IsolatedStorageCache class.
		/// </summary>
		public IsolatedStorageCache()
			: this("OpenMapCache", IsolatedStorageFile.GetUserStoreForAssembly())
		{
		}

		/// <summary>
		/// Initializes a new instance of the IsolatedStorageCache class.
		/// </summary>
		/// <param name="cachePath">Path to the cached files.</param>
		/// <param name="isolatedStorageFile">IsolatedStorageFile instance.</param>
		public IsolatedStorageCache(string cachePath, IsolatedStorageFile isolatedStorageFile)
		{
			this.isolatedStorageFile = isolatedStorageFile;
			this.CachePath = cachePath;
		}

		/// <summary>
		/// Clears all files in the cache.
		/// </summary>
		public void Clear()
		{
			this.isolatedStorageFile.DeleteDirectory(this.CachePath);

			this.OnCachePathChanged(this.CachePath, this.CachePath);
		}

		/// <summary>
		/// Creates a stream.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream.</returns>
		protected override Stream CreateCacheStream(string fileName)
		{
			return new IsolatedStorageFileStream(this.GetFullFilePath(fileName), FileMode.Create, this.isolatedStorageFile);
		}

		/// <summary>
		/// Returns stream to the file name.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream.</returns>
		protected override Stream OpenCacheStream(string fileName)
		{
			return new IsolatedStorageFileStream(this.GetFullFilePath(fileName), FileMode.Open, FileAccess.Read, FileShare.Read, this.isolatedStorageFile);
		}

		/// <summary>
		/// Obtains file names according to mask.
		/// </summary>
		/// <param name="cachePath">Path.</param>
		/// <param name="fileMask">File mask.</param>
		/// <returns>File list.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override string[] GetFiles(string cachePath, string fileMask)
		{
			try
			{
				string[] files = this.isolatedStorageFile.GetFileNames(cachePath + "\\" + fileMask);
				for (int i = 0; i < files.Length; i++)
				{
					files[i] = cachePath + "\\" + files[i];
				}

				return files;
			}
			catch
			{
			}

			return new string[0];
		}

		/// <summary>
		/// Calls when the CachePath property is changed.
		/// </summary>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override void OnCachePathChanged(string oldValue, string newValue)
		{
			base.OnCachePathChanged(oldValue, newValue);

			string[] folders;
			try
			{
				folders = this.isolatedStorageFile.GetDirectoryNames(newValue);
			}
			catch
			{
				folders = null;
			}

			if (folders == null || folders.Length == 0)
			{
				this.isolatedStorageFile.CreateDirectory(newValue);
			}
		}

		/// <summary>
		/// Removes file by name.
		/// </summary>
		/// <param name="fileName">File name.</param>
		protected override void RemoveFile(string fileName)
		{
			fileName = this.GetFullFilePath(fileName);
			this.isolatedStorageFile.DeleteFile(fileName);
			this.isolatedStorageFile.DeleteFile(fileName + ".meta");
		}

		/// <summary>
		/// Loads file metadata.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>ICacheRecordMetadata implementation instance.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override ICacheRecordMetadata LoadFileMetadata(string fileName)
		{
			try
			{
				using (Stream stream = this.OpenCacheStream(fileName + ".meta"))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						string line = reader.ReadLine();
						DateTime expires = DateTime.FromFileTimeUtc(long.Parse(line));

						line = reader.ReadLine();
						long length = long.Parse(line);

						line = reader.ReadLine();
						DateTime lastAccess = DateTime.FromFileTimeUtc(long.Parse(line));

						return new FileCacheBase.CacheRecordMetadata(fileName, expires, length, lastAccess);
					}
				}
			}
			catch
			{
			}

			return null;
		}

		/// <summary>
		/// Saves file metadata.
		/// </summary>
		/// <param name="cacheRecord">Cache record metadata.</param>
		protected override void SaveFileMetadata(FileCacheBase.ICacheRecordMetadata cacheRecord)
		{
			using (Stream stream = this.CreateCacheStream(cacheRecord.FileName + ".meta"))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.WriteLine(cacheRecord.Expires.ToFileTimeUtc());
					writer.WriteLine(cacheRecord.FileStorageLength);
					writer.WriteLine(DateTime.Now.ToFileTimeUtc());
				}
			}
		}
	}
}
