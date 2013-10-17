using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using log4net;

namespace SPData
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            ILog log = LogManager.GetLogger(typeof(Program));
            log.Info("Start Program");
            Processor.ProcessRates();
            log.Info("End Program");
        }
    }
}
