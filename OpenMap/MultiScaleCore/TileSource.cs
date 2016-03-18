using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenMap
{
    internal class TileSource
    {
        private byte[] body;
        private BitmapImage bitmapImage;
        private bool error;

        public TileSource(TileId tileId, byte[] imageBody)
        {
            this.TileId = tileId;
            this.body = imageBody;
        }

        public BitmapImage Bitmap
        {
            get
            {
                if (this.bitmapImage == null && !this.error)
                {
                    this.bitmapImage = this.CreateBitmap();
                }

                return this.bitmapImage;
            }
        }

        public bool IsUsed
        {
            get;
            set;
        }

        public int Size
        {
            get
            {
                int size = 0;
                byte[] imageBody = this.body;
                if (imageBody != null)
                {
                    size = imageBody.Length;
                }

                return size;
            }
        }

        public TileId TileId
        {
            get;
            private set;
        }

        public void ClearBitmap()
        {
            this.bitmapImage = null;
        }

        public void Dispose()
        {
            this.bitmapImage = null;
            this.body = null;
        }

        private BitmapImage CreateBitmap()
        {
            BitmapImage bitmap = null;
            byte[] imageBody = this.body;
            if (imageBody != null)
            {
                using (Stream stream = new MemoryStream(imageBody, false))
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();

                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.None;

                    try
                    {
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    catch
                    {
                        this.error = true;
                        bitmap = null;
                    }
                }
            }

            return bitmap;
        }
    }
}
