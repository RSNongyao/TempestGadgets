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

                    if (ppFolder.searchForFile(name) != null)
                    {
                        FileEntry passportFile = ppFolder.searchForFile(name);

                        if (!passportFile.data.StartsWith("MIMIKATZ_KERBEROS_PASSPORT :: 2.2.0 ------------"))
                        {
                            throw new FormatException("Invalid PassPort File");
                        }

                        PassPortContent content = PassPortContent.GetContentsFromEncodedFileString(passportFile.data) ?? throw new FormatException("Invalid PassPort File");
                        foreach (var entry in content.entries)
                        {
                            if (entry.id == id)
                            {
                                entry.isActive = isActive;


                                passportFile.data = content.GetEncodedFileString();

                                bool match = PPInjectorExe.IsEntryDuplicate(entry, TempestGadgets.UsedPassPort);
                                if (!match)
                                {
                                    TempestGadgets.UsedPassPort.Add(content);
                                }
                                else
                                {
                                    foreach (var usedContent in TempestGadgets.UsedPassPort)
                                    {
                                        if (usedContent.entries.Any(e => e.id == id))
                                        {
                                            usedContent.entries.RemoveAll(e => e.id == id);
                                            usedContent.entries.Add(entry);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException("PassPort file not found in specified folder.");
                    }
                }
                else
                {
                    PassPortContent contents = TempestGadgets.PassPortComps[c.idName];
                    foreach (var entry in contents.entries)
                    {
                        bool match = PPInjectorExe.IsEntryDuplicate(entry, TempestGadgets.UsedPassPort);
                        if (entry.id == id)
                        {
                            entry.isActive = isActive;
                            if (!match)
                            {
                                var singleEntryContent = new PassPortContent
                                {
                                    entries = new List<PassPortEntry> { entry }
                                };
                                TempestGadgets.UsedPassPort.Add(singleEntryContent);
                            }
                            else
                            {
                                foreach (var usedContent in TempestGadgets.UsedPassPort)
                                {
                                    if (usedContent.entries.Any(e => e.id == id))
                                    {
                                        usedContent.entries.RemoveAll(e => e.id == id);
                                        usedContent.entries.Add(entry);
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}