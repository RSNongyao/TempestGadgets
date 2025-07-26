using Hacknet;
using Pathfinder.Port;

namespace TempestGadgets.Utils
{
    public class PortDetect
    {
        public static bool IsHasPort(Computer computer, int port)
        {
            Dictionary<string, PortState> PortDict = computer.GetPortStateDict();

            foreach (var kvp in PortDict)
            {
                if (kvp.Value.PortNumber == port)
                {
                    return true;
                }
            }
            return false;
        }
    }
}