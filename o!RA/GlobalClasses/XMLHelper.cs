using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace o_RA.GlobalClasses
{
    public class XMLHelper
    {
        public static T DeSerialize<T>(string xmlFile)
        {
            using (FileStream localeStream = new FileStream(xmlFile, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T newObject = (T)serializer.Deserialize(localeStream);
                return newObject;
            }
        }

        public static void Serialize<T>(string xmlFile, T data)
        {
            using (FileStream localeStream = new FileStream(xmlFile, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(localeStream, data);
            }
        }
    }

    [Serializable]
    public class LocaleElement
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    [Serializable]
    public class Locale
    {
        [XmlArray]
        [XmlArrayItem(ElementName = "LanguageElement")]
        public List<LocaleElement> Features { get; set; }
    }
}
