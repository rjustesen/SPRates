using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SPData
{
    public class Holiday : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("date", IsRequired = true)]
        public DateTime Date
        {
            get { return (DateTime)base["date"]; }
            set { base["date"] = value; }
        }

    }
}
