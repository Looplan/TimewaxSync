using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TimewaxSync
{
    [XmlRoot(ElementName = "entry")]
    public class CalendarEntry
    {
        [XmlElement(ElementName = "id")]
        public string ID { get; set; }

        [XmlElement(ElementName = "date")]
        public string Date { get; set; }

        [XmlElement(ElementName = "resourceName")]
        public string ResourceName { get; set; }

        [XmlElement(ElementName ="project")]
        public string Project { get; set; }

        [XmlElement(ElementName ="breakdown")]
        public string Breakdown { get; set; }

        [XmlElement(ElementName ="breakdownCode")]
        public string BreakdownCode { get; set; }

        [XmlElement(ElementName = "breakdownParent")]
        public string BreakdownParent { get; set; }

        [XmlElement(ElementName = "timeFrom")]
        public string TimeFrom { get; set; }

        [XmlElement(ElementName = "timeTo")]
        public string TimeTo { get; set; }
    }
}
