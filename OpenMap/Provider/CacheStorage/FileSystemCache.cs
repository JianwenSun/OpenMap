using System;
using System.IO;
using System.Text.RegularExpressions;

namespace OpenMap
{
	/// <summary>
	/// Represents the FileSystemCacheStorage class.
	/// </summary>
	public class FileSystemCache : FileCacheBase
	{
		/// <summary>
		/// Initializes a new instance of the FileSystemCache class.
		/// </summary>
		public FileSystemCache()
		{
		}

		/// <summary>
		/// Initializes a new instance of the FileSystemCache class.
		/// </summary>
		/// <param name="cachePath">Cache path.</param>
		public FileSystemCache(string cachePath)
		{
			this.CachePath = cachePath;
		}

		/// <summary>
		/// Creates a stream.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream.</returns>
		protected override Stream CreateCacheStream(string fileName)
		{
			return new FileStream(this.GetFullFilePath(fileName), FileMode.Create);
		}

		/// <summary>
		/// Returns stream to the file name.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Stream.</returns>
		protected override Stream OpenCacheStream(string fileName)
		{
			return new FileStream(this.GetFullFilePath(fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
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
				return Directory.GetFiles(cachePath, fileMask);
			}
			catch
			{
				return new string[0];
			}
		}

		/// <summary>
		/// Calls when the CachePath property is changed.
		/// </summary>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		protected override void OnCachePathChanged(string oldValue, string newValue)
		{
			base.OnCachePathChanged(oldValue, newValue);

			if (!Directory.Exists(newValue))
			{
				Directory.CreateDirectory(newValue);
			}
		}

		/// <summary>
		/// Removes file by name.
		/// </summary>
		/// <param name="fileName">File name.</param>
		protected override void RemoveFile(string fileName)
		{
			fileName = this.GetFullFilePath(fileName);

            //// Blind fix for ticket #542805
            try
            {
                File.Delete(fileName);
            }
            catch
            {
            }
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
				FileInfo info = new FileInfo(this.GetFullFilePath(fileName));
                if (info.Exists)
                {
                    DateTime expires = info.LastWriteTime;
                    long length = info.Length;
                    DateTime lastAccess = info.LastAccessTime;

                    return new FileCacheBase.CacheRecordMetadata(fileName, expires, length, lastAccess);
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override void SaveFileMetadata(FileCacheBase.ICacheRecordMetadata cacheRecord)
		{
			try
			{
				string fileName = this.GetFullFilePath(cacheRecord.FileName);

				File.SetLastWriteTime(fileName, cacheRecord.Expires);
				File.SetLastAccessTime(fileName, cacheRecord.LastAccess);
			}
			catch
			{
			}
		}
	}
}
