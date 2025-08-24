using Hacknet;
using Pathfinder.Port;
using Pathfinder.Util;

namespace TempestGadgets.Utils
{
    public class Whitelist
    {
        public static void AddWhitelist(string WhitelistNodeId,OS os)
        {
            Computer computer = ComputerLookup.FindById(WhitelistNodeId);
            Computer playerComp = ComputerLookup.FindByIp("#PLAYER_IP#");
            string playerIP = playerComp.ip;
            Folder folderAtPath = Programs.getFolderAtPath("Whitelist", os, computer.files.root, true);
            if (folderAtPath != null)
            {
                FileEntry fileEntry = folderAtPath.searchForFile("list.txt");
                fileEntry.data = fileEntry.data + "\n" + $"{playerIP}";
            }

        }
    }
}