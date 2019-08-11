using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TimewaxSync.TimewaxWebApi
{
    [XmlRoot(ElementName = "response")]
    public class TokenObject
    {

        [XmlElement(ElementName = "token")]
        public string Token { get; set; }

        [XmlElement(ElementName ="validUntil")]
        public string ValidUntil { get; set; }

    }
}
