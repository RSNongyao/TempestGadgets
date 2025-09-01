using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Daemon;
using Pathfinder.Executable;
using TempestGadgets.Daemons;
using Pathfinder.Action;
using Pathfinder.Event.Loading;
using Pathfinder.Event;
using Hacknet;
using Pathfinder.Util.XML;
using TempestGadgets.Executables;
using TempestGadgets.Patches;
using TempestGadgets.Actions;
using Pathfinder.Event.Saving;
using System.Xml.Linq;
using Pathfinder.Meta.Load;
using Pathfinder.Replacements;

namespace TempestGadgets;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class TempestGadgets : HacknetPlugin
{
    public const string ModGUID = "com.wncry.TempestGadgets";
    public const string ModName = "TempestGadgets";
    public const string ModVer = "0.2.0";

    public override bool Load()
    {
        string logo = @"

+===============================================================================+
| _________  _______   _____ ______   ________  _______   ________  _________   |
||\___   ___\\  ___ \ |\   _ \  _   \|\   __  \|\  ___ \ |\   ____\|\___   ___\ |
|\|___ \  \_\ \   __/|\ \  \\\__\ \  \ \  \|\  \ \   __/|\ \  \___|\|___ \  \_| |
|     \ \  \ \ \  \_|/_\ \  \\|__| \  \ \   ____\ \  \_|/_\ \_____  \   \ \  \  |
|      \ \  \ \ \  \_|\ \ \  \    \ \  \ \  \___|\ \  \_|\ \|____|\  \   \ \  \ |
|       \ \__\ \ \_______\ \__\    \ \__\ \__\    \ \_______\____\_\  \   \ \__\|
|        \|__|  \|_______|\|__|     \|__|\|__|     \|_______|\_________\   \|__||
|                                                           \|_________|        |
| ________  ________  ________  ________  _______  _________  ________          |
||\   ____\|\   __  \|\   ___ \|\   ____\|\  ___ \|\___   ___\\   ____\         |
|\ \  \___|\ \  \|\  \ \  \_|\ \ \  \___|\ \   __/\|___ \  \_\ \  \___|_        |
| \ \  \  __\ \   __  \ \  \ \\ \ \  \  __\ \  \_|/__  \ \  \ \ \_____  \       |
|  \ \  \|\  \ \  \ \  \ \  \_\\ \ \  \|\  \ \  \_|\ \  \ \  \ \|____|\  \      |
|   \ \_______\ \__\ \__\ \_______\ \_______\ \_______\  \ \__\  ____\_\  \     |
|    \|_______|\|__|\|__|\|_______|\|_______|\|_______|   \|__| |\_________\    |
|                                                               \|_________|    |
+===============================================================================+
";

        Console.WriteLine(logo);
        LogDebug("Loading Daemons...");
        DaemonManager.RegisterDaemon<NuclearDaemon>();

        LogDebug("Loading Executables...");
        ExecutableManager.RegisterExecutable<NetSpoofExe>("#NET_SPOOF#");
        ExecutableManager.RegisterExecutable<VPNBypassExe>("#VPN_BYPASS#");
        ExecutableManager.RegisterExecutable<EOSRootKitExe>("#EOS_ROOTKIT#");
        ExecutableManager.RegisterExecutable<SignalFilterExe>("#SIGNAL_FILTER#");
        ExecutableManager.RegisterExecutable<MimikatzExe>("#PASSPORT_READER#");
        ExecutableManager.RegisterExecutable<PPInjectorExe>("#PASSPORT_INJECT#");
        ExecutableManager.RegisterExecutable<EnBreakerExe>("#ENSEC_BREAKER#");

        LogDebug("Loading Actions...");
        ActionManager.RegisterAction<ScreenAction.EnableScreenGlitch>("EnableScreenGlitch");
        ActionManager.RegisterAction<ScreenAction.DisableScreenGlitch>("DisableScreenGlitch");
        ActionManager.RegisterAction<PassPortAction.SwitchPPEntry>("SwitchPPEntry");

        LogDebug("Loading Events...");
        Action<SaveComputerEvent> PassPortSaveDelegate = SavePassPortIntoComps;
        Action<SaveComputerLoadedEvent> PassPortLoadDelegate = LoadPassPortComps;
        Action<SaveEvent> SaveDelegate = InjectSaveData;


        EventManager<SaveComputerEvent>.AddHandler(PassPortSaveDelegate);
        EventManager<SaveComputerLoadedEvent>.AddHandler(PassPortLoadDelegate);
        EventManager<SaveEvent>.AddHandler(SaveDelegate);

        return true;
    }

    public static Dictionary<string, PassPortContent> PassPortComps = new();
    public static List<PassPortContent> UsedPassPort = new();

    public void SavePassPortIntoComps(SaveComputerEvent saveComp)
    {
        Computer c = saveComp.Comp;

        if (PassPortComps.ContainsKey(c.idName))
        {
            PassPortContent contents = PassPortComps[c.idName];
            XElement compElement = saveComp.Element;

            LogDebug($"Saving PassPort data on node {c.idName}...");

            XElement PassPortElement = new XElement("PassPort");

            foreach (var entry in contents.entries)
            {
                XElement PassPortEntryElement = new XElement("PP");

                XAttribute wID = new XAttribute("id", entry.id);
                XAttribute wTarget = new XAttribute("TargetComp", entry.TargetComp);
                XAttribute wOpenPorts = new XAttribute("OpenPorts", entry.OpenPorts);
                XAttribute wOverloadProxy = new XAttribute("OverloadProxy", entry.OverloadProxy.ToString());
                XAttribute wCrackFirewall = new XAttribute("CrackFirewall", entry.CrackFirewall.ToString());
                XAttribute wAddWhitelist = new XAttribute("AddWhitelist", entry.AddWhitelist);
                XAttribute wLoadAction = new XAttribute("LoadAction", entry.LoadAction);
                XAttribute wIsActive = new XAttribute("isActive", entry.isActive.ToString());

                PassPortEntryElement.Add(wID, wTarget, wOpenPorts, wOverloadProxy, wCrackFirewall, wAddWhitelist, wLoadAction,wIsActive);
                PassPortElement.Add(PassPortEntryElement);
            }

            compElement.FirstNode.AddAfterSelf(PassPortElement);
        }
    }

    public void LoadPassPortComps(SaveComputerLoadedEvent saveComp)
    {
        Computer comp = saveComp.Comp;
        ElementInfo xCompElem = saveComp.Info;

        if (xCompElem.Children.FirstOrDefault(e => e.Name == "PassPort") != null)
        {
            ElementInfo PassPortElement = xCompElem.Children.First(e => e.Name == "PassPort");
            PassPortContent contents = new PassPortContent();

            for (var i = 0; i < PassPortElement.Children.Count; i++)
            {
                ElementInfo e = PassPortElement.Children[i];

                bool OverloadProxy = bool.Parse(e.Attributes["OverloadProxy"].ToLower());
                bool CrackFirewall = bool.Parse(e.Attributes["CrackFirewall"].ToLower());
                bool isActive = bool.Parse(e.Attributes["isActive"].ToLower());

                PassPortEntry entry = new PassPortEntry(e.Attributes["id"], e.Attributes["TargetComp"], e.Attributes["OpenPorts"],
                    OverloadProxy, CrackFirewall, e.Attributes["AddWhitelist"], e.Attributes["LoadAction"], isActive);

                contents.entries.Add(entry);
            }

            contents.originID = comp.idName;

            PassPortComps.Add(comp.idName, contents);
        }
    }


    public void InjectSaveData(SaveEvent save_event)
    {
        Console.WriteLine($"Saving {UsedPassPort.Count} used passports...");
        XElement usedPassPort = new XElement("UsedPassPort");

        var uniqueEntries = new HashSet<string>();
        foreach (var content in UsedPassPort)
        {
            XElement PassPortElement = new XElement("PassPort");
            foreach (var entry in content.entries)
            {

                string uniqueKey = $"{entry.id}|{entry.TargetComp}|{entry.OpenPorts}|{entry.OverloadProxy}|{entry.CrackFirewall}|{entry.AddWhitelist}|{entry.LoadAction}";
                if (uniqueEntries.Contains(uniqueKey))
                    continue;
                uniqueEntries.Add(uniqueKey);

                XElement PassPortEntryElement = new XElement("PP");
                XAttribute wID = new XAttribute("id", entry.id);
                XAttribute wTarget = new XAttribute("TargetComp", entry.TargetComp);
                XAttribute wOpenPorts = new XAttribute("OpenPorts", entry.OpenPorts);
                XAttribute wOverloadProxy = new XAttribute("OverloadProxy", entry.OverloadProxy.ToString());
                XAttribute wCrackFirewall = new XAttribute("CrackFirewall", entry.CrackFirewall.ToString());
                XAttribute wAddWhitelist = new XAttribute("AddWhitelist", entry.AddWhitelist);
                XAttribute wLoadAction = new XAttribute("LoadAction", entry.LoadAction);
                XAttribute wIsActive = new XAttribute("isActive", entry.isActive.ToString());

                PassPortEntryElement.Add(wID, wTarget, wOpenPorts, wOverloadProxy, wCrackFirewall, wAddWhitelist, wLoadAction, wIsActive);
                PassPortElement.Add(PassPortEntryElement);
            }
            if (PassPortElement.HasElements)
                usedPassPort.Add(PassPortElement);
        }

        save_event.Save.FirstNode.AddBeforeSelf(usedPassPort);
    }


    [SaveExecutor("HacknetSave.UsedPassPort",ParseOption.ParseInterior)]
    public class ReadUsedPassPort : SaveLoader.SaveExecutor
    {

        public override void Execute(EventExecutor exec, ElementInfo info)
        {

            UsedPassPort.Clear();
            foreach (var passPortElem in info.Children)
            {

                PassPortContent content = new PassPortContent();
                foreach (var ppElem in passPortElem.Children)
                {
                    if (!string.Equals(ppElem.Name, "PP", StringComparison.OrdinalIgnoreCase)) continue;

                    string id = ppElem.Attributes.ContainsKey("id") ? ppElem.Attributes["id"] : "";
                    string targetComp = ppElem.Attributes.ContainsKey("TargetComp") ? ppElem.Attributes["TargetComp"] : "";
                    string openPorts = ppElem.Attributes.ContainsKey("OpenPorts") ? ppElem.Attributes["OpenPorts"] : "";
                    bool overloadProxy = ppElem.Attributes.ContainsKey("OverloadProxy") && bool.TryParse(ppElem.Attributes["OverloadProxy"], out var op) && op;
                    bool crackFirewall = ppElem.Attributes.ContainsKey("CrackFirewall") && bool.TryParse(ppElem.Attributes["CrackFirewall"], out var cf) && cf;
                    string addWhitelist = ppElem.Attributes.ContainsKey("AddWhitelist") ? ppElem.Attributes["AddWhitelist"] : "";
                    string loadAction = ppElem.Attributes.ContainsKey("LoadAction") ? ppElem.Attributes["LoadAction"] : "";
                    bool isActive = true;
                    if (ppElem.Attributes.ContainsKey("isActive"))
                        bool.TryParse(ppElem.Attributes["isActive"], out isActive);

                    var entry = new PassPortEntry(id, targetComp, openPorts, overloadProxy, crackFirewall, addWhitelist, loadAction, isActive);
                    content.entries.Add(entry);
                }
                if (content.entries.Count > 0)
                {
                    UsedPassPort.Add(content);
                }
            }

        }
    }


    public void LogDebug(string message)
    {
        Log.LogDebug(message);
    }

}
