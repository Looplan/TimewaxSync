using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TimewaxSync
{
    [XmlRoot(ElementName = "breakdown")]
    public class Breakdown
    {
        [XmlElement(ElementName = "id")]
        public string ID { get; set; }

        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }




    }
}
