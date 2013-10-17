using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPRates;

namespace SandPTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string retValue;
            string retTime;
            ScreenScraper.GetRate("http://www.bloomberg.com/markets/stocks/world-indexes/", out retValue, out retTime);
            Console.WriteLine(retValue);
            Console.WriteLine(retTime);
        }
    }
}
