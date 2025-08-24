using Pathfinder.Meta.Load;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;
using Hacknet;
using Microsoft.Xna.Framework.Content;
using TempestGadgets.Executables;
using Hacknet.Extensions;

namespace TempestGadgets.Patches
{
    [ComputerExecutor("Computer.PassPort", ParseOption.ParseInterior)]
    public class PassPortLoader : ContentLoader.ComputerExecutor
    {
        public override void Execute(EventExecutor exec, ElementInfo info)
        {
            PassPortContent contents = PassPortContent.ReadPassPortXML(info, Comp.idName);

            if (TempestGadgets.PassPortComps.ContainsKey(Comp.idName))
            {
                Console.WriteLine(string.Format("Computer with ID of '{0}' already exists in " +
                    "PassPortComp! Overwriting...", Comp.idName));
                TempestGadgets.PassPortComps[Comp.idName] = contents;
            }
            else
            {
                TempestGadgets.PassPortComps.Add(Comp.idName, contents);
            }

        }
    }

    [ComputerExecutor("Computer.PassPortFile", ParseOption.ParseInterior)]
    public class PassPortFileLoader : ContentLoader.ComputerExecutor
    {
        public override void Execute(EventExecutor exec, ElementInfo info)
        {
            if (!info.Attributes.ContainsKey("path") || !info.Attributes.ContainsKey("name"))
            {
                throw new FormatException("Missing required attribute on PassPortFile element");
            }

            string folderPath = info.Attributes["path"];
            string filename = info.Attributes["name"];

            PassPortContent contents = PassPortContent.ReadPassPortXML(info, Comp.idName);
            string filedata = contents.GetEncodedFileString();
            Folder targetFolder = Comp.getFolderFromPath(folderPath, true);

            if (targetFolder.searchForFile(filename) != null)
            {
                targetFolder.searchForFile(filename).data = filedata;
            }
            else
            {
                targetFolder.files.Add(new FileEntry(filedata, filename));
            }
        }
    }




}