using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark_Dino_Manager
{
    class IniParser
    {
        public static Dictionary<string, Dictionary<string, string>> ParseIniFile(string filePath)
        {
            var sections = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = null;
            var currentSectionData = new Dictionary<string, string>();

            foreach (var line in File.ReadLines(filePath))
            {
                var trimmedLine = line.Trim();

                // Ignore empty lines or comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    continue;

                // Check for section headers
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // Save the previous section if any
                    if (currentSection != null)
                    {
                        sections[currentSection] = currentSectionData;
                    }

                    // Start a new section
                    currentSection = trimmedLine.Trim('[', ']');
                    currentSectionData = new Dictionary<string, string>();
                }
                else
                {
                    // Parse key-value pairs inside sections
                    var keyValue = trimmedLine.Split(new[] { '=' }, 2);
                    if (keyValue.Length == 2)
                    {
                        currentSectionData[keyValue[0].Trim()] = keyValue[1].Trim();
                    }
                }
            }

            // Add the last section
            if (currentSection != null)
            {
                sections[currentSection] = currentSectionData;
            }

            return sections;
        }

    }

}
