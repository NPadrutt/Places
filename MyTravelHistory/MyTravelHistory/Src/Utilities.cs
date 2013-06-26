using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MyTravelHistory.Src
{
    public class Utilities
    {
        public static string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
        }

        public static byte[] ConvertToBytes(WriteableBitmap image)
        {
            MemoryStream ms = new MemoryStream();
            image.SaveJpeg(ms, 1000, 2000, 0, 100);

            return ms.GetBuffer();
        }

        public static WriteableBitmap ConvertToImage(byte[] inputBytes)
        {
            return GetImage(inputBytes, 1000, 2000);
        }

        public static WriteableBitmap ConvertToImage(byte[] inputBytes, int width, int height)
        {
            return GetImage(inputBytes, width, height);
        }

        private static WriteableBitmap GetImage(byte[] inputBytes, int width, int height)
        {
            WriteableBitmap img = new WriteableBitmap(width, height);

            var ms = new MemoryStream(inputBytes);
            img.LoadJpeg(ms);

            return img;
        }
    }
}
