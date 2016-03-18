using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    internal struct MultiscaleImageViewport
    {
        internal Point ViewportOrigin
        {
            get;
            set;
        }

        internal double ViewportWidth
        {
            get;
            set;
        }

        internal double ActualWidth
        {
            get;
            set;
        }

        internal double ActualHeight
        {
            get;
            set;
        }

        internal int TileWidth
        {
            get;
            set;
        }

        internal int TileHeight
        {
            get;
            set;
        }
    }
}
