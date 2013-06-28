using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.ComponentModel;
using MyTravelHistory.Src;
using System.Threading;
using System.Windows.Media.Imaging;

namespace MyTravelHistoryTests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        [Description("Make sure that the version isn't null")]
        public void GetVersionTest()
        {
            Assert.IsNotNull(Utilities.GetVersion());
        }

        [TestMethod]
        [Description("Check if return value is a byte array to be sure the image can be saved properly")]
        public void ConvertToBytesTest()
        {
            WriteableBitmap bmp = null;
            using (var are = new AutoResetEvent(false))
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    bmp = new WriteableBitmap(10, 10);
                    are.Set();
                });
                are.WaitOne();
            }

            Assert.IsInstanceOfType(Utilities.ConvertToBytes(bmp), typeof(byte[]), "return value has to be a byte array");
        }

    }
}
