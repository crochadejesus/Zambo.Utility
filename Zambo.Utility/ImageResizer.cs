using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Value.Infrastructure.Configuration;

namespace Value.Infrastructure.Helpers
{
    public enum AnchorPosition
    {
        Left,
        Right,
        Top,
        Bottom,
        Center,
    }

    public class ImageManipulation
    {
        public static byte[] GetImage(int width, int height, string imagePath, bool crop)
        {
            byte[] file = new byte[0];
            // Verifica se ficheiro existe
            var fi = new FileInfo(imagePath);
            if (fi.Exists)
            {
                // get original image
                Stream stream = new StreamReader(imagePath).BaseStream;
                Image originalImage = Image.FromStream(stream);
                // process missing width or height calculations for resize
                int resizeWidth = width == 0 ? ImageResizer.CalculateImageWidth(originalImage, height) : width;
                int resizeHeight = height == 0 ? ImageResizer.CalculateImageHeight(originalImage, width) : height;
                // resize original image with or whithout crop
                Image resizedImage;
                if (crop)
                {
                    resizedImage = ImageResizer.Crop(originalImage, resizeWidth, resizeHeight, AnchorPosition.Center);
                }
                else
                {
                    resizedImage = ImageResizer.FixedSize(originalImage, resizeWidth, resizeHeight);
                }
                // return new image byte array
                using (var ms = new MemoryStream())
                {
                    var imageFormat = ImageFormat.Jpeg;
                    resizedImage.Save(ms, imageFormat);
                    file = ms.ToArray();
                }
            }

            return file;
        }
    }

    public class ImageResizer
    {
        /*
         * SCALE BY PERCENT
         * Method of scaling a photograph by a specified percentage.
         * Both the width and height will be scaled uniformly.
         */

        public static int CalculateImageHeight(Image originalImage, int width)
        {
            var ratio = (double)originalImage.Width / (double)originalImage.Height;
            return width > 0 ? (int)(width / ratio) : originalImage.Width;
        }

        public static int CalculateImageWidth(Image originalImage, int height)
        {
            var ratio = (double)originalImage.Width / (double)originalImage.Height;
            return height > 0 ? (int)(height / ratio) : originalImage.Width;
        }

        public static Image Crop(Image imgPhoto, int Width, int Height, AnchorPosition Anchor)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destX = 0;
            int destY = 0;

            double nPercent = 0;
            double nPercentW = 0;
            double nPercentH = 0;

            nPercentW = ((double)Width / (double)sourceWidth);
            nPercentH = ((double)Height / (double)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                switch (Anchor)
                {
                    case AnchorPosition.Top:
                        destY = 0;
                        break;

                    case AnchorPosition.Bottom:
                        destY = (int)(Height - (sourceHeight * nPercent));
                        break;

                    default:
                        destY = (int)((Height - (sourceHeight * nPercent)) / 2);
                        break;
                }
            }
            else
            {
                nPercent = nPercentH;
                switch (Anchor)
                {
                    case AnchorPosition.Left:
                        destX = 0;
                        break;

                    case AnchorPosition.Right:
                        destX = (int)(Width - (sourceWidth * nPercent));
                        break;

                    default:
                        destX = (int)((Width - (sourceWidth * nPercent)) / 2);
                        break;
                }
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            return RenderWithWatermark(imgPhoto, Width, Height, destWidth, destHeight, destX, destY);
        }

        public static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destX = 0;
            int destY = 0;
            return RenderWithWatermark(imgPhoto, Width, Height, Width, Height, destX, destY);
        }

        public static ImageFormat GetImageFormat(string contentType)
        {
            var format = ImageFormat.Jpeg;
            switch (contentType)
            {
                case "image/png":
                    format = ImageFormat.Png;
                    break;

                case "image/jpeg":
                    format = ImageFormat.Jpeg;
                    break;

                case "image/gif":
                    format = ImageFormat.Gif;
                    break;

                case "image/bmp":
                    format = ImageFormat.Bmp;
                    break;
            }
            return format;
        }

        public static Image ScaleByPercent(Image imgPhoto, int Percent)
        {
            double nPercent = ((double)Percent / 100);
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            return RenderWithWatermark(imgPhoto, destWidth, destHeight, destWidth, destHeight, destX, destY);
        }

        /*
         * SCALE TO FIXED SIZE
         * Since images will have varying orientations it will be necessary to fit either the width or height,
         * then pad the opposite dimension with filler.
         */
        /*
         * SCALE WITH CROPPING
         * When cropping an image you have 5 choices as to how one decides, what part of the image to crop. We refer to this as how one anchors the image.
         * Top, bottom, and center are appropriate when cropping an images height where as left, right and center are appropriate for width.
         */
        /* HELPERS */

        private static Size CalculateWatermarkSize(Bitmap imageToWatermark, Bitmap watermark, decimal percentageCover)
        {
            bool widthMode = ConfigurationManager.AppSettings["Watermark.Dimension.Cover"] == "width" ? true : false;

            decimal wmWidth = Convert.ToDecimal(watermark.Width);
            decimal wmHeight = Convert.ToDecimal(watermark.Height);

            decimal calculatedWidth = 0;
            decimal calculatedHeight = 0;

            if (widthMode)
            {
                calculatedWidth = imageToWatermark.Width * percentageCover;
                calculatedHeight = calculatedWidth / (wmWidth / wmHeight);
            }
            else
            {
                calculatedHeight = imageToWatermark.Height * percentageCover;
                calculatedWidth = calculatedHeight * (wmWidth / wmHeight);
            }
            return new Size(Convert.ToInt32(calculatedWidth), Convert.ToInt32(calculatedHeight));
        }

        private static Image Render(Image imgPhoto, int width, int height, int destWidth, int destheight, int destX, int destY)
        {
            Bitmap bmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            //HQ Bicubic is 2 pass. Its only 30% slower than low quality, why not have HQ results?
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //Ensures the edges are crisp
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            //Prevents artifacts at the edges
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //Ensures matted PNGs look decent
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            //Prevents really ugly transparency issues
            grPhoto.CompositingMode = CompositingMode.SourceOver;

            using (var ia = new ImageAttributes())
            {
                //Fixes the 50% gray border issue on bright white or dark images
                ia.SetWrapMode(WrapMode.TileFlipXY);

                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, destWidth, destheight),
                    0, 0, imgPhoto.Width, imgPhoto.Height,
                    GraphicsUnit.Pixel, ia);
            }
            grPhoto.Dispose();
            return bmPhoto;
        }

        private static Image RenderWithWatermark(Image imgPhoto, int width, int height, int destWidth, int destheight, int destX, int destY)
        {
            Bitmap bmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            //HQ Bicubic is 2 pass. Its only 30% slower than low quality, why not have HQ results?
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //Ensures the edges are crisp
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            //Prevents artifacts at the edges
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //Ensures matted PNGs look decent
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            //Prevents really ugly transparency issues
            grPhoto.CompositingMode = CompositingMode.SourceOver;

            using (var ia = new ImageAttributes())
            {
                //Fixes the 50% gray border issue on bright white or dark images
                ia.SetWrapMode(WrapMode.TileFlipXY);

                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, destWidth, destheight),
                    0, 0, imgPhoto.Width, imgPhoto.Height,
                    GraphicsUnit.Pixel, ia);
            }
            if (WatermarkIsRequired(width, height))
            {
                //Apply watermark
                //using (Bitmap bmp = new Bitmap(System.Web.HttpContext.Current.Server.MapPath("~/Content/watermark_logo.png")))
                using (Bitmap bmp = new Bitmap(Path.Combine(Environment.CurrentDirectory, WebConfig.GetWatermarkPathKeyValue())))
                {
                    decimal percentageCover = decimal.Parse(ConfigurationManager.AppSettings["Watermark.Percentage.Cover"], System.Globalization.CultureInfo.InvariantCulture);
                    Size wmSize = CalculateWatermarkSize(bmPhoto, bmp, percentageCover);
                    using (Bitmap bmWatermark = new Bitmap(bmp, wmSize.Width, wmSize.Height))
                    {
                        int pos_x = (width / 2 - bmWatermark.Width / 2);
                        int pos_y = (height / 2 - bmWatermark.Height / 2);
                        float opacity = float.Parse(ConfigurationManager.AppSettings["Watermark.Opacity"], System.Globalization.CultureInfo.InvariantCulture);
                        Image final = WatermarkImage(bmPhoto, bmWatermark, new Point(pos_x, pos_y), opacity);
                    }
                }
            }
            grPhoto.Dispose();
            return bmPhoto;
        }

        private static Bitmap WatermarkImage(Bitmap ImageToWatermark, Bitmap Watermark, Point WatermarkPosition, float Opacity)
        {
            using (Graphics G = Graphics.FromImage(ImageToWatermark))
            {
                using (ImageAttributes IA = new ImageAttributes())
                {
                    ColorMatrix CM = new ColorMatrix();
                    CM.Matrix33 = Opacity;
                    IA.SetColorMatrix(CM);
                    G.DrawImage(Watermark, new Rectangle(WatermarkPosition, Watermark.Size), 0, 0, Watermark.Width, Watermark.Height, GraphicsUnit.Pixel, IA);
                }
            }
            return ImageToWatermark;
        }

        private static bool WatermarkIsRequired(int imgWidth, int imgHeight)
        {
            //Watermark requirement is based on the app settings
            bool apply = Convert.ToBoolean(ConfigurationManager.AppSettings["Watermark.Apply"]);
            int minWidth = Convert.ToInt32(ConfigurationManager.AppSettings["Watermark.Start.Width"]);
            int minHeight = Convert.ToInt32(ConfigurationManager.AppSettings["Watermark.Start.Height"]);
            return (apply && (imgWidth > minWidth || imgHeight > minHeight));
        }
    }
}
