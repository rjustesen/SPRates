using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using SPRates;
using log4net;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;


namespace SPData
{
    public class Processor
    {
        private enum RunResults
        {
            Success,
            BadDate,
            NoFile,
        }


        private static ILog log = LogManager.GetLogger(typeof(Processor));

        private static NameValueCollection holidays;

       
        public static void ProcessRates()
        {
            holidays = ConfigurationManager.GetSection("Holidays") as NameValueCollection;
            string url = ConfigurationManager.AppSettings["url"] as string; 
            string workArea = ConfigurationManager.AppSettings["lifePROWorkarea"] as string; 
            string emailAddress = ConfigurationManager.AppSettings["emailNotifcations"] as string;
            string retrivalTime = ConfigurationManager.AppSettings["retrivalTime"] as string;
            string outputFile = ConfigurationManager.AppSettings["outputFile"] as string;
            string goodFile = ConfigurationManager.AppSettings["goodFile"] as string; 
            string cmdPath = ConfigurationManager.AppSettings["cmdPath"] as string; 
            string cmd = ConfigurationManager.AppSettings["cmd"] as string; 
            string args = ConfigurationManager.AppSettings["args"] as string; 

            string retVal; 
            string retTime;

            DateTime startTime = Convert.ToDateTime(retrivalTime);
          
            double millisecondsToWait = (startTime - DateTime.Now).TotalMilliseconds;

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(millisecondsToWait))
                {
                    // Do nothing until stopwatch stops...
                    //log.Debug("Waiting..");
                }
                stopwatch.Stop();
            }
            catch (Exception e)
            {
                log.Error(e);
                SendMail(emailAddress, "SPProcessor@benlife.com", "Error getting Standard and Poors closing index",
                                           "Error while wiating for closing time of " + retrivalTime);
                return;
            }


            if (!IsHoliday())
            {
                if (File.Exists(Path.Combine(workArea, goodFile)))
                {
                    File.Delete(Path.Combine(workArea, goodFile));
                }
                if (ScreenScraper.GetRate(url, out retVal, out retTime))
                {
                    WriteOutputFile(Path.Combine(workArea, outputFile), retVal);
                    RunResults result = CallLifepro(Path.Combine(cmdPath, cmd), args, Path.Combine(workArea, goodFile));
                    switch (result)
                    {
                        case RunResults.Success:
                            SendMail(emailAddress, "SPProcessor@benlife.com", "Successful call to get Standard and Poors closing index",
                                    "Standard and Poors Closing index value sucessfully retrieved and LifePRO program called at: " + DateTime.Now.ToShortDateString() + "  " + DateTime.Now.ToShortTimeString());
                            break;
                        case RunResults.BadDate:
                            log.Error("SANDP.GOOD was present but had the wrong date present. This indicates that the LifePRO program  did not run. Rates will need to be entered manually.");
                            SendMail(emailAddress, "SPProcessor@benlife.com", "Error getting Standard and Poors closing index",
                                        "SANDP.GOOD was present but had the wrong date present. This indicates that the LifePRO program  did not run. Rates will need to be entered manually.");
                            break;
                        case RunResults.NoFile:
                            log.Error("No SANDP.GOOD FILE Found. The LifePRO program must not have run. Rates need to be entered manually.");
                            SendMail(emailAddress, "SPProcessor@benlife.com", "Error getting Standard and Poors closing index",
                                            "No SANDP.GOOD FILE Found. The LifePRO program must not have run. Rates need to be entered manually.");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    log.Error(retVal);
                    SendMail(emailAddress, "SPProcessor@benlife.com", "Error getting Standard and Poors closing index",
                                            "There was an error scraping the screen for S & P index rates. The error is " + retVal);
                }

            }
        }

        /// <summary>
        /// If today is a holiday then do no run 
        /// </summary>
        /// <returns></returns>
        private static bool IsHoliday()
		{
            DateTime dtNow = DateTime.Today; //new DateTime(2013,07,04); 
            
            var pairs = from key in holidays.Cast<String>()
                            from value in holidays.GetValues(key)
                            select new { key, value };

            var dt = pairs.FirstOrDefault(x => x.value.Equals(dtNow.ToString("MM/dd/yyyy")));
            // if dt is not null then today is a holiday
            return (null != dt);
        }

        private static void WriteOutputFile(string outputFile, string retValue)
        {
            try
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                    waitSeconds(2);
                }
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    writer.WriteLine(DateTime.Today.ToString("yyyyMMdd") + retValue.Replace(",", ""));
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        /// <summary>
        /// Sleep for a bit
        /// </summary>
        /// <param name="secondsToWait"></param>
        private static void waitSeconds(int secondsToWait)
        {
            Thread.Sleep(secondsToWait * 1000);
        }

        /// <summary>
        /// Call lifePRO
        /// </summary>
        /// <returns></returns>
        private static RunResults CallLifepro(string command, string args, string goodFile)
        {
            RunResults result = RunResults.Success;
            Process process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = GetCycleCoder() +  args;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
            waitSeconds(10);  //give external program time to finish

            if (File.Exists(goodFile))
            {
                using (TextReader reader = File.OpenText(goodFile))
                {
                    string line = reader.ReadLine();
                    line = line.Substring(4, 2) + "/" + line.Substring(6, 2) + "/" + line.Substring(0, 4);
                    DateTime dateTimeOfSandPGood = DateTime.ParseExact(line, "MM/dd/yyyy", null);
                    reader.Close();
                    DateTime today = DateTime.Today;
                    if (dateTimeOfSandPGood.Year != today.Year || dateTimeOfSandPGood.Month != today.Month || dateTimeOfSandPGood.Day != today.Day)
                    {
                        result = RunResults.BadDate;
                    }
                }
            }
            else
            {
                result = RunResults.NoFile;
            }
            return result;
        }


        /// <summary>
        /// Send out an email
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private static void SendMail(string to, string from, string subject, string body)
        {
            try
            {

                using (MailMessage message = new MailMessage())
                {
                    MailAddress fromAddress = new MailAddress(from, from);
                    message.From = fromAddress;

                    string[] split = to.Split(';');
                    foreach (string s in split)
                    {
                        message.To.Add(s);
                    }
                    
                    message.Subject = subject;
                    message.IsBodyHtml = true;
                    message.Body = body;
                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Send(message);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private static string GetCycleCoder()
        {
            string coder = null;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday: 
                case DayOfWeek.Saturday:
                    coder = "CYCW";
                    break;
                case DayOfWeek.Monday:
                    coder = "CYC1";
                    break;
                case DayOfWeek.Tuesday:
                    coder = "CYC2";
                    break;
                case DayOfWeek.Wednesday:
                    coder = "CYC3";
                    break;
                case DayOfWeek.Thursday:
                    coder = "CYC4";
                    break;
                case DayOfWeek.Friday:
                    coder= "CYC5";
                    break;
                default:   
                    coder= "CYCW";
                    break;
            }
            return coder;
        }

    }
}
