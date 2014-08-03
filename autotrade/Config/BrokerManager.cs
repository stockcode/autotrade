using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace autotrade.Config
{

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class BrokerManager
    {

        private BrokerManagerNetworkInfo[] networkInfoField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NetworkInfo")]
        public BrokerManagerNetworkInfo[] NetworkInfo
        {
            get
            {
                return this.networkInfoField;
            }
            set
            {
                this.networkInfoField = value;
            }
        }

        public void SetHotBroker(string selectedText)
        {
            foreach (var broker in NetworkInfo)
            {
                broker.isHot = broker.name == selectedText ? 1 : 0;
            }
        }

        public void SaveToFile(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();   //Represents an XML document, 
            // Initializes a new instance of the XmlDocument class.          
            XmlSerializer xmlSerializer = new XmlSerializer(GetType());
            // Creates a stream whose backing store is memory. 
            using (MemoryStream xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, this);
                xmlStream.Position = 0;
                //Loads the XML document from the specified string.
                xmlDoc.Load(xmlStream);
                string xml = xmlDoc.InnerXml;
                File.WriteAllText(filePath, xml);
            }
        }

        public BrokerManagerNetworkInfo GetHotBroker()
        {
            foreach (var broker in NetworkInfo)
            {
                if (broker.isHot == 1) return broker;
            }
            return null;
        }

        public string GetHotMarketUrl()
        {
            var broker = GetHotBroker();
            var netline = broker.NetLine.First(line => line.type == "MD" && line.isHot == 1);
            return "tcp://" + netline.ip + ":" + netline.port;
        }

        public string GetHotTradeUrl()
        {
            var broker = GetHotBroker();
            var netline = broker.NetLine.First(line => line.type == "TD" && line.isHot == 1);
            return "tcp://" + netline.ip + ":" + netline.port;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class BrokerManagerNetworkInfo
    {

        private BrokerManagerNetworkInfoNetLine[] netLineField;

        private string nameField;

        private string typeField;

        private ushort brokerIdField;

        private int isHotField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NetLine")]
        public BrokerManagerNetworkInfoNetLine[] NetLine
        {
            get
            {
                return this.netLineField;
            }
            set
            {
                this.netLineField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort BrokerId
        {
            get
            {
                return this.brokerIdField;
            }
            set
            {
                this.brokerIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int isHot
        {
            get
            {
                return this.isHotField;
            }
            set
            {
                this.isHotField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class BrokerManagerNetworkInfoNetLine
    {

        private string ipField;

        private ushort portField;

        private string typeField;

        private byte isHotField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ip
        {
            get
            {
                return this.ipField;
            }
            set
            {
                this.ipField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort port
        {
            get
            {
                return this.portField;
            }
            set
            {
                this.portField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte isHot
        {
            get
            {
                return this.isHotField;
            }
            set
            {
                this.isHotField = value;
            }
        }
    }


}
