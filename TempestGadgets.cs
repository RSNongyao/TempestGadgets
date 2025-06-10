using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Daemon;
using Pathfinder.Executable;
using TempestGadgets.Daemons;

namespace TempestGadgets;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class TempestGadgets : HacknetPlugin
{
    public const string ModGUID = "com.wncry.TempestGadgets";
    public const string ModName = "TempestGadgets";
    public const string ModVer = "0.0.1";

    public override bool Load()
    {
        LogDebug("Loading Daemons...");
        DaemonManager.RegisterDaemon<NuclearDaemon>();


        LogDebug("Loading Executables...");
        ExecutableManager.RegisterExecutable<VPNBypass>("#VPN_BYPASS#");
        return true;
    }

    private void LogDebug(string message)
    {
        Log.LogDebug(message);
    }
}
