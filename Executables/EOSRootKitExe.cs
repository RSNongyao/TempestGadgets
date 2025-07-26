using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Hacknet.Extensions;
using TempestGadgets.Utils;

// special THANKS to BI3TKL for graphics!!!
public class EOSRootKitExe : Pathfinder.Executable.BaseExecutable
{
    public int EOSPort;
    public enum eOSState { Intro, MainIntro, MainBody, Ending }

    private const float INTRO_TIME = 3f;
    private const float MAIN_INTRO_TIME = 3f;
    private const float MAIN_BODY_TIME = 3f;
    private const float ENDING_TIME = 1.2f;
    private string[] bodyText;
    private Color flashColor;
    private Color brightDrawColor;
    private float initStringCharDelay = 0.1f;
    private float currentStateTimer;
    private float timeTaken;
    private eOSState state;

    public const string initText =
        "Initializing###.#.#.#\n" +
        "Packeting###.#.#.#.#.#.#.#.#..#.\n" +
        "Injecting Semi-Tethered Exploits###.#.#.##.#.#.#";

    public const string mainIntroText =
        "eOS Vulnerability DETECED\n" +
        "##Initializing eOS Root Dump##>#>##>#>#>#>>>>>";

    public const string errorIntroText =
        "e05^ Vu$$r$$ty CO3@PI\"} DGT^C.D\n" +
        "##Ini$$!l^zi/g eO: -!0t 3@Hp##>#>##>#>#>#>>>>>";



    public EOSRootKitExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = 300;
        IdentifierName = "EOSRootKit";
        needsProxyAccess = true;
        name = "EOS Rootkit";

    }

    public override void LoadContent()
    {
        Computer comp = ComputerLookup.FindByIp(targetIP);
        EOSPort = comp.GetDisplayPortNumberFromCodePort(3659);
        bool isPortExist = PortDetect.IsHasPort(comp, EOSPort);

        if (Args.Length < 2)
        {
            os.write("No port number Provided");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }
        else if (Int32.Parse(Args[1]) != EOSPort || !isPortExist)
        {
            os.write("Target Port is Closed");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }
        else if (comp.adminPass == "alpine" || comp.isPortOpen(EOSPort))
        {
            os.write("No exploits found");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }



            initStringCharDelay = INTRO_TIME / initText.Replace("#", "@@").Length;

        var segment = initText + "\n\n" + mainIntroText + "\n\n" + errorIntroText;
        var fullText = string.Join("\n", Enumerable.Repeat(segment, 3));


        string extensionFolder = ExtensionLoader.ActiveExtensionInfo.FolderPath;
        string rootkitDumpPath = Path.Combine(extensionFolder, "RootkitDump.txt");
        if (File.Exists(rootkitDumpPath))
        {
            bodyText = File.ReadAllText(rootkitDumpPath).Split(Utils.newlineDelim);
        }
        else
        {
            bodyText = new string[] { "RootkitDump.txt not found!" };
        }


        flashColor = os.lockedColor; flashColor.A = 0;
        brightDrawColor = os.unlockedColor; brightDrawColor.A = 0;

        Programs.getComputer(os, targetIP).hostileActionTaken();
        base.LoadContent();
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawOutline();
        drawTarget("app:");

        Rectangle dest = bounds;
        dest.Inflate(-2, -(PANEL_HEIGHT + 1));
        dest.Y += PANEL_HEIGHT;

        drawBackground(dest);

        switch (state)
        {
            case eOSState.Intro: DrawIntro(dest); break;
            case eOSState.MainIntro: DrawMainIntro(dest); break;
            case eOSState.MainBody: DrawMainBody(dest); break;
            case eOSState.Ending: DrawEnding(dest); break;
        }
    }

    private void drawBackground(Rectangle dest)
    {
        spriteBatch.Draw(Utils.gradient, dest, os.highlightColor * 0.2f);
    }

    private string getDelayDrawString(string original, float timeSec)
    {
        string result = string.Empty;
        float cumulative = 0f;

        foreach (char c in original)
        {
            cumulative += initStringCharDelay;
            if (c == '#') cumulative += initStringCharDelay;
            if (timeSec >= cumulative && c != '#')
                result += c;
        }

        return result;
    }




    // 状态切换逻辑
    private void DrawIntro(Rectangle dest)
    {
        var text = getDelayDrawString(initText, currentStateTimer);
        spriteBatch.DrawString(GuiData.detailfont, text, new Vector2(dest.X + 2, dest.Y + 2), Color.White);
    }

    private void DrawMainIntro(Rectangle dest)
    {
        if (currentStateTimer < 0.8f)
        {
            if (currentStateTimer % 0.2f < 0.07f)
                PatternDrawer.draw(dest, 3f, os.darkBackgroundColor * 0.2f, os.lockedColor, spriteBatch, PatternDrawer.binaryTile);
            else
                spriteBatch.DrawString(
                    GuiData.detailfont,
                    errorIntroText.Replace("#", string.Empty),
                    new Vector2(dest.X + 2, dest.Y + 2),
                    Color.White
                );
        }
        else
        {
            var text = getDelayDrawString(mainIntroText, currentStateTimer - 0.8f);
            spriteBatch.DrawString(GuiData.detailfont, text, new Vector2(dest.X + 2, dest.Y + 2), Color.White);
        }
    }

    private void DrawMainBody(Rectangle dest)
    {
        // 标题
        spriteBatch.DrawString(GuiData.detailfont, mainIntroText.Replace("#", string.Empty), new Vector2(dest.X + 2, dest.Y + 2), Color.White * fade);

        // 动态计算滚动速度，确保正文在 MAIN_BODY_TIME 内完全显示
        int num3 = 6;
        int num4 = (dest.Height - 30) / num3;
        int totalLines = bodyText.Length;
        float scrollDuration = MAIN_BODY_TIME; // 3f
        int num = (int)(currentStateTimer / scrollDuration * totalLines);
        if (num > totalLines) num = totalLines;

        int num5 = 0;
        if (num > num4)
        {
            num5 = num - num4;
            if (num5 < 0) num5 = 0;
        }
        Vector2 position = new Vector2(dest.X + 2, dest.Y + 30);
        for (int i = num5; i < num; i++)
        {
            spriteBatch.DrawString(GuiData.detailfont, bodyText[i], position, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.3f);
            position.Y += num3;
        }
    }

    private void DrawEnding(Rectangle dest)
    {
        float num = this.currentStateTimer;
        this.currentStateTimer = 5f;
        this.currentStateTimer = num;
        Rectangle destinationRectangle = new Rectangle(dest.X, dest.Y + dest.Height / 3, dest.Width, dest.Height / 3);
        this.spriteBatch.Draw(Utils.white, destinationRectangle, this.os.unlockedColor * 0.8f);
        destinationRectangle.Height -= 6;
        destinationRectangle.Y += 3;
        this.spriteBatch.Draw(Utils.white, destinationRectangle, this.os.indentBackgroundColor * 0.8f);
        string text = "Jailbreak";
        Vector2 vector = GuiData.font.MeasureString(text);
        Vector2 vector2 = new Vector2((float)(destinationRectangle.X + destinationRectangle.Width / 2) - vector.X / 2f, (float)(destinationRectangle.Y + destinationRectangle.Height / 2) - vector.Y / 2f);
        this.spriteBatch.DrawString(GuiData.font, text, vector2 - Vector2.One, this.brightDrawColor * this.fade);
        this.spriteBatch.DrawString(GuiData.font, text, vector2 + Vector2.One, this.brightDrawColor * this.fade);
        this.spriteBatch.DrawString(GuiData.font, text, vector2, Color.White * this.fade);
    }
    private void UpdateState()
    {
        if (timeTaken < INTRO_TIME) state = eOSState.Intro;
        else if (timeTaken < INTRO_TIME + MAIN_INTRO_TIME) state = eOSState.MainIntro;
        else if (timeTaken < INTRO_TIME + MAIN_INTRO_TIME + MAIN_BODY_TIME) state = eOSState.MainBody;
        else if (timeTaken < INTRO_TIME + MAIN_INTRO_TIME + MAIN_BODY_TIME + ENDING_TIME)
            state = eOSState.Ending;
        else if (!isExiting)
        {
            Completed();
            isExiting = true;
        }
    }
    // 状态逻辑结束



    public override void Update(float t)
    {
        base.Update(t);
        timeTaken += t;
        currentStateTimer += t;

        var previous = state;
        UpdateState();
        if (state != previous)
            currentStateTimer = 0f;
    }

    public override void Completed()
    {
        base.Completed();
        var comp = Programs.getComputer(os, targetIP);
        comp.openPort(EOSPort, os.thisComputer.ip);

        if (comp.adminPass != "alpine")
        {
            os.warningFlash();
            os.write("[ROOTKIT] Jailbreak device detected.");
            os.write("[ROOTKIT] Device will reboot in 60 seconds.");
            os.delayer.Post(ActionDelayer.Wait(60f), () =>
            {
                comp.closePort(EOSPort, os.thisComputer.ip);
                comp.adminPass = PortExploits.getRandomPassword();
                comp.adminIP = "";
                comp.currentUser = new UserDetail();
                comp.reboot(os.thisComputer.ip);
            });
        }
    }


}