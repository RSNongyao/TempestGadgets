using BepInEx;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using System.Runtime.Remoting.Lifetime;
using TempestGadgets.Patches;
using TempestGadgets.Utils;


namespace TempestGadgets.Executables
{
    public class MimikatzExe : Pathfinder.Executable.BaseExecutable
    {
        private float lifetime;
        bool GenerateAll = false;
        bool isReading = false;
        bool isGranting = false;
        private MimikatzState currentState;
        private float loadingTime = 0f;
        private enum MimikatzState
        {
            Loading,
            Error,
            Grant,
            Read,
            Done
        }

        public MimikatzExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
        {
            ramCost = 120;
            IdentifierName = "Mimikatz";
            name = "Mimikatz";

        }
        public override void LoadContent()
        {
            Computer targetComp = ComputerLookup.FindByIp(targetIP);

            if (Args.Length < 2)
            {
                this.needsRemoval = true;
                os.terminal.writeLine("[MIMIKZ] No Arguments Found!");
                return;
            }
            else if (Args[1] == "-s")
            {
                
                if (!targetComp.PlayerHasAdminPermissions())
                {
                    this.needsRemoval = true;
                    os.terminal.writeLine("[MIMIKZ] Administrator access required to grant passport!");
                    return;
                }
                else
                {
                    currentState = MimikatzState.Loading;
                    isGranting = true;
                    return;
                }
            }
            else if (Args[1] == "-f")
            {
                currentState = MimikatzState.Loading;
                isReading = true;
                return;
            }
            else
            {
                this.needsRemoval = true;
                os.terminal.writeLine("[MIMIKZ] Invalid Arguments!");
                return;
            }
        }

        public override void Draw(float t)
        {
            drawOutline();
            drawTarget("app:");
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width, this.bounds.Height - Module.PANEL_HEIGHT), 1);
            Rectangle textArea = new Rectangle(drawArea.Right / 3, drawArea.Y + 8 * drawArea.Height / 12, 2 * drawArea.Width / 3, 4 * drawArea.Height / 12);
            string MovingText;
            string date = DateTime.Now.ToString("R");
            string logo = @"
 .#####.  
.## ^ ##.   
## / \ ##    
## \ / ##
'## v ##'
 '#####'
";

            switch (currentState)
            {
                case MimikatzState.Error:
                    MovingText = "# Error :(";
                    break;
                case MimikatzState.Grant:
                    MovingText = "* Granting...";
                    break;
                case MimikatzState.Read:
                    MovingText = "@ Reading...";
                    break;
                case MimikatzState.Loading:
                    char[] spinner = new char[] { '-', '\\', '|', '/' };
                    int spinnerIndex = (int)((lifetime / 0.1f) % spinner.Length);
                    MovingText = spinner[spinnerIndex].ToString() + " Loading...";
                    break;
                case MimikatzState.Done:
                    MovingText = "+ Done! Exiting...";
                    break;
                default:
                    MovingText = "default";
                    break;
            }

            spriteBatch.Draw(Hacknet.Utils.white, drawArea, Color.DarkOliveGreen);
            GuiData.spriteBatch.DrawString(GuiData.detailfont, $"{logo}", new Vector2(drawArea.Left + 3, drawArea.Y - 15), Color.White, 0f, Vector2.Zero, 1f, default, default);
            GuiData.spriteBatch.DrawString(GuiData.detailfont, "mimikatz 2.2.0", new Vector2(drawArea.Right / 3 - 4, drawArea.Y), Color.White, 0f, Vector2.Zero, 1f, default, default);
            GuiData.spriteBatch.DrawString(GuiData.detailfont, "20150813(x86/hacknetOS)",new Vector2(drawArea.Right / 3, drawArea.Y + drawArea.Height / 6),Color.White, 0f, Vector2.Zero, 1f,default,default);
            GuiData.spriteBatch.DrawString(GuiData.detailfont, "\"A La Vie, A L'Amour\" - (oe.eo)", new Vector2(drawArea.Right / 3 + 5, drawArea.Y + drawArea.Height / 3), Color.White, 0f, Vector2.Zero, 1f, default, default);
            GuiData.spriteBatch.DrawString(GuiData.detailfont, $"{date}", new Vector2(drawArea.Right / 3 + 5, drawArea.Y + drawArea.Height / 2), Color.White, 0f, Vector2.Zero, 0.92f, default, default);
            var movingTextSize = GuiData.detailfont.MeasureString(MovingText);
            float movingTextY = textArea.Center.Y - movingTextSize.Y / 2f;
            TextItem.doFontLabel(new Vector2(textArea.Left, movingTextY - 10), $"{MovingText}", GuiData.smallfont, Color.White);


        }
        public override void Update(float t)
        {
            lifetime += t;
            bounds.Height = ramCost;
            switch (currentState)
            {
                case MimikatzState.Loading:
                    Computer target = ComputerLookup.FindByIp(targetIP);
                    if (!TempestGadgets.PassPortComps.ContainsKey(target.idName) && !isReading)
                    {
                        if (lifetime < 3f) return;
                        currentState = MimikatzState.Error;
                        os.terminal.writeLine("[MIMIKZ] No passport found!");
                        return;
                    }
                    else
                    {

                        if (isGranting)
                        {
                            PassPortContent contents = TempestGadgets.PassPortComps[target.idName];
                            loadingTime = contents.entries.Count * 2f;

                            if (lifetime >= loadingTime + 3f)
                            {
                                currentState = MimikatzState.Grant;
                                isGranting = false;
                                string ppEntryId = Args.Length > 2 ? Args[2] : "";
                                GenerateGrantFile(ppEntryId);
                            }
                        }
                        if (isReading && lifetime >= loadingTime)
                        {
                            loadingTime = 5f;
                            if (lifetime >= loadingTime)
                            {
                                currentState = MimikatzState.Read;
                                isReading = false;
                                string ppFile = Args.Length > 2 ? Args[2] : "";
                                ReadPPFile(ppFile);

                            }
                        }

                    }
                    break;
                case MimikatzState.Done:
                    this.isExiting = true;
                    break;

            }
            base.Update(t);
        }

        private void GenerateGrantFile(string PPEntryId = "")
        {
            Computer target = ComputerLookup.FindByIp(targetIP);
            bool matched = false;
            bool isError = false;
            PassPortContent contents = TempestGadgets.PassPortComps[target.idName];
            contents.originID = target.idName;
            Folder userPassPortFolder = os.thisComputer.getFolderFromPath("home/passport", true);

            if (PPEntryId == "*" || PPEntryId == "all" || PPEntryId == "") GenerateAll = true;
            foreach (var entry in contents.entries)
            {
                string filename = $"{target.name}_{entry.id}.passport";
                if (GenerateAll || PPEntryId == entry.id)
                {
                    if (!entry.isActive)
                    {
                        if (!GenerateAll)
                        {
                            os.terminal.writeLine($"[MIMIKZ] Passport {entry.id} is inactive!");
                            isError = true;
                            currentState = MimikatzState.Error;
                            matched = true;
                            break;
                        }
                        continue;
                    }
                    if (userPassPortFolder.containsFile(filename))
                    {
                        var existingFile = userPassPortFolder.searchForFile(filename);
                        userPassPortFolder.files.Remove(existingFile);
                        os.terminal.writeLine($"[MIMIKZ] {filename} already exists! Overwriting...");
                    }
                    FileEntry ppFile = new FileEntry(contents.GetEncodedFileString(entry.id), filename);
                    userPassPortFolder.files.Add(ppFile);
                    os.terminal.writeLine($"[MIMIKZ] Save passport file to /home/passport/{filename}!");
                    target.log($"KERBEROS_PASSPORT_GRANTED:{entry.id}");
                    matched = true;
                    if (!GenerateAll) break;
                }

            }

            if (!matched)
            {
                currentState = MimikatzState.Error;
                os.terminal.writeLine("[MIMIKZ] No passport found!");
                return;
            }

            matched = false;
            GenerateAll = false;
            if (isError) currentState = MimikatzState.Error;
            else currentState = MimikatzState.Done;

        }

        private void ReadPPFile(string ppFile = "")
        {
            if (ppFile != "")
            {
                string filename = ppFile;

                Folder currentFolder = Programs.getCurrentFolder(os);

                if (currentFolder != null && currentFolder.searchForFile(filename) != null)
                {
                    FileEntry passportFile = currentFolder.searchForFile(filename);

                    if (passportFile == null || passportFile.data == null || !passportFile.data.StartsWith("MIMIKATZ_KERBEROS_PASSPORT :: 2.2.0 ------------"))
                    {
                        currentState = MimikatzState.Error;
                        os.terminal.writeLine("[MIMIKZ] Invalid File");
                        return;
                    }

                    PassPortContent content = PassPortContent.GetContentsFromEncodedFileString(passportFile.data);

                    if (content == null)
                    {
                        currentState = MimikatzState.Error;
                        os.terminal.writeLine("[MIMIKZ] Invalid File");
                        return;
                    }
                    os.write("PASSID   |  USER  |  ACTIVE  |  PORTS  |  WHITELIST");
                    os.write("---------------------------------------------------");
                    Thread.Sleep(100);
                    foreach (var entry in content.entries)
                    {
                        Computer c = Programs.getComputer(os, entry.TargetComp);
                        string Whitelist = entry.AddWhitelist != "NONE" && Programs.getComputer(os, entry.AddWhitelist) != null
                            ? Programs.getComputer(os, entry.AddWhitelist).ip
                            : "NONE";

                        if (entry.isActive) os.write($"{entry.id}   |  {(c != null ? c.ip : "UNKNOWN")}  |  {entry.isActive.ToString().ToUpper()}  |  {entry.OpenPorts}  |  {Whitelist}");
                        if (!entry.isActive) os.write($"{entry.id}   |  UNKNOWN  |  {entry.isActive.ToString().ToUpper()}  |  UNKNOWN  |  UNKNOWN");

                    }
                    os.write("");
                    currentState = MimikatzState.Done;
                }
                else
                {
                    currentState = MimikatzState.Error;
                    os.terminal.writeLine("[MIMIKZ] Invalid File");
                    return;
                }
                return;
            }
            else
            {
                Computer target = ComputerLookup.FindByIp(targetIP);
                if (target == null || !TempestGadgets.PassPortComps.ContainsKey(target.idName))
                {
                    if (lifetime < 3f) return;
                    currentState = MimikatzState.Error;
                    os.terminal.writeLine("[MIMIKZ] No passport found!");
                    return;
                }
                PassPortContent contents = TempestGadgets.PassPortComps[target.idName];
                contents.originID = target.idName;
                if (contents.entries.Count > 0 && ppFile == "" && !target.PlayerHasAdminPermissions())
                {
                    os.write("PASSID   |  USER  |  ACTIVE");
                    os.write("---------------------------");
                    Thread.Sleep(100);
                    foreach (var entry in contents.entries)
                    {
                        Computer c = Programs.getComputer(os, entry.TargetComp);
                        if (entry.isActive) os.write($"{entry.id}   |  {(c != null ? c.ip : "UNKNOWN")}  |  {entry.isActive.ToString().ToUpper()}");
                        if (!entry.isActive) os.write($"{entry.id}   |  UNKNOWN  |  {entry.isActive.ToString().ToUpper()}");
                    }
                    os.write("");
                    currentState = MimikatzState.Done;
                }
                else if (contents.entries.Count > 0 && ppFile == "" && target.PlayerHasAdminPermissions())
                {
                    os.write("PASSID   |  USER  |  ACTIVE  |  PORTS  |  WHITELIST");
                    os.write("---------------------------------------------------");
                    Thread.Sleep(100);
                    foreach (var entry in contents.entries)
                    {
                        Computer c = Programs.getComputer(os, entry.TargetComp);
                        string Whitelist = entry.AddWhitelist != "NONE" && Programs.getComputer(os, entry.AddWhitelist) != null
                            ? Programs.getComputer(os, entry.AddWhitelist).ip
                            : "NONE";
                        if (entry.isActive) os.write($"{entry.id}   |  {(c != null ? c.ip : "UNKNOWN")}  |  {entry.isActive.ToString().ToUpper()}  |  {entry.OpenPorts}  |  {Whitelist}");
                        if (!entry.isActive) os.write($"{entry.id}   |  UNKNOWN  |  {entry.isActive.ToString().ToUpper()}  |  UNKNOWN  |  UNKNOWN");

                    }
                    os.write("");
                }
                else
                {
                    currentState = MimikatzState.Error;
                    os.write("No available passport");
                    return;
                }

            }
            currentState = MimikatzState.Done;
        }

    }

}

    public class PassPortEntry
    {
        public string id;
        public string TargetComp;
        public string OpenPorts;
        public bool OverloadProxy;
        public bool CrackFirewall;
        public string AddWhitelist;
        public string LoadAction;
        public bool isActive;
        public int[] Ports;


        public PassPortEntry(string id, string TargetComp, string OpenPorts, bool OverloadProxy, bool CrackFirewall, string AddWhitelist, string LoadAction, bool isActive = true)
        {
            this.id = id;
            this.TargetComp = TargetComp;
            this.OpenPorts = OpenPorts;
            this.OverloadProxy = OverloadProxy;
            this.CrackFirewall = CrackFirewall;
            this.AddWhitelist = AddWhitelist;
            this.LoadAction = LoadAction;
            this.isActive = isActive;
            Ports = OpenPorts == "NONE" ? null : Array.ConvertAll(OpenPorts.Split(','), int.Parse);
        }

        public PassPortEntry(string id, string TargetComp, string OpenPorts, bool OverloadProxy, bool CrackFirewall, string AddWhitelist, bool isActive = true)
        {
            this.id = id;
            this.TargetComp = TargetComp;
            this.OpenPorts = OpenPorts;
            this.OverloadProxy = OverloadProxy;
            this.CrackFirewall = CrackFirewall;
            this.AddWhitelist = AddWhitelist;
            this.LoadAction = "NONE";
            this.isActive = isActive;
            Ports = OpenPorts == "NONE" ? null : Array.ConvertAll(OpenPorts.Split(','), int.Parse);

        }

        public PassPortEntry(string id, string TargetComp, string OpenPorts, bool OverloadProxy, bool CrackFirewall, bool isActive = true)
        {
            this.id = id;
            this.TargetComp = TargetComp;
            this.OpenPorts = OpenPorts;
            this.OverloadProxy = OverloadProxy;
            this.CrackFirewall = CrackFirewall;
            this.AddWhitelist = "NONE";
            this.LoadAction = "NONE";
            this.isActive = isActive;
            Ports = OpenPorts == "NONE" ? null : Array.ConvertAll(OpenPorts.Split(','), int.Parse);

        }

        public PassPortEntry(string id, string TargetComp, string OpenPorts, bool isActive = true)
        {
            this.id = id;
            this.TargetComp = TargetComp;
            this.OpenPorts = OpenPorts;
            this.OverloadProxy = false;
            this.CrackFirewall = false;
            this.AddWhitelist = "NONE";
            this.LoadAction = "NONE";
            this.isActive = isActive;
            Ports = OpenPorts == "NONE" ? null : Array.ConvertAll(OpenPorts.Split(','), int.Parse);

        }


        public PassPortEntry()
        {
            this.id = "NONE";
            this.TargetComp = "#PLAYER_IP#";
            this.OpenPorts = "NONE";
            this.OverloadProxy = false;
            this.CrackFirewall = false;
            this.AddWhitelist = "NONE";
            this.LoadAction = "NONE";
            this.isActive = true;
            this.Ports = null;

        }
        
    }








