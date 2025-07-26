using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Daemon;
using Pathfinder.Executable;
using TempestGadgets.Daemons;
using Pathfinder.Action;

namespace TempestGadgets;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class TempestGadgets : HacknetPlugin
{
    public const string ModGUID = "com.wncry.TempestGadgets";
    public const string ModName = "TempestGadgets";
    public const string ModVer = "0.1.0";

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

        LogDebug("Loading Actions...");
        ActionManager.RegisterAction<EnableScreenGlitch>("EnableScreenGlitch");
        ActionManager.RegisterAction<DisableScreenGlitch>("DisableScreenGlitch");




        return true;
    }

    private void LogDebug(string message)
    {
        Log.LogDebug(message);
    }
}
