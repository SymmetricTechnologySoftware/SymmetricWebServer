using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WebServer.Tags
{
    public abstract class SWBaseTag
    {
        public enum BaseTagTypes { Form, Template, Both }
        
        public const string TagStart = "<sws";
        public const string TagEnd = ">";
        public const string TypeAttribute = "type";
        public const string NameAttribute = "name";

        public BaseTagTypes BaseTagType { private set; get; }
        public string TagType { private set; get; }
        public string FormattedHTML { protected set; get; }
        private string _value;
        public string Value 
        {
            set
            {
                if (value == null)
                {
                    value = "";
                }
                _value = value;
            }
            get
            {
                return _value;
            }
        }

        private string _name;
        public string Name 
        {
            set
            {
                if (value == null) value = "";
                _name = value.ToLower();
            }
            get
            {
                return _name;
            }
        }
        public XmlAttributeCollection Attributes { private set; get; }

        protected SWBaseTag(BaseTagTypes baseTagType, string tagType)
        {
            this._value = "";
            this._name = "";
            this.BaseTagType = baseTagType;
            this.TagType = tagType.ToLower();
        }

        public static SWBaseTag GetTag(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) return null;
            type = type.ToLower();

            SWBaseTag tag = null;
            switch (type)
            {
                case SWInputTextTag.Type:
                    tag = new SWInputTextTag();
                    break;
                case SWDateTag.Type:
                    tag = new SWDateTag();
                    break;
                case SWVarTag.Type:
                    tag = new SWVarTag();
                    break;
                case SWQueryTag.Type:
                    tag = new SWQueryTag();
                    break;
                case SWSelectTag.Type:
                    tag = new SWSelectTag();
                    break;
                case SWInputDateTag.Type:
                    tag = new SWInputDateTag();
                    break;
                case SWInputTimeTag.Type:
                    tag = new SWInputTimeTag();
                    break;
            }
            return tag;
        }

        public static List<SWBaseTag> GetTags(string html, BaseTagTypes baseTagType)
        {
            if (String.IsNullOrWhiteSpace(html)) return new List<SWBaseTag>();

            List<SWBaseTag> result = new List<SWBaseTag>();

            int startIndex = -1;
            int endIndex = -1;

            html = html.ToLower();

            while ((startIndex = html.IndexOf(SWBaseTag.TagStart.ToLower(), endIndex + 1)) >= 0)
            {
                endIndex = html.IndexOf(SWBaseTag.TagEnd.ToLower(), startIndex);
                int tempStart = html.IndexOf(SWBaseTag.TagStart.ToLower());
                if (tempStart > endIndex) continue;

                string xmlTag = html.Substring(startIndex, endIndex - startIndex + 1);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlTag.ToLower());
                XmlNode newNode = doc.DocumentElement;

                SWBaseTag tag = null;
                XmlAttribute typeAttr = newNode.Attributes[SWBaseTag.TypeAttribute];
                if (typeAttr != null)
                {
                    tag = GetTag(typeAttr.Value);
                }

                if (tag == null) continue;

                XmlAttribute nameAttr = newNode.Attributes[SWBaseTag.NameAttribute];
                if (nameAttr != null)
                {
                    tag.Name = nameAttr.Value;
                }

                tag.Attributes = newNode.Attributes;

                if (tag != null &&
                    tag.BaseTagType == baseTagType || 
                    tag.BaseTagType == BaseTagTypes.Both)
                {
                    result.Add(tag);
                }
            }

            return result;
        }
    }
}
