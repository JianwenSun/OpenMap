using System;

namespace OpenMap
{
	/// <summary>
	/// Tile source which don't provide any tiles.
	/// </summary>
	public class EmptyTileMapSource : TiledMapSource
	{
		/// <summary>
		/// Initializes a new instance of the EmptyTileMapSource class.
		/// </summary>
		public EmptyTileMapSource()
			: this(256, 256)
		{
		}

		/// <summary>
		/// Initializes a new instance of the EmptyTileMapSource class.
		/// </summary>
		/// <param name="tileWidth">Tile width.</param>
		/// <param name="tileHeight">Tile height.</param>
		public EmptyTileMapSource(int tileWidth, int tileHeight)
			: base(1, 20, tileWidth, tileHeight)
		{
		}

		/// <summary>
		/// Initialize provider.
		/// </summary>
		public override void Initialize()
		{
			// Raise provider intialized event.
			this.RaiseInitializeCompleted();
		}
	}
}
