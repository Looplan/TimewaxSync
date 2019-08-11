using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TimewaxSync.TimewaxWebApi
{
    [XmlRoot(ElementName = "authenticator")]
    public class Authenticator
    {
        [XmlElement(ElementName = "client")]
        public string Client { get; set; }

        [XmlElement(ElementName = "username")]
        public string Username { get; set; }

        [XmlElement(ElementName = "password")]
        public string Password { get; set; }
    }
}
