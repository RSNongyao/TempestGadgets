using System;
using System.Text;
using System.Xml;
using Hacknet;
using Pathfinder.Util.XML;
using TempestGadgets.Executables;


namespace TempestGadgets.Patches
{
    public class PassPortContent
    {
        public List<PassPortEntry> entries = new List<PassPortEntry>();
        public string originID = "playerComp";
        public bool IsValid { get; private set; } = true;
        private const string key = "11451-41919-810";

        public static PassPortContent Deserialize(XmlReader xml, string originID = "playerComp")
        {
            PassPortContent contents = new();
            while (xml.Name != "PassPort")
            {
                xml.Read();
                if (xml.EOF)
                {
                    throw new FormatException("Unexpected end of file looking for PassPort tag.");
                }
            }

            do
            {

                xml.Read();
                if (xml.Name == "PassPort" && !xml.IsStartElement())
                {
                    return contents;
                }

                if (xml.Name == "PP" && xml.IsStartElement())
                {
                    contents.IsValid = true;

                    string id = "NONE";
                    string TargetComp = "#PLAYER_IP#";
                    string OpenPorts = "NONE";
                    bool OverloadProxy = false;
                    bool CrackFirewall = false;
                    string AddWhitelist = "NONE";
                    string LoadAction = "NONE";
                    bool isActive = true;

                    if (xml.MoveToAttribute("id"))
                    {
                        id = xml.ReadContentAsString();
                    }

                    if (xml.MoveToAttribute("TargetComp"))
                    {
                        TargetComp = xml.ReadContentAsString();
                        Console.WriteLine($"TargetComp: {xml.ReadContentAsString()}");

                    }

                    if (xml.MoveToAttribute("OpenPorts"))
                    {
                        OpenPorts = xml.ReadContentAsString();
                        Console.WriteLine($"OpenPorts: {xml.ReadContentAsString()}");

                    }

                    if (xml.MoveToAttribute("OverloadProxy"))
                    {
                        OverloadProxy = bool.Parse(xml.ReadContentAsString().ToLower());
                        Console.WriteLine($"OverloadProxy: {OverloadProxy}");

                    }

                    if (xml.MoveToAttribute("CrackFirewall"))
                    {
                        CrackFirewall = bool.Parse(xml.ReadContentAsString().ToLower());
                        Console.WriteLine($"CrackFirewall: {CrackFirewall}");

                    }

                    if (xml.MoveToAttribute("AddWhitelist"))
                    {
                        AddWhitelist = xml.ReadContentAsString();
                        Console.WriteLine($"AddWhitelist: {xml.ReadContentAsString()}");

                    }

                    if (xml.MoveToAttribute("LoadAction"))
                    {
                        LoadAction = xml.ReadContentAsString();
                        Console.WriteLine($"LoadAction: {xml.ReadContentAsString()}");

                    }

                    if (xml.MoveToAttribute("isActive"))
                    {
                        isActive = bool.Parse(xml.ReadContentAsString().ToLower());
                        Console.WriteLine($"isActive: {isActive}");
                    }
                    xml.MoveToContent();

                    PassPortEntry entry = new PassPortEntry(id, TargetComp, OpenPorts, OverloadProxy, CrackFirewall, AddWhitelist, LoadAction, isActive);

                    contents.entries.Add(entry);
                }
            } while (!xml.EOF);
            throw new FormatException("Unexpected end of file trying to deserialize passport contents!");
        }

        public static PassPortContent ReadPassPortXML(ElementInfo info, string originID = "playerComp")
        {
            PassPortContent contents = new() { originID = originID };

            if (info.Children.Count == 0)
            {
                throw new FormatException("PassPort element doesn't have any children!");
            }

            foreach (var child in info.Children)
            {
                if (child.Name != "PP")
                {
                    throw new FormatException("Unrecognized child element in PassPort element");
                }

                string id = "NONE";
                string TargetComp = "#PLAYER_IP#";
                string OpenPorts = "NONE";
                bool OverloadProxy = false;
                bool CrackFirewall = false;
                string AddWhitelist = "NONE";
                string LoadAction = "NONE";
                bool isActive = true;

                throwIfNotFound("id", child);
                throwIfNotFound("TargetComp", child);
                throwIfNotFound("isActive", child);


                if (!bool.TryParse(child.Attributes["isActive"], out isActive))
                {
                    throw new FormatException("PP element 'isActive' needs to be a boolean (true/false)");
                }



                if (child.Attributes.ContainsKey("id"))
                {
                    id = child.Attributes["id"];
                }
                if (child.Attributes.ContainsKey("TargetComp"))
                {
                    TargetComp = child.Attributes["TargetComp"];
                }
                if (child.Attributes.ContainsKey("OpenPorts"))
                {
                    OpenPorts = child.Attributes["OpenPorts"];
                }
                if (child.Attributes.ContainsKey("OverloadProxy"))
                {
                    OverloadProxy = bool.Parse(child.Attributes["OverloadProxy"]);
                }
                if (child.Attributes.ContainsKey("CrackFirewall"))
                {
                    CrackFirewall = bool.Parse(child.Attributes["CrackFirewall"]);
                }
                if (child.Attributes.ContainsKey("AddWhitelist"))
                {
                    AddWhitelist = child.Attributes["AddWhitelist"];
                }
                if (child.Attributes.ContainsKey("LoadAction"))
                {
                    LoadAction = child.Attributes["LoadAction"];
                }
                if (child.Attributes.ContainsKey("isActive"))
                {
                    isActive = bool.Parse(child.Attributes["isActive"]);
                }
                else isActive = true;
                    PassPortEntry entry = new(id, TargetComp, OpenPorts, OverloadProxy, CrackFirewall, AddWhitelist, LoadAction, isActive);
                contents.entries.Add(entry);
            }

            return contents;

            void throwIfNotFound(string attribute, ElementInfo elem)
            {
                if (!elem.Attributes.ContainsKey(attribute))
                {
                    throw new FormatException(string.Format("PP element " +
                        "is missing required attribute '{0}'", attribute));
                }
            }
        }

        public string GetSaveString()
        {
            StringBuilder saveString = new();

            saveString.Append($"<PassPort origin=\"{originID}\">\r\n");
            foreach (var entry in entries)
            {
                saveString.Append($"<PP id=\"{entry.id}\" TargetComp=\"{entry.TargetComp}\" " 
                    +$"OpenPorts=\"{entry.OpenPorts}\" OverloadProxy=\"{entry.OverloadProxy}\" "
                    +$"CrackFirewall=\"{entry.CrackFirewall}\" AddWhitelist=\"{entry.AddWhitelist}\" " 
                    +$"LoadAction=\"{entry.LoadAction}\" isActive=\"{entry.isActive}\"></PP>");
            }
            saveString.Append("</PassPort>");

            return saveString.ToString();
        }

        public string GetSaveString(string entryID)
        {
            StringBuilder saveString = new();
            saveString.Append($"<PassPort origin=\"{originID}\">\r\n");

            foreach (var entry in entries)
            {
                if (entry.id == entryID)
                {
                    saveString.Append($"<PP id=\"{entry.id}\" TargetComp=\"{entry.TargetComp}\" "
                    + $"OpenPorts=\"{entry.OpenPorts}\" OverloadProxy=\"{entry.OverloadProxy}\" "
                    + $"CrackFirewall=\"{entry.CrackFirewall}\" AddWhitelist=\"{entry.AddWhitelist}\" "
                    + $"LoadAction=\"{entry.LoadAction}\" isActive=\"{entry.isActive}\"></PP>");
                }
            }
            saveString.Append("</PassPort>");

            return saveString.ToString();

        }


        public string GetEncodedFileString()
        {
            string saveString = GetSaveString();
            string encoded = FileEncrypter.EncryptString(saveString, "MIMIKATZ PASSPORT", "======", key);
            return "MIMIKATZ_KERBEROS_PASSPORT :: 2.2.0 ------------\n\n" + encoded;
        }
        public string GetEncodedFileString(string entryID)
        {
            string saveString = GetSaveString(entryID);
            string encoded = FileEncrypter.EncryptString(saveString, "MIMIKATZ PASSPORT", "======", key);
            return "MIMIKATZ_KERBEROS_PASSPORT :: 2.2.0 ------------\n\n" + encoded;
        }


        public static PassPortContent GetContentsFromEncodedFileString(string data)
        {
            string mainEncodedContent = data.Substring("MIMIKATZ_KERBEROS_PASSPORT :: 2.2.0 ------------\n\n".Length);
            string decodedContent = FileEncrypter.DecryptString(mainEncodedContent, key)[2];

            using Stream input = Hacknet.Utils.GenerateStreamFromString(decodedContent);
            XmlReader reader = XmlReader.Create(input);
            return Deserialize(reader);



        }

    }

}