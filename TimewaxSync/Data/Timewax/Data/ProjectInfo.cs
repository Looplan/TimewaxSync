using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TimewaxSync.TimewaxWebApi.Data
{
    [XmlRoot(ElementName = "project")]
    public class ProjectInfo
    {
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "shortname")]
        public string Shortname { get; set; }

    }
}
