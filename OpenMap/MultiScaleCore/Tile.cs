using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OpenMap
{
    internal class MapTile : Shape
    {
        private TileSource tileSource;
        private Size size;
        private ImageBrush imageBrush;
        private TranslateTransform imageTranslate;
        private ScaleTransform imageScale;

        internal MapTile(Size size, Point leftTop)
        {
            this.size = size;
            this.imageBrush = this.CreateImage();
            this.Fill = this.imageBrush;
            this.IsHitTestVisible = false;

            this.Width = this.size.Width;
            this.Height = this.size.Height;

            TranslateTransform translateTransform = new TranslateTransform()
            {
                X = leftTop.X,
                Y = leftTop.Y
            };
            translateTransform.Freeze();
            this.RenderTransform = translateTransform;
        }

        internal TileId RelativeTileId
        {
            get;
            set;
        }

        internal TileId TileId
        {
            get;
            set;
        }

        internal TileSource Source
        {
            get
            {
                return this.tileSource;
            }

            set
            {
                TileSource source = value;
                this.tileSource = source;
                if (source != null)
                {
                    TileId id = this.TileId - source.TileId;
                    this.imageScale.ScaleX = this.imageScale.ScaleY = Math.Pow(2, id.Level);
                    this.imageTranslate.X = -id.X * this.size.Width;
                    this.imageTranslate.Y = -id.Y * this.size.Height;

                    this.imageBrush.ImageSource = source.Bitmap;
                    if (this.imageBrush.ImageSource != null)
                    {
                        source.IsUsed = true;
                    }
                }
                else
                {
                    this.imageBrush.ImageSource = null;
                }
            }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                Geometry geometry = new RectangleGeometry(new Rect(this.size));
                return geometry;
            }
        }

        private ImageBrush CreateImage()
        {
            ImageBrush image = new ImageBrush();

            this.imageScale = new ScaleTransform();
            this.imageScale.ScaleX = this.imageScale.ScaleY = 1;
            this.imageTranslate = new TranslateTransform();

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(this.imageScale);
            transformGroup.Children.Add(this.imageTranslate);

            image.Transform = transformGroup;

            return image;
        }
    }
}
