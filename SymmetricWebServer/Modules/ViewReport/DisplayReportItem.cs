using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WebServer.Database;
using WebServer.Tags;

namespace WebServer.Modules.ViewReport
{
    public class DisplayReportItem
    {
        public string FormHTML { private set; get; }

        public string TemplateHTML { private set; get; }

        public Dictionary<string, object> PhysicalFormValues { private set; get; }

        public ReportItemBase ReportItem { private set; get; }

        public DisplayReportItem(ReportItemBase reportItem,
                                 Dictionary<string, object> physicalFormValues)
        {
            this.ReportItem = reportItem;
            this.PhysicalFormValues = physicalFormValues;
        }

        private void CreateHTML(out string errormessage,
                                ref string html,
                                DBContent.TagValueItemTypes htmlType)
        {
            errormessage = "";
            if (String.IsNullOrWhiteSpace(html)) return;

            int startIndex = -1;
            int endIndex = -1;
            
            while ((startIndex = html.IndexOf(SWBaseTag.TagStart.ToLower(), endIndex + 1, StringComparison.CurrentCultureIgnoreCase)) >= 0)
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
                    tag = SWBaseTag.GetTag(typeAttr.Value);
                }

                if (tag == null) continue;

                XmlAttribute nameAttr = newNode.Attributes[SWBaseTag.NameAttribute];
                if (nameAttr != null)
                {
                    tag.Name = nameAttr.Value;
                }

                string newHTML = this.ProcessTag(tag, htmlType, out errormessage);
                if (newHTML == null)
                {
                    newHTML = "";
                }
                int s = newHTML.Length;
                if (!String.IsNullOrWhiteSpace(errormessage))
                {
                    html = null;
                    return;
                }

                html = html.Substring(0, startIndex) +
                            newHTML +
                            html.Substring(endIndex + 1, html.Length - endIndex - 1);
                int takeaway = endIndex - startIndex + 1;
                startIndex += newHTML.Length - takeaway;
                endIndex += newHTML.Length - takeaway;
            }
        }

        public void CreateTemplate(out string errormessage)
        {
            errormessage = "";
            this.TemplateHTML = null;

            if (this.ReportItem == null ||
                this.ReportItem.TemplateItem == null) return;

            string html = this.ReportItem.TemplateItem.HTML;
            this.CreateHTML(out errormessage, ref html, DBContent.TagValueItemTypes.Template);
            if (!String.IsNullOrWhiteSpace(html))
            {
                this.TemplateHTML = html;
            }
        }

        public void CreateForm(out string errormessage)
        {
            errormessage = "";
            this.FormHTML = null;
            if (this.ReportItem == null ||
                this.ReportItem.FormItem == null) return;

            string html = this.ReportItem.FormItem.HTML;
            this.CreateHTML(out errormessage, ref html, DBContent.TagValueItemTypes.Form);
            if (!String.IsNullOrWhiteSpace(html))
            {
                this.FormHTML = html;
            }
        }

        private string ProcessTag(SWBaseTag tag, DBContent.TagValueItemTypes htmlType, out string errormessage)
        {
            errormessage = "";
            if (tag == null)
            {
                return "";
            }

            switch (tag.TagType)
            {
                case SWInputTextTag.Type:
                    return this.ProcessInputTextTag(tag as SWInputTextTag, htmlType, out errormessage);
                case SWInputDateTag.Type:
                case SWInputTimeTag.Type:
                    return this.ProcessInputDateTimeTag(tag as SWBaseInputDateTimeTag, htmlType, out errormessage);
                case SWDateTag.Type:
                    return DateTime.Now.ToString();
                case SWVarTag.Type:
                    return this.ProcessVarTag(tag as SWVarTag, htmlType, out errormessage);
                case SWQueryTag.Type:
                    return this.ProcessQueryTag(tag as SWQueryTag, out errormessage);
                case SWSelectTag.Type:
                     return this.ProcessSelectTag(tag as SWSelectTag, htmlType, out errormessage);
                
             /*   case SWInputDateTag.Type:
                    switch (htmlType)
                    {
                        case DBContent.TagValueItemTypes.Form:
                            return this.ProcessInputDateTag(foundTag as SWInputDateTag, out errormessage);
                        case DBContent.TagValueItemTypes.Template:
                            return foundTag.Value;
                    }
                    break;*/
            }

            return "";
        }

        private Dictionary<string, object> SetupQueryParameters(SWBaseSQLTag tag)
        {
            Dictionary<string, object> parameters = tag.GetParameters();
            if (this.PhysicalFormValues != null &&
                parameters != null)
            {
                foreach (KeyValuePair<string, object> kvp in this.PhysicalFormValues)
                {
                    string key = "@" + kvp.Key.ToLower();
                    object value = this.PhysicalFormValues[kvp.Key];
                    if (!parameters.ContainsKey(key))
                    {
                        parameters.Add(key, value);
                    }
                    else
                    {
                        parameters[key] = value;
                    }
                }
            }
            return parameters;
        }

        private string ProcessVarTag(SWVarTag tag, DBContent.TagValueItemTypes htmlType, out string errormessage)
        {
            errormessage = "";
            if (tag == null) return "";

            ReadOnlyCollection<SWBaseTag> lst = null;
            switch (htmlType)
            {
                case DBContent.TagValueItemTypes.Form:
                    lst = this.ReportItem.FormTags;
                    break;
                case DBContent.TagValueItemTypes.Template:
                    lst = this.ReportItem.TemplateTags;
                    break;
            }

            tag = (from t in lst
                   where tag.Name.ToLower().Equals(t.Name.ToLower()) &&
                   tag.TagType.Equals(t.TagType)
                   select t as SWVarTag).FirstOrDefault();

            if (tag == null) return "";

            return tag.Value;
        }

        private string ProcessQueryTag(SWQueryTag tag, out string errormessage)
        {
            errormessage = "";
            if (tag == null) return "";

            tag = (from t in this.ReportItem.TemplateTags
                   where tag.Name.ToLower().Equals(t.Name.ToLower()) &&
                   tag.TagType.Equals(t.TagType)
                   select t as SWQueryTag).FirstOrDefault();
            if (tag == null) return "";

            string result = "<table class=\"table table-striped table-bordered table-hover dataTable no-footer\">";
            DbDataReader reader = null;
            DbConnection conn = null;
            try
            {
                reader = ConnectionItem.GetReader(this.ReportItem.ConnectionItem, tag.Value, out errormessage, out conn, this.SetupQueryParameters(tag));
     
                if (reader != null)
                {
                    result += "<thead>";
                    result += "<tr role=\"row\">";
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result += String.Format("<th class=\"sorting_asc\" tabindex=\"0\" rowspan=\"1\" colspan=\"1\">{0}</th>", reader.GetName(i));
                    }
                    result += "</tr>";
                    result += "</thead>";

                    string isSame = "";
                    while (reader.Read())
                    {
                        result += "<tr>";
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            string value = WebUtility.HtmlEncode(reader[i].ToString());
                            if (i == 0)
                            {
                                if (String.Equals(isSame, value))
                                {
                                    value = "";
                                }
                                else
                                {
                                    isSame = value;
                                }
                            }
                            result += String.Format("<td>{0}</td>", value);
                        }
                        result += "</tr>";
                    }
                }
            }
            catch (Exception ex)
            {
                errormessage = ex.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }

                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
            result += "</table>";
            return result;
        }

        private string ProcessInputTextTag(SWInputTextTag tag, DBContent.TagValueItemTypes htmlType, out string errormessage)
        {
            errormessage = "";
            if (tag == null) return "";
            switch (htmlType)
            {
                case DBContent.TagValueItemTypes.Form:
                    string html = String.Format("<input type=\"text\" class=\"form-control\" id = \"i{0}\" name=\"{0}\" />", tag.Name);
                    if (this.PhysicalFormValues.ContainsKey(tag.Name))
                    {
                        html += "<script>$(document).ready(function (){";
                        html += String.Format("$('#i{0}').val('{1}');", tag.Name, this.PhysicalFormValues[tag.Name]);
                        html += "});</script>";
                    }
                    return html;
                case DBContent.TagValueItemTypes.Template:
                    if (this.PhysicalFormValues.ContainsKey(tag.Name))
                    {
                        object value = this.PhysicalFormValues[tag.Name];
                        if (value != null)
                        {
                            return WebUtility.HtmlEncode(value.ToString());
                        }
                    }
                    break;
            }
            return "";
        }

        private string ProcessInputDateTimeTag(SWBaseInputDateTimeTag tag, DBContent.TagValueItemTypes htmlType, out string errormessage)
        {
            errormessage = "";
            if (tag == null) return "";
            switch (htmlType)
            {
                case DBContent.TagValueItemTypes.Form:
                    string html = "";

                    string typealias = "";
                    switch (tag.TagType)
                    {
                        case SWInputDateTag.Type:
                            typealias = "date";
                            break;
                        case SWInputTimeTag.Type:
                            typealias = "time";
                            break;
                    }

                    html += String.Format("<input type=\"{0}\" class=\"form-control\" id = \"i{1}\" name=\"{1}\" />", typealias, tag.Name);
                    if (this.PhysicalFormValues.ContainsKey(tag.Name))
                    {
                        html += "<script>$(document).ready(function (){";
                        html += String.Format("$('#i{0}').val('{1}');", tag.Name, this.PhysicalFormValues[tag.Name]);
                        html += "});</script>";
                    }
                    else
                    {
                        html += @"<script>$(document).ready(function ()
                              {
                                var now = new Date();
	                            var day = ('0' + now.getDate()).slice(-2);
		                        var month = ('0' + (now.getMonth() + 1)).slice(-2);
		                        var today = now.getFullYear()+'-'+(month)+'-'+(day);
                                var todaytime = now.getHours() + ':' + now.getMinutes() + ':' + now.getSeconds();
                                ";

                        switch (tag.TagType)
                        {
                            case SWInputDateTag.Type:
                                html += String.Format("$('#i{0}').val(today);", tag.Name);
                                break;
                            case SWInputTimeTag.Type:
                                html += String.Format("$('#i{0}').val(todaytime);", tag.Name);
                                break;
                        }
                      
                        html += "});</script>";
                    }
                    return html;
                case DBContent.TagValueItemTypes.Template:
                    if (this.PhysicalFormValues.ContainsKey(tag.Name))
                    {
                        object value = this.PhysicalFormValues[tag.Name];
                        if (value != null)
                        {
                            return WebUtility.HtmlEncode(value.ToString());
                        }
                    }
                    break;
            }
            return "";
        }

        private string ProcessSelectTag(SWSelectTag tag, DBContent.TagValueItemTypes htmlType, out string errormessage)
        {
            errormessage = "";

            switch (htmlType)
            {
                case DBContent.TagValueItemTypes.Template:
                    return ProcessSelectTag_Template(tag, out errormessage);
                case DBContent.TagValueItemTypes.Form:
                    return ProcessSelectTag_Form(tag, out errormessage);
            }
            return "";
        }

        private string ProcessSelectTag_Template(SWSelectTag tag, out string errormessage)
        {
            errormessage = "";
            if (tag == null) return "";

            tag = (from t in this.ReportItem.FormTags
                   where tag.Name.ToLower().Equals(t.Name.ToLower()) &&
                   tag.TagType.Equals(t.TagType)
                   select t as SWSelectTag).FirstOrDefault();

            if (tag == null) return "";

            if(!this.PhysicalFormValues.ContainsKey(tag.Name))
            {
                return "";
            }

            DbDataReader reader = null;
            DbConnection conn = null;
            try
            {
                reader = ConnectionItem.GetReader(this.ReportItem.ConnectionItem, tag.Value, out errormessage, out conn, this.SetupQueryParameters(tag as SWBaseSQLTag));
                if (reader != null && reader.FieldCount >= 2)
                {
                    while (reader.Read())
                    {
                        if (reader[0].ToString().Equals(this.PhysicalFormValues[tag.Name]))
                        {
                            return WebUtility.HtmlEncode(reader[1].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errormessage = ex.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }

                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
            return "";
        }

        private string ProcessSelectTag_Form(SWSelectTag tag, out string errormessage)
        {
            errormessage = "";
            if (tag == null) return "";


            tag = (from t in this.ReportItem.FormTags
                    where tag.Name.ToLower().Equals(t.Name.ToLower()) &&
                    tag.TagType.Equals(t.TagType)
                    select t as SWSelectTag).FirstOrDefault();

            if (tag == null) return "";

            string result = String.Format("<select class=\"form-control\" name=\"{0}\" onchange=\"this.form.submit()\">", tag.Name);

            DbDataReader reader = null;
            DbConnection conn = null;
            try
            {
                reader = ConnectionItem.GetReader(this.ReportItem.ConnectionItem, tag.Value, out errormessage, out conn, this.SetupQueryParameters(tag as SWBaseSQLTag));
                if (reader != null && reader.FieldCount >= 2)
                {
                    while (reader.Read())
                    {
                        if (!this.PhysicalFormValues.ContainsKey(tag.Name))
                        {
                            this.PhysicalFormValues.Add(tag.Name, reader[0].ToString());
                        }

                        string selected = "";
                        if (reader[0].ToString().Equals(this.PhysicalFormValues[tag.Name]))
                        {
                            selected = "selected";
                        }
                        result += String.Format("<option {2} value=\"{0}\">{1}</option>",
                                                WebUtility.HtmlEncode(reader[0].ToString()),
                                                WebUtility.HtmlEncode(reader[1].ToString()), selected);
                    }
                }
            }
            catch (Exception ex)
            {
                errormessage = ex.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }

                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
            result += "</select>";
            return result;
        }
    }
}
