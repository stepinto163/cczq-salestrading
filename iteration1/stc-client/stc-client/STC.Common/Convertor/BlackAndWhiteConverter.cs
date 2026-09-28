using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STC.Common.Convertor
{
    public class BlackAndWhiteConverter : IValueConverter
    {
        public double TransparentThreshold { get; set; } = 0.5;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage originalImage2 = new BitmapImage(new Uri((string)value, UriKind.RelativeOrAbsolute));
            if (originalImage2 is BitmapImage)
            {
                BitmapImage originalImage = (BitmapImage)originalImage2;

                FormatConvertedBitmap grayscaleBitmap = new FormatConvertedBitmap(originalImage, PixelFormats.Gray8, BitmapPalettes.Gray256, 0);

                byte transparentThresholdByte = (byte)(TransparentThreshold * 255);
                byte[] pixelData = new byte[grayscaleBitmap.PixelWidth * grayscaleBitmap.PixelHeight];
                grayscaleBitmap.CopyPixels(pixelData, grayscaleBitmap.PixelWidth, 0);

                for (int i = 0; i < pixelData.Length; i++)
                {
                    if (pixelData[i] <= transparentThresholdByte)
                    {
                        pixelData[i] = 0; // Set transparent
                    }
                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight, grayscaleBitmap.DpiX, grayscaleBitmap.DpiY, PixelFormats.Gray8, null);
                writeableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight), pixelData, grayscaleBitmap.PixelWidth, 0);

                return writeableBitmap;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
