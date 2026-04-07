using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace L5xModuleReport
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Full paths as string variables
            string l5xFullPath = @"D:\C_SharpDev\b-72_Plant12_OldPLC\CanbriamP12_04_06_2026_L5XFormat.L5X";
            string csvFullPath = @"D:\C_SharpDev\b-72_Plant12_OldPLC\CanbriamP12_04_06_2026_ModulesReport.csv";

            if (!File.Exists(l5xFullPath))
            {
                Console.Error.WriteLine($"L5X file not found: {l5xFullPath}");
                Environment.Exit(1);
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(l5xFullPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to load L5X XML: " + ex.Message);
                Environment.Exit(2);
                return;
            }

            // Find all <Module> nodes (namespace-safe)
            var moduleElements = doc
                .Descendants()
                .Where(e => e.Name.LocalName == "Module")
                .ToList();

            if (moduleElements.Count == 0)
            {
                Console.WriteLine("No <Module> elements found.");
                File.WriteAllText(csvFullPath, "No Modules Found" + Environment.NewLine);
                return;
            }

            var rows = new List<Dictionary<string, string>>();

            foreach (var m in moduleElements)
            {
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // A) Module attributes
                foreach (var attr in m.Attributes())
                    row[attr.Name.LocalName] = attr.Value;

                // B) Add <Port> child attributes (flattened)
                AddPortAttributes(row, m);

                rows.Add(row);
            }

            // Headers: union of all columns across all rows
            var headers = rows
                .SelectMany(r => r.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(h => h, StringComparer.OrdinalIgnoreCase)
                .ToList();

            WriteCsv(csvFullPath, headers, rows);

            Console.WriteLine($"Done. Modules: {moduleElements.Count}");
            Console.WriteLine($"CSV written to: {csvFullPath}");
        }

        private static void AddPortAttributes(Dictionary<string, string> row, XElement module)
        {
            // Typical L5X structure:
            // <Ports>
            //   <Port Id="1" Type="..." Address="..." ... />
            //   <Port Id="2" Type="..." Address="..." ... />
            // </Ports>

            var portElements = module
                .Descendants()
                .Where(e => e.Name.LocalName == "Port")
                .ToList();

            foreach (var port in portElements)
            {
                // Prefer Id as the port key; fall back to index-like token if missing
                string portId = port.Attribute("Id")?.Value?.Trim();
                if (string.IsNullOrWhiteSpace(portId))
                {
                    // If no Id, try "Number" or "PortId" (rare), else "Unknown"
                    portId = port.Attribute("Number")?.Value?.Trim()
                             ?? port.Attribute("PortId")?.Value?.Trim()
                             ?? "Unknown";
                }

                foreach (var a in port.Attributes())
                {
                    // Column name example: Port[1].Address, Port[1].Type, Port[1].Id
                    string colName = $"Port[{portId}].{a.Name.LocalName}";
                    row[colName] = a.Value;
                }

                // If a <Port> has text content (rare), store it too
                var text = (port.Value ?? "").Trim();
                if (!string.IsNullOrEmpty(text) && !port.HasElements)
                {
                    row[$"Port[{portId}].Value"] = text;
                }
            }
        }

        private static void WriteCsv(string csvFullPath, List<string> headers, List<Dictionary<string, string>> rows)
        {
            using var writer = new StreamWriter(csvFullPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            writer.WriteLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (var row in rows)
            {
                var line = string.Join(",", headers.Select(h =>
                {
                    row.TryGetValue(h, out var v);
                    return EscapeCsv(v ?? "");
                }));
                writer.WriteLine(line);
            }
        }

        private static string EscapeCsv(string value)
        {
            if (value == null) return "";

            bool mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!mustQuote) return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
