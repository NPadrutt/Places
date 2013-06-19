using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.ComponentModel;
using MyTravelHistory.Src;

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
    }
}
