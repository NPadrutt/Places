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
            WriteableBitmap img = new WriteableBitmap(1000, 2000);

            var ms = new MemoryStream(inputBytes);
            img.LoadJpeg(ms);

            return img;
        }
    }
}
