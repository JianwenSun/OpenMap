using System;
using System.IO;

namespace OpenMap
{
	/// <summary>
	/// Provides methods for async reading of the tiles.
	/// This class is designed for internal use.
	/// </summary>
	public sealed class AsyncStreamOperations
	{
		private const int TileBodyPartLength = 4096;

		private Stream stream;
		private Action<byte[]> readerCallback;
		private Action<object> writerCallback;

		private MemoryStream memoryStream;
		private byte[] tileBodyPart;
		private object userState;

		private AsyncStreamOperations(Stream stream)
		{
			this.stream = stream;
		}

		/// <summary>
		/// Loads stream asynchronously to byte array.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="callback">Callback.</param>
		public static void LoadAsync(Stream stream, Action<byte[]> callback)
		{
			AsyncStreamOperations loader = new AsyncStreamOperations(stream);
			loader.readerCallback = callback;
			loader.memoryStream = new MemoryStream();
			loader.tileBodyPart = new byte[TileBodyPartLength];

			loader.stream.BeginRead(loader.tileBodyPart, 0, TileBodyPartLength, ReadCallback, loader);
		}

		/// <summary>
		/// Saves byte array asynchronously to the stream.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="tile">Byte array.</param>
		public static void SaveAsync(Stream stream, byte[] tile)
		{
			SaveAsync(stream, tile, null, null);
		}

		/// <summary>
		/// Saves byte array asynchronously to the stream.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="tile">Byte array.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="userState">User state.</param>
		public static void SaveAsync(Stream stream, byte[] tile, Action<object> callback, object userState)
		{
			AsyncStreamOperations writer = new AsyncStreamOperations(stream);
			writer.writerCallback = callback;
			writer.userState = userState;
			writer.tileBodyPart = tile;

			writer.stream.BeginWrite(writer.tileBodyPart, 0, writer.tileBodyPart.Length, WriteCallback, writer);
		}

		private static void ReadCallback(IAsyncResult asyncResult)
		{
			AsyncStreamOperations loader = (AsyncStreamOperations)asyncResult.AsyncState;
			int loaded = loader.stream.EndRead(asyncResult);
			if (asyncResult.IsCompleted && loaded > 0)
			{
				loader.memoryStream.Write(loader.tileBodyPart, 0, loaded);
				loader.stream.BeginRead(loader.tileBodyPart, 0, TileBodyPartLength, ReadCallback, loader);
				return;
			}

			byte[] tile = loader.memoryStream.ToArray();
			loader.Dispose();

			loader.readerCallback(tile);
			loader = null;
		}

		private static void WriteCallback(IAsyncResult asyncResult)
		{
			AsyncStreamOperations writer = (AsyncStreamOperations)asyncResult.AsyncState;
			writer.stream.EndWrite(asyncResult);
			writer.Dispose();
			if (writer.writerCallback != null)
			{
				writer.writerCallback(writer.userState);
			}
			writer = null;
		}

		private void Dispose()
		{
			this.stream.Dispose();

			if (this.memoryStream != null)
			{
				this.memoryStream.Dispose();
			}

			this.tileBodyPart = null;
		}
	}
}
