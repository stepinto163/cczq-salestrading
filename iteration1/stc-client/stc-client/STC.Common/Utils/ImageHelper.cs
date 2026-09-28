using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class ImageHelper
    {

        public static string DocumentToBase64Str(string fileName)
        {
            FileStream filestream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            byte[] bt = new byte[filestream.Length];
            //调用read读取方法
            filestream.Read(bt, 0, bt.Length);
            string base64Str = Convert.ToBase64String(bt);
            filestream.Close();
            return base64Str;
        }

        // Image 对象转换为 base64 字符串
        public static string ImageToBase64(Image _image)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                //if (ImageFormatGuidToString(_image.RawFormat) == null)
                //{
                    _image.Save(ms, ImageFormat.Png);
                //}
                //else
                //{
                //    _image.Save(ms, _image.RawFormat);
                //}
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                return Convert.ToBase64String(arr);
            }
            catch
            {
                return null;
            }
            finally
            {
                ms.Close();
            }
        }

        // base64 字符串转换为 Image 对象
        public static Image Base64ToImage(string _base64)
        {
            byte[] arr = Convert.FromBase64String(_base64);
            MemoryStream ms = new MemoryStream(arr);
            Bitmap bmp = new Bitmap(ms);
            try
            {
                Image result = new Bitmap(bmp.Width, bmp.Height);
                Graphics g = Graphics.FromImage(result);
                g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                g.Dispose();
                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                bmp.Dispose();
                ms.Close();
            }
        }

        // 用于检查图像格式
        public static string ImageFormatGuidToString(ImageFormat _format)
        {
            if (_format.Guid == ImageFormat.Bmp.Guid)
            {
                return "bmp";
            }
            else if (_format.Guid == ImageFormat.Gif.Guid)
            {
                return "gif";
            }
            else if (_format.Guid == ImageFormat.Jpeg.Guid)
            {
                return "jpg";
            }
            else if (_format.Guid == ImageFormat.Png.Guid)
            {
                return "png";
            }
            else if (_format.Guid == ImageFormat.Icon.Guid)
            {
                return "ico";
            }
            else if (_format.Guid == ImageFormat.Emf.Guid)
            {
                return "emf";
            }
            else if (_format.Guid == ImageFormat.Exif.Guid)
            {
                return "exif";
            }
            else if (_format.Guid == ImageFormat.Tiff.Guid)
            {
                return "tiff";
            }
            else if (_format.Guid == ImageFormat.Wmf.Guid)
            {
                return "wmf";
            }
            else
            {
                return null;
            }
        }
    }
}
