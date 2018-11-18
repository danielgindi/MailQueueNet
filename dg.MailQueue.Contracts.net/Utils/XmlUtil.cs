using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dg.MailQueue.Contracts.Utils
{
    internal static class XmlUtil
    {
        static public XmlNode GetConfigNode(XmlDocument xml, string nodePath)
        {
            if (!string.IsNullOrEmpty(xml.DocumentElement.NamespaceURI))
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ms", xml.DocumentElement.NamespaceURI);
                return xml.SelectSingleNode("ms:" + xml.DocumentElement.Name + "/ms:" + nodePath.Replace("/", "/ms:"), nsmgr);
            }
            else
            {
                return xml.SelectSingleNode(xml.DocumentElement.Name + "/" + nodePath);
            }
        }
    }
}
