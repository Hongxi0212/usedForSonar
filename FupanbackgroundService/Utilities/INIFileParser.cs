using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FupanBackgroundService.Utilities
{
    public class INIFileParser
    {
        private string fileName;
        private Dictionary<string, Dictionary<string, string>> sections;

        public INIFileParser(string fileName)
        {
            this.fileName = fileName;

            sections = new Dictionary<string, Dictionary<string, string>>();

            LoadFileContent();
        }

        public void WritePrivateProfileString(string section, string key, string value)
        {
            if (!sections.ContainsKey(section))
            {
                sections[section] = new Dictionary<string, string>();
            }
            sections[section][key] = value;
        }

        public string GetPrivateProfileString(string section, string key, string defaultValue = "")
        {
            if (sections.ContainsKey(section) && sections[section].ContainsKey(key))
            {
                return sections[section][key];
            }
            return defaultValue;
        }

        public List<string> GetAllKeys(string section)
        {
            if (sections.ContainsKey(section))
            {
                return new List<string>(sections[section].Keys);
            }
            return new List<string>();
        }

        public List<string> GetAllSections()
        {
            return new List<string>(sections.Keys);
        }

        public bool Save()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach (var section in sections)
                    {
                        writer.WriteLine($"[{section.Key}]");
                        foreach (var keyValue in section.Value)
                        {
                            writer.WriteLine($"{keyValue.Key}={keyValue.Value}");
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void LoadFileContent()
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (StreamReader reader = new StreamReader(fileName))
            {
                string currentSection = "";
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.Substring(1, line.Length - 2);
                    }
                    else
                    {
                        int equalPos = line.IndexOf('=');
                        if (equalPos != -1)
                        {
                            string key = line.Substring(0, equalPos).Trim();
                            string value = line.Substring(equalPos + 1).Trim();
                            WritePrivateProfileString(currentSection, key, value);
                        }
                    }
                }
            }
        }
    }
}
