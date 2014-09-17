using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

public class XmlConfiguration : IDisposable
{
    private SortedDictionary<string, object> Settings = new SortedDictionary<string, object>();
    private string InternalPath;
    private string Name;

    public XmlConfiguration(string name, string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        Name = name;
        InternalPath = path + "/" + name + ".xml";
    }

    public void AddSetting(string id, object defaultValue)
    {
        if (!Settings.ContainsKey(id))
        {
            Settings.Add(id, defaultValue);
        }
    }

    public T GetSetting<T>(string id)
    {
        return (T)Settings[id];
    }

    public void SetValue(string id, object value)
    {
        Settings[id] = value;
    }

    public void FinishedAddingSettings()
    {
        Load();
        Save();
    }

    private void Load()
    {
        if (File.Exists(InternalPath))
        {
            using (XmlReader reader = XmlReader.Create(InternalPath))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("item"))
                            {
                                string id = reader.GetAttribute("id");
                                if (Settings.ContainsKey(id))
                                {
                                    reader.Read();
                                    if (Settings[id] is bool)
                                    {
                                        Settings[id] = reader.Value.Equals("true");
                                    }
                                    else if (Settings[id] is Int32)
                                    {
                                        Int32 value = (Int32)Settings[id];
                                        Int32.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is UInt32)
                                    {
                                        UInt32 value = (UInt32)Settings[id];
                                        UInt32.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is Int16)
                                    {
                                        Int16 value = (Int16)Settings[id];
                                        Int16.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is UInt16)
                                    {
                                        UInt16 value = (UInt16)Settings[id];
                                        UInt16.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is Int64)
                                    {
                                        Int64 value = (Int64)Settings[id];
                                        Int64.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is UInt64)
                                    {
                                        UInt64 value = (UInt64)Settings[id];
                                        UInt64.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is Byte)
                                    {
                                        Byte value = (Byte)Settings[id];
                                        Byte.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is Single)
                                    {
                                        Single value = (Single)Settings[id];
                                        Single.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is Double)
                                    {
                                        Double value = (Double)Settings[id];
                                        Double.TryParse(reader.Value, out value);
                                        Settings[id] = value;
                                    }
                                    else if (Settings[id] is string)
                                    {
                                        Settings[id] = reader.Value;
                                    }
                                    else
                                    {
                                        throw new Exception("Setting " + id + " is of invalid type: " + Settings[id].GetType().Name);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
    }

    public void Save()
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.IndentChars = "  ";
        settings.NewLineChars = "\r\n";
        settings.NewLineHandling = NewLineHandling.Replace;
        using (XmlWriter writer = XmlWriter.Create(InternalPath, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("config");
            writer.WriteAttributeString("name", Name);
            foreach (KeyValuePair<string, object> keyPair in Settings)
            {
                writer.WriteStartElement("item");
                writer.WriteAttributeString("id", keyPair.Key);
                writer.WriteAttributeString("type", keyPair.Value.GetType().Name);
                writer.WriteValue(keyPair.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }
    }

    public void Dispose()
    {
        Save();
    }
}