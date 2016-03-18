using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    public class GoogleMapSource : GoogleTiledMapSource
    {
        private const string TileUrlFormat = @"http://{prefix}.google.cn/vt/lyrs=m@258000000&x={x}&y={y}&z={zoom}";
        //@"http://mt3.google.cn/vt/lyrs=m@258000000&hl=zh-CN&gl=CN&src=app&x=0&y=0&z=0&s=Ga";

        /// <summary>
        /// Initializes a new instance of the GoogleMapSource class.
        /// </summary>
        public GoogleMapSource()
            : base(TileUrlFormat)
        {
        }
    }
}
