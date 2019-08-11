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

        [XmlElement(ElementName = "parentBreakdown")]
        public string ParentID { get; set; }

        [XmlElement(ElementName = "parentBreakdownCode")]
        public string ParentCode { get; set; }

        [XmlElement(ElementName = "parentBreakdownName")]
        public string ParentName { get; set; }

        [XmlElement(ElementName = "status")]
        public string Status { get; set; }
    }
}
