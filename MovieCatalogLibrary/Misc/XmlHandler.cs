using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MovieCatalogLibrary.Misc
{
    static class XmlHandler
    {

        /// <summary>
        /// Returns the default location of the database based on the Config.xml file.
        /// </summary>
        /// <returns></returns>
        public static string ReturnDefaultDB()
        {
            XDocument doc = XDocument.Load("Config.xml");

            return (from a in doc.Descendants("defaultConnection")
                           select new
                           {
                               Server = a.Attribute("Server").Value
                           }).FirstOrDefault().Server.ToString();
        }

    }
}
