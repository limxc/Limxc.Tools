//
// C# (cross-platform)
// IniSaved
// v 0.2, 27.06.2023
// https://github.com/dkxce/INISaved
// en,ru,1251,utf-8
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Limxc.Tools.Core.Dependencies
{
    /// <summary>
    ///     Ini Serializer (XML <--> INI)
    ///     Supports ; & # (using XMLSerializer)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class IniSaved<T>
    {
        public static bool presaveFileHeader = false;
        public static bool presaveXmlSerialization = false;
        public static string customHeaderLine = null;

        #region SAVE

        public static void SaveHere(string file, T obj, string section = null)
        {
            Save(Path.Combine(CurrentDirectory(), file), obj, section);
        }

        public static void Save(string file, Type type, object obj, string section = null)
        {
            using (var sw = new StreamWriter(file, false, Encoding.UTF8))
            {
                var chl = string.IsNullOrEmpty(customHeaderLine) ? "" : $";{customHeaderLine}\r\n";
                if (presaveFileHeader)
                    sw.WriteLine(
                        $";\r\n;IniSaved File UTF-8\r\n{chl};[section]\r\n;@attr|param=value\r\n;@ -> \\u0040, ; -> \\u003B, # -> \\u0023, \\r -> \\u000D, \\n -> \\u000A\r\n;\r\n");
                sw.Write(Save(type, obj, section));
            }

            ;
        }

        public static void Save(string file, T obj, string section = null)
        {
            Save(file, typeof(T), obj, section);
        }

        public static void Save(StreamWriter file, T obj, string section = null)
        {
            file.Write(Save(typeof(T), obj, section));
        }

        public static string Save(T obj, string section = null)
        {
            return Save(typeof(T), obj, section);
        }

        public static string Save(Type type, object obj, string section = null)
        {
            if (string.IsNullOrEmpty(section))
            {
                section = typeof(T).Name;
                if (type.IsArray) section = $"ArrayOf{section.Replace("[]", "")}";
                var ina = typeof(T).GetCustomAttribute<IniSectionAttribute>();
                if (ina != null && !string.IsNullOrEmpty(ina.name)) section = ina.name;
            }

            ;

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var xs = new XmlSerializer(typeof(T));
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            xs.Serialize(writer, obj, ns);
            writer.Flush();
            ms.Position = 0;
            var bb = new byte[ms.Length];
            ms.Read(bb, 0, bb.Length);
            writer.Close();
            var xml = Encoding.UTF8.GetString(bb);

            // TEST //
            /*
            XmlSerializer xxs = new XmlSerializer(typeof(T));
            StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
            T c = (T)xxs.Deserialize(reader);
            reader.Close();
            */

            // XML -> INI
            var xd = new XmlDocument();
            xd.LoadXml(xml);
            var sections = new Dictionary<string, int>();
            var res = GetIni(xd.DocumentElement, section, sections);
            if (presaveXmlSerialization)
                res = ";" + xml.Replace("\r\n", "\r\n;") + "\r\n;\r\n\r\n" + res;
            return res;
        }

        private static string GetIni(XmlNode xn, string section, Dictionary<string, int> sections)
        {
            var hasRoot = false;
            var hasBody = false;

            section = NormalizeValue(section);
            if (sections.ContainsKey(section)) section = $"{section}.{sections[section]++}";
            else sections.Add(section, 1);

            var sb = new StringBuilder();

            if (xn.Attributes != null)
                foreach (XmlAttribute a in xn.Attributes)
                {
                    AddRoot(ref hasRoot, sb, section);
                    sb.Append($"@{NormalizeValue(a.Name, true)}={NormalizeValue(a.Value)}\r\n");
                    hasBody = true;
                }

            ;

            var simpleNodes = new List<XmlNode>();
            var detailNodes = new List<XmlNode>();

            if (xn.HasChildNodes)
                foreach (XmlNode n in xn.ChildNodes)
                    if (n.NodeType == XmlNodeType.Text)
                    {
                        AddRoot(ref hasRoot, sb, section);
                        sb.Append($"@={NormalizeValue(n.Value)}\r\n");
                        hasBody = true;
                    }
                    else if (n.HasChildNodes && n.ChildNodes.Count == 1 &&
                             n.ChildNodes[0].NodeType == XmlNodeType.Text &&
                             (n.Attributes == null || n.Attributes.Count == 0))
                    {
                        simpleNodes.Add(n);
                    }
                    else
                    {
                        detailNodes.Add(n);
                    }

            ;

            foreach (var n in simpleNodes)
            {
                AddRoot(ref hasRoot, sb, section);
                sb.Append($"{NormalizeValue(n.Name, true)}={NormalizeValue(n.ChildNodes[0].Value)}\r\n");
                hasBody = true;
            }

            ;

            if (simpleNodes.Count > 0) sb.Append("\r\n");

            foreach (var n in detailNodes)
            {
                var sn = $"{section}.{NormalizeValue(n.Name)}";
                var dnt = GetIni(n, sn, sections);
                sb.Append(dnt);
                hasBody = true;
            }

            ;

            if (hasBody) sb.Append("\r\n");
            return sb.ToString();
        }

        private static void AddRoot(ref bool hasRoot, StringBuilder sb, string section)
        {
            if (!hasRoot)
            {
                sb.Append($"[{section}]\r\n");
                hasRoot = true;
            }

            ;
        }

        #endregion SAVE

        #region LOAD

        public static void Load(StreamReader sr, ref T obj, string section = null)
        {
            object v = obj;
            Load(sr, typeof(T), ref v, section);
            obj = (T)v;
        }

        public static T LoadHere(string file, string section = null)
        {
            return Load(Path.Combine(CurrentDirectory(), file, section));
        }

        public static T LoadFromText(string text, string section = null)
        {
            var t = typeof(T);
            T obj;
            if (IsSimple(t))
            {
                obj = t == typeof(string)
                    ? (T)Convert.ChangeType(null, typeof(T))
                    : (T)Activator.CreateInstance(typeof(T));
            }
            else if (t.IsArray)
            {
                obj = (T)Activator.CreateInstance(typeof(T), 0);
            }
            else
            {
                var c = t.GetConstructor(new Type[0]);
                obj = (T)c.Invoke(null);
            }

            ;
            using (var fs = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                Load(new StreamReader(fs), ref obj, section);
            }

            return obj;
        }

        public static T Load(string file, string section = null)
        {
            var t = typeof(T);
            T obj;
            if (IsSimple(t))
            {
                obj = t == typeof(string)
                    ? (T)Convert.ChangeType(null, typeof(T))
                    : (T)Activator.CreateInstance(typeof(T));
            }
            else if (t.IsArray)
            {
                obj = (T)Activator.CreateInstance(typeof(T), 0);
            }
            else
            {
                var c = t.GetConstructor(new Type[0]);
                obj = (T)c.Invoke(null);
            }

            ;
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                Load(new StreamReader(fs), ref obj, section);
            }

            return obj;
        }

        public static void Load(StreamReader sr, Type type, ref object obj, string section = null)
        {
            var xmlbase = "";
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                var xxs = new XmlSerializer(typeof(T));
                var ms = new MemoryStream();
                var writer = new StreamWriter(ms);
                xxs.Serialize(writer, obj, ns);
                writer.Flush();
                ms.Position = 0;
                var bb = new byte[ms.Length];
                ms.Read(bb, 0, bb.Length);
                writer.Close();
                var xxd = new XmlDocument();
                xxd.LoadXml(Encoding.UTF8.GetString(bb));
                xmlbase = xxd.DocumentElement.Name;
            }
            ;

            if (string.IsNullOrEmpty(section))
            {
                section = typeof(T).Name;
                if (type.IsArray) section = $"ArrayOf{section.Replace("[]", "")}";
                var ina = typeof(T).GetCustomAttribute<IniSectionAttribute>();
                if (ina != null && !string.IsNullOrEmpty(ina.name)) section = ina.name;
            }

            ;

            // READ INI SECTIONS
            List<KeyValuePair<string, List<string>>> secSorted = null;
            {
                var currsect = "";
                var sections = new Dictionary<string, List<string>>();
                while (!sr.EndOfStream)
                {
                    var ln = sr.ReadLine().Trim();
                    if (string.IsNullOrEmpty(ln) || ln.StartsWith("#") || ln.StartsWith(";")) continue;
                    if (ln.StartsWith("["))
                    {
                        currsect = ReversizeValue(ln.Split('#', ';')[0].Trim('[', ']'));
                        sections.Add(currsect, new List<string>());
                        continue;
                    }

                    ;
                    if (string.IsNullOrEmpty(currsect)) continue;
                    if (ln.IndexOf("=") < 0) continue;
                    ln = ln.Split(';', '#')[0];
                    sections[currsect].Add(ln);
                }

                ;
                // SORT
                secSorted = new List<KeyValuePair<string, List<string>>>(sections);
                secSorted.Sort(new CustomComparer());
                // REPLACE ROOT
                if (secSorted.Count > 0 && !string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(xmlbase) &&
                    section != xmlbase)
                {
                    var tmps = new List<KeyValuePair<string, List<string>>>();
                    foreach (var kvp in secSorted)
                        if (kvp.Key == section || kvp.Key.StartsWith($"{section}."))
                        {
                            var kName = kvp.Key == section
                                ? xmlbase
                                : xmlbase + "." + kvp.Key.Substring(section.Length + 1);
                            tmps.Add(new KeyValuePair<string, List<string>>(kName, kvp.Value));
                        }
                        else
                        {
                            tmps.Add(kvp);
                        }

                    ;
                    secSorted = tmps;
                }

                ;
            }
            ;

            // INI -> XML
            var xd = new XmlDocument();
            var xml_declaration = xd.CreateXmlDeclaration("1.0", "utf-8", null);
            xd.InsertBefore(xml_declaration, xd.DocumentElement);

            foreach (var s in secSorted)
            {
                var fullKey = s.Key;
                var shrtKey = s.Key;

                // Normalize Key (strip .\d$)
                {
                    var iof = fullKey.LastIndexOf(".");
                    if (iof > 0 && int.TryParse(fullKey.Substring(iof + 1), out _))
                        fullKey = fullKey.Substring(0, iof);
                }
                ;

                // Find/Create Path
                XmlNode thisRoot = xd;
                if (fullKey.Contains("."))
                {
                    var paths = fullKey.Split('.');
                    for (var i = 0; i < paths.Length - 1; i++)
                    {
                        var cpath = NormalizeValue(paths[i]);
                        var pos = -1;
                        if (!int.TryParse(cpath, out pos)) pos = -1;
                        XmlNode sn = null;
                        if (pos < 0)
                        {
                            sn = thisRoot.SelectSingleNode(cpath + "[last()]");
                        }
                        else
                        {
                            cpath = thisRoot.Name;
                            thisRoot = thisRoot.ParentNode;
                            sn = thisRoot.SelectSingleNode(cpath + $"[{pos + 1}]");
                        }

                        ;
                        if (sn == null)
                            sn = thisRoot.AppendChild(xd.CreateElement(cpath));
                        thisRoot = sn;
                    }

                    ;
                    shrtKey = paths[paths.Length - 1];
                }

                ;

                // Create Node
                var xn = xd.CreateNode(XmlNodeType.Element, null, shrtKey, null);
                thisRoot = thisRoot.AppendChild(xn);

                // Fill Node
                var attrs = new List<KeyValuePair<string, string>>();
                var vars = new List<KeyValuePair<string, string>>();
                foreach (var line in s.Value)
                {
                    var name_value = line.Split(new[] { '=' }, 2);
                    var name = ReversizeValue(name_value[0].Trim(), true);
                    var value = ReversizeValue(name_value[1].Trim());

                    if (name == "@")
                    {
                        var t = xd.CreateTextNode(value);
                        xn.AppendChild(t);
                    }
                    else if (name.StartsWith("@"))
                    {
                        var attr = name.Substring(1);
                        if (!attr.StartsWith("xmlns:") && attr.Contains(":"))
                        {
                            var attrdel = attr.Split(':');
                            var a = xd.CreateAttribute(attrdel[0], attrdel[1],
                                "http://www.w3.org/2001/XMLSchema-instance");
                            a.Value = value;
                            (xn as XmlElement).SetAttributeNode(a);
                        }
                        else
                        {
                            (xn as XmlElement).SetAttribute(attr, value);
                        }
                    }
                    else
                    {
                        var t = xd.CreateTextNode(value);
                        var e = xd.CreateElement(name);
                        e.AppendChild(t);
                        xn.AppendChild(e);
                    }

                    ;
                }

                ;
            }

            ;

            var res = xd.OuterXml;

            var xs = new XmlSerializer(typeof(T));
            var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(res)));
            var c = (T)xs.Deserialize(reader);
            reader.Close();
            obj = c;
        }

        public class CustomComparer : IComparer<KeyValuePair<string, List<string>>>
        {
            public int Compare(KeyValuePair<string, List<string>> x, KeyValuePair<string, List<string>> y)
            {
                var a = x.Key;
                var b = y.Key;
                var s = FindSame(a, b);

                if (!string.IsNullOrEmpty(s))
                {
                    var av = a.Substring(s.Length);
                    if (av.Length > 0) av = av.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var bv = b.Substring(s.Length);
                    if (bv.Length > 0) bv = bv.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (int.TryParse(av, out var ai) && int.TryParse(bv, out var bi)) return ai.CompareTo(bi);
                    if (int.TryParse(av, out _)) return 1;
                    if (int.TryParse(bv, out _)) return -1;
                }

                ;
                return a.CompareTo(b);
            }

            private string FindSame(string a, string b)
            {
                if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return null;
                var r = "";
                for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
                    if (a[i] == b[i]) r += a[i];
                    else break;
                return r;
            }
        }

        #endregion LOAD

        #region STATIC

        public static string CurrentDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
            // return Application.StartupPath;
            // return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // return Directory.GetCurrentDirectory();
            // return Environment.CurrentDirectory;
            // return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            // return Path.GetDirectory(Application.ExecutablePath);
        }

        public static bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0].GetTypeInfo());
            return type.IsPrimitive
                   || type.IsEnum
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(decimal));
        }

        public static string NormalizeValue(string value, bool name = false)
        {
            if (name) value = value.Replace("@", "\\u0040");
            value = value.Replace(";", "\\u003B");
            value = value.Replace("#", "\\u0023");
            value = value.Replace("\r", "\\u000D");
            value = value.Replace("\n", "\\u000A");
            return value;
        }

        public static string ReversizeValue(string value, bool name = false)
        {
            if (name) value = value.Replace("\\u0040", "@");
            value = value.Replace("\\u003B", ";").Replace("\\u003b", ";");
            value = value.Replace("\\u0023", "#");
            value = value.Replace("\\u000D", "\r").Replace("\\u000d", "\r");
            value = value.Replace("\\u000A", "\n").Replace("\\u000a", "\n");
            return value;
        }

        #endregion STATIC
    }

    public class IniSectionAttribute : Attribute
    {
        public IniSectionAttribute(string name)
        {
            this.name = name;
        }

        public string name { set; get; }
    }

    /// <summary>
    ///     Class for Serialize Dictionary
    /// </summary>
    public class DictionaryEntry
    {
        public object Key;
        public object Value;

        public DictionaryEntry()
        {
        }

        public DictionaryEntry(object key, object value)
        {
            Key = key;
            Value = value;
        }

        public static DictionaryEntry[] Get(IDictionary dict)
        {
            var entries = new List<DictionaryEntry>(dict == null ? 0 : dict.Count);
            if (dict != null)
                foreach (var key in dict.Keys)
                    entries.Add(new DictionaryEntry(key, dict[key]));
            return entries.ToArray();
        }

        public static void Set(IDictionary dict, List<DictionaryEntry> values)
        {
            Set(dict, values?.ToArray());
        }

        public static void Set(IDictionary dict, DictionaryEntry[] values)
        {
            dict.Clear();
            if (values == null || values.Length == 0) return;
            foreach (var entry in values) dict.Add(entry.Key, entry.Value);
        }
    }
}