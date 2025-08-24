using Hacknet;
using Hacknet.Extensions;
using Microsoft.Xna.Framework;
using TempestGadgets.Patches;
using TempestGadgets.Utils;
using TempestGadgets;
using Pathfinder.Util;

namespace TempestGadgets.Executables
{
    public class PPInjectorExe : Pathfinder.Executable.BaseExecutable
    {
        string filename;
        Folder currentFolder;

        public PPInjectorExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
        {
            ramCost = 0;
            IdentifierName = "PPInjector";
            needsProxyAccess = false;
            name = "PPInjector";

        }

        public override void LoadContent()
        {
            currentFolder = Programs.getCurrentFolder(os);
            int entriesCount = 0;

            if (Args.Length < 2)
            {
                this.needsRemoval = true;
                os.terminal.writeLine("No Arguments Found!");
                this.needsRemoval = true;
                return;
            }
            filename = Args[1];

            if (currentFolder.searchForFile(filename) != null)
            {
                FileEntry passportFile = currentFolder.searchForFile(filename);

                if (!passportFile.data.StartsWith("MIMIKATZ_KERBEROS_PASSPORT :: 2.2.0 ------------"))
                {
                    os.terminal.writeLine("Invalid File");
                    this.needsRemoval = true;
                    return;
                }

                PassPortContent content = PassPortContent.GetContentsFromEncodedFileString(passportFile.data);

                if (content == null)
                {
                    os.terminal.writeLine("Invalid File");
                    this.needsRemoval = true;
                    return;
                }

                var comparer = new PassPortEntryComparer();
                PassPortContent existContent = TempestGadgets.UsedPassPort
                    .FirstOrDefault(usedContent => usedContent.entries.SequenceEqual(content.entries, comparer));

                bool hasUniqueEntry = content.entries.Any(entry => !IsEntryDuplicate(entry, TempestGadgets.UsedPassPort));

                if (existContent != null)
                {
                    os.terminal.writeLine("Synchronizing passport data...");
                    os.terminal.writeLine("Get newer passport to update details!");
                }
                else
                {
                    existContent = content;
                    if (hasUniqueEntry)
                        TempestGadgets.UsedPassPort.Add(content);
                }

                Console.WriteLine($"{TempestGadgets.UsedPassPort.Count} in list");
                foreach (var entry in existContent.entries.Where(e => e.isActive))
                {
                    Computer c = Programs.getComputer(os, entry.TargetComp);
                    entriesCount += 1;
                    foreach (var port in entry.Ports ?? Array.Empty<int>())
                    {
                        Computer computer = ComputerLookup.FindByIp("#PLAYER_IP#");
                        c.openPort(port, computer.ip);
                    }
                    if (entry.OverloadProxy)
                    {
                        c.proxyActive = false;
                    }
                    if (entry.CrackFirewall)
                    {
                        c.firewall = null;
                    }
                    if (entry.AddWhitelist != "NONE")
                    {
                        Whitelist.AddWhitelist(entry.AddWhitelist, os);
                    }
                    if (entry.LoadAction != "NONE")
                    {
                        string completePath = ExtensionLoader.ActiveExtensionInfo.GetFullFolderPath() + "/" + entry.LoadAction;
                        RunnableConditionalActions.LoadIntoOS(completePath, os);
                    }
                }
                os.terminal.writeLine("=========================");
                os.terminal.writeLine($"    {entriesCount} Inject Finished");
                os.terminal.writeLine("=========================");

                this.needsRemoval = true;
                return;
            }
            else
            {
                os.terminal.writeLine("Invalid File");
                this.needsRemoval = true;
                return;
            }

        }


        public static bool IsEntryDuplicate(PassPortEntry entry, List<PassPortContent> usedPassPorts)
        {
            foreach (var usedContent in usedPassPorts)
            {
                foreach (var usedEntry in usedContent.entries)
                {
                    if (
                        entry.id == usedEntry.id &&
                        entry.TargetComp == usedEntry.TargetComp &&
                        entry.OpenPorts == usedEntry.OpenPorts &&
                        entry.OverloadProxy == usedEntry.OverloadProxy &&
                        entry.CrackFirewall == usedEntry.CrackFirewall &&
                        entry.AddWhitelist == usedEntry.AddWhitelist &&
                        entry.LoadAction == usedEntry.LoadAction
                    )
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public override void Draw(float t)
        {
            base.Draw(t);
        }


        public override void Update(float t)
        {
            base.Update(t);
        }


    }
}

public class PassPortEntryComparer : IEqualityComparer<PassPortEntry>
{
    public bool Equals(PassPortEntry x, PassPortEntry y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.id == y.id &&
               x.TargetComp == y.TargetComp &&
               x.OpenPorts == y.OpenPorts &&
               x.OverloadProxy == y.OverloadProxy &&
               x.CrackFirewall == y.CrackFirewall &&
               x.AddWhitelist == y.AddWhitelist &&
               x.LoadAction == y.LoadAction &&
               ((x.Ports == null && y.Ports == null) ||
                (x.Ports != null && y.Ports != null && x.Ports.SequenceEqual(y.Ports)));
    }

    public int GetHashCode(PassPortEntry obj)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (obj.id?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.TargetComp?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.OpenPorts?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.OverloadProxy.GetHashCode();
            hash = hash * 23 + obj.CrackFirewall.GetHashCode();
            hash = hash * 23 + (obj.AddWhitelist?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.LoadAction?.GetHashCode() ?? 0);
            if (obj.Ports != null)
                foreach (var p in obj.Ports)
                    hash = hash * 23 + p.GetHashCode();
            return hash;
        }
    }
}
