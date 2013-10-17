///------------------------------------------------------------------------
// Copyright 2013 Beneficial Financial Group
// This program is an unpublished work fully protected by the United      
// States Copyright laws and is considered a trade secret belonging to   
// the copyright holder -- Beneficial Life Insurance Company
//------------------------------------------------------------------------
#region History
/*
*  SR#              INIT   DATE        DESCRIPTION
*  -----------------------------------------------------------------------
*                   RTJ   02/20/13   Original development
*/
#endregion
#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using HtmlAgilityPack;

#endregion
namespace SPRates
{

    public class ScreenScraper
    {

          public static bool GetRate(string url, out string retValue, out string retTime)
          {
              try
              {
                  HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                  HtmlAgilityPack.HtmlDocument doc = web.Load(url);


                  var query = from table in doc.DocumentNode.SelectNodes("//table")
                              from row in table.SelectNodes("//tr")
                              from cell in row.SelectNodes("//td")
                              select cell.InnerText;

                  List<string> list = query.ToList<string>();
                  var val = list.FirstOrDefault(x => x.Contains("S&P 500 Index"));
                  if (null != val)
                  {
                      int index = list.IndexOf(val) + 1;
                      retValue = list[index];
                      //<td class="value_change down">-18.99</td> =   index + 2
                      //<td class="percent_change down">-1.24%</td> = index + 3
                      //<td class='time last'>16:38:59</td> = index + 4
                      retTime = list[index + 3];
                  }
                  else
                  {
                      retValue = "Could not find S&P 500 Index rate in HTML stream";
                      retTime = null;
                      return false;
                  }
              }
              catch (Exception e)
              {
                  retValue = e.Message;
                  retTime = null;
                  return false;
              }
              return true;
          }
    }
}
