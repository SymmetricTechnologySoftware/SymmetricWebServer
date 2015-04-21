using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.ComponentModel;

namespace WebServer
{
	public static class XmlWrapper
	{
		private const string Variables = "Variables";

		private static XmlDocument CheckFile(string filename)
		{
			XmlDocument doc = null;
			if (!File.Exists(filename))
			{
				doc = new XmlDocument();
				doc.LoadXml(String.Format("<{0}></{0}>", Variables));
				doc.Save(filename);
			}
			else
			{
				doc = new XmlDocument();
				try
				{
					doc.Load(filename);
				}
				catch(Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("Please manually remove: " + filename + " - " + ex.Message);//bbb, debug logger.
					return null;
				}
			}

			XmlNode root = doc.DocumentElement;
			if (root == null)
			{
				doc.CreateElement(Variables);
			}
			return doc;
		}

		private static void RemoveNodes(XmlNode root, string name)
		{
			foreach (XmlNode node in root.SelectNodes(name))
			{
				root.RemoveChild(node);
			}
		}

		public static void WriteVariable(string filename, string name, object value)
		{
			if (String.IsNullOrWhiteSpace(name)) return;

			XmlDocument doc = CheckFile(filename);
			if (doc == null) return;
			RemoveNodes(doc.DocumentElement, name);

			XmlElement elem = doc.CreateElement(name);
			if (value != null)
			{
				elem.InnerText = value.ToString();
			}
			doc.DocumentElement.AppendChild(elem);
			doc.Save(filename);
		}

		public static void WriteVariable(string filename, string name, List<object> values)
		{
			if (String.IsNullOrWhiteSpace(name)) return;

			XmlDocument doc = CheckFile(filename);
			if (doc == null) return;
			RemoveNodes(doc.DocumentElement, name);


			foreach (object value in values)
			{
				XmlElement elem = doc.CreateElement(name);
				if (value != null)
				{
					elem.InnerText = value.ToString();
				}
				doc.DocumentElement.AppendChild(elem);
			}
			doc.Save(filename);
		}

		public static T ReadVariable<T>(string filename, string name, T defaultValue)
		{
			if (String.IsNullOrWhiteSpace(name)) return defaultValue;

			XmlDocument doc = CheckFile(filename);
			if (doc == null) return defaultValue;
			XmlNodeList lst = doc.DocumentElement.SelectNodes(name);

			List<string> result = new List<string>();
			foreach (XmlNode node in lst)
			{
				result.Add(node.InnerText);
			}
			if (result.Count <= 0) return defaultValue;

			var converter = TypeDescriptor.GetConverter(typeof(T));
			try
			{
				return (T)converter.ConvertFrom(result[0]);
			}
			catch
			{
				return defaultValue;
			}

		}

		public static List<string> ReadVariable(string filename, string name)
		{
			if (String.IsNullOrWhiteSpace(name)) return null;

			XmlDocument doc = CheckFile(filename);
			if (doc == null) return null;
			XmlNodeList lst = doc.DocumentElement.SelectNodes(name);

			List<string> result = new List<string>();
			foreach(XmlNode node in lst)
			{
				result.Add(node.InnerText);
			}
			return result;
		}

	}
}

