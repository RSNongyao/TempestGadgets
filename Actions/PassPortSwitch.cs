using BepInEx;
using Hacknet;
using Pathfinder.Action;
using Pathfinder.Util;
using TempestGadgets.Patches;
using TempestGadgets.Executables;
using System.Text.RegularExpressions;

namespace TempestGadgets.Actions
{
    public class PassPortAction
    {
        public class SwitchPPEntry : DelayablePathfinderAction
        {
            [XMLStorage]
            public string target;

            [XMLStorage]
            public string path;

            [XMLStorage]
            public string name;

            [XMLStorage]
            public string id;

            [XMLStorage]
            public bool isActive;

            public override void Trigger(OS os)
            {
                Computer c = ComputerLookup.FindById(target);

                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(name))
                {
                    Folder ppFolder = c.getFolderFromPath(path);
                    FileEntry passportFile = ppFolder.searchForFile(name)
                        ?? throw new FileNotFoundException("PassPort file not found in specified folder.");

                    if (!passportFile.data.StartsWith("MIMIKATZ_KERBEROS_PASSPORT :: 2.2.0 ------------"))
                        throw new FormatException("Invalid PassPort File");

                    PassPortContent content = PassPortContent.GetContentsFromEncodedFileString(passportFile.data)
                        ?? throw new FormatException("Invalid PassPort File");

                    var entry = content.entries.FirstOrDefault(e => e.id == id);
                    if (entry != null)
                    {
                        entry.isActive = isActive;
                        passportFile.data = content.GetEncodedFileString();
                        UpdateUsedPassPort(entry, content);
                    }
                }
                else
                {
                    PassPortContent contents = TempestGadgets.PassPortComps[c.idName];
                    var entry = contents.entries.FirstOrDefault(e => e.id == id);
                    if (entry != null)
                    {
                        entry.isActive = isActive;
                        if (!PPInjectorExe.IsEntryDuplicate(entry, TempestGadgets.UsedPassPort))
                        {
                            TempestGadgets.UsedPassPort.Add(new PassPortContent { entries = new List<PassPortEntry> { entry } });
                        }
                        else
                        {
                            UpdateUsedPassPort(entry, null);
                        }
                    }
                }
            }
            private void UpdateUsedPassPort(PassPortEntry entry, PassPortContent content)
            {
                bool match = PPInjectorExe.IsEntryDuplicate(entry, TempestGadgets.UsedPassPort);
                if (!match && content != null)
                {
                    TempestGadgets.UsedPassPort.Add(content);
                }
                else
                {
                    foreach (var usedContent in TempestGadgets.UsedPassPort)
                    {
                        if (usedContent.entries.Any(e => e.id == entry.id))
                        {
                            usedContent.entries.RemoveAll(e => e.id == entry.id);
                            usedContent.entries.Add(entry);
                            break;
                        }
                    }
                }
            }
        }
    }
}