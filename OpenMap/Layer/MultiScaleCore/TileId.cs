using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    internal class TileId : IEqualityComparer<TileId>
    {
        private int[] tileId;

        internal TileId(int level, int x, int y)
        {
            this.tileId = new int[] { level, x, y };
        }

        private TileId()
        {
        }

        internal static IEqualityComparer<TileId> EqualityComparer
        {
            get
            {
                return new TileId();
            }
        }

        internal int Level
        {
            get
            {
                return this.tileId[0];
            }
        }

        internal int X
        {
            get
            {
                return this.tileId[1];
            }
        }

        internal int Y
        {
            get
            {
                return this.tileId[2];
            }
        }

        public static TileId operator -(TileId t1, TileId t2)
        {
            int level = t1.Level - t2.Level;
            int x = t1.X - (t2.X << level);
            int y = t1.Y - (t2.Y << level);

            TileId id = new TileId(level, x, y);
            return id;
        }

        public override bool Equals(object obj)
        {
            TileId other = obj as TileId;
            if (other == null)
            {
                return false;
            }

            return this.tileId.SequenceEqual(other.tileId);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(TileId x, TileId y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(TileId obj)
        {
            return obj.Level.GetHashCode() ^ obj.X.GetHashCode() ^ obj.Y.GetHashCode();
        }
    }
}
