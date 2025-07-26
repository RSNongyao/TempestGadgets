using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using TempestGadgets.Utils;
using Hacknet.Gui;


public class VPNBypassExe : Pathfinder.Executable.BaseExecutable
{

    private float lifetime = 0f;
    private int VPNPort;
    private int SSLPort;
    public static Texture2D point;
    public static float pointRatio = 2.5f;
    public float Radius;
    public Color lineColor = Color.White;
    public float crackTime = 10f;
    private bool VPNPortOpened = false;

    public VPNBypassExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = 360;
        IdentifierName = "VPNBypass";
        needsProxyAccess = true;
        name = "VPN Bypass";

    }

    public override void LoadContent()
    {
        Computer c = ComputerLookup.FindByIp(targetIP);
        VPNPort = c.GetDisplayPortNumberFromCodePort(123);
        SSLPort = c.GetDisplayPortNumberFromCodePort(443);
        bool isPortExisit = PortDetect.IsHasPort(c, VPNPort);

        foreach (var exe in os.exes)
        {
            if (exe is VPNBypassExe)
            {
                this.needsRemoval = true;
                os.terminal.writeLine("[ERROR] Only one VPN Tunnel can be opened at a time!");
                return;
            }
        }

        if (Args.Length < 2)
        {
            os.write("No port number Provided");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }
        else if (Int32.Parse(Args[1]) != VPNPort || !isPortExisit)
        {
            os.write("Target Port is Closed");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }
        if (!c.isPortOpen(SSLPort))
        {
            os.write("HTTPS(SSL) needed to build VPN Tunnel!");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }
        c.hostileActionTaken();
        base.LoadContent();
    }

    public override void Draw(float t)
    {
        drawOutline();
        drawTarget("app:");
        point = os.content.Load<Texture2D>("Circle");
        Vector2 scale = new Vector2(pointRatio / point.Width);
        List<Vector2> tangentPoints = GenerateArithmeticPoints();
        Vector2 center = new Vector2(Bounds.Center.X, Bounds.Center.Y);
        Rectangle rectangle = new Rectangle(this.bounds.X, this.bounds.Y, this.bounds.Width, this.bounds.Height);
        Vector2 realStart = new Vector2();
        Vector2 realEnd = new Vector2();

        foreach (Vector2 tangent in tangentPoints)
        {
            GuiData.spriteBatch.Draw(point, tangent, null, Color.White, 0f, new Vector2(point.Width / 2f, point.Height / 2f), scale, SpriteEffects.None, 0f);
            Vector2 dirAlt = tangent - center;
            if (dirAlt.LengthSquared() > 0.0001f)
            {
                dirAlt.Normalize();
                Vector2 tangentDir = new Vector2(dirAlt.Y, -dirAlt.X);
                float tangentLength = 1000f;
                Vector2 start = tangent + tangentDir * tangentLength / 2f;
                Vector2 end = tangent - tangentDir * tangentLength / 2f;
                if (CohenClip.CohenSutherlandClip(rectangle, start, end, out realStart, out realEnd))
                {
                    start = realStart;
                    end = realEnd;
                    Utils.drawLine(GuiData.spriteBatch, start, end, new Vector2(0, 0), lineColor, 0f);
                }
                else
                {
                    Utils.drawLine(GuiData.spriteBatch, start, end, new Vector2(0, 0), lineColor, 0f);
                }
            }
        }


        Color TextColor = Color.White;
        Rectangle dest = new Rectangle(1, bounds.Y + (int)(bounds.Height * 0.1f), (int)(bounds.Width * 0.7f), 40);
        if (lifetime < crackTime && lifetime >= 0f)
        {
            TextItem.doRightAlignedBackingLabelFill(dest, "Opening Tunnels...", GuiData.detailfont, new Color(0, 0, 0, 200), TextColor);
        }
        if (lifetime >= crackTime)
        {
            TextColor = Color.LimeGreen;
            TextItem.doRightAlignedBackingLabelFill(dest, "Tunnel Established :P", GuiData.detailfont, new Color(0, 0, 0, 200), TextColor);
        }
        base.Draw(t);
    }
    List<Vector2> GenerateArithmeticPoints()
    {
        float PowerMult = GetPowerMult();
        float first = (float)(Math.Pow(10, -PowerMult) * Math.PI);
        float max = (float)(2 * Math.PI);
        float lastPowerMult = float.NaN;
        bool IsGenerating = true;
        List<float> Sequence = new List<float>();
        if (PowerMult != lastPowerMult)
        {
            Sequence.Clear();
            float value = first;
            while (value <= max && IsGenerating == true)
            {
                Sequence.Add(value);
                value += first;
            }

            if (Sequence.Count > 0)
            {
                float last = Sequence[Sequence.Count - 1];
                if (Math.Abs(last - max) <= 1e-4f || Math.Abs((last % max)) <= 1e-4f)
                {
                    Sequence[Sequence.Count - 1] = max;
                    IsGenerating = false;
                }
                else if (Math.Abs(last - max) >= first / 2 && Math.Abs(last - max) > 1e-4f)
                {
                    Sequence.Add(value);
                    value += first;
                    IsGenerating = false;
                }
                else if (Math.Abs(last - max) < first / 2 && Math.Abs(last - max) > 1e-4f)
                {
                    IsGenerating = false;
                }
            }
            lastPowerMult = PowerMult;
        }

        List<Vector2> Points = new List<Vector2>();
        foreach (float value in Sequence)
        {
            float x = (float)(Bounds.Center.X + Radius * Math.Cos(value + lifetime));
            float y = (float)(Bounds.Center.Y + Radius * Math.Sin(value + lifetime));
            Points.Add(new Vector2(x, y));
        }
        return Points;

        float GetPowerMult()
        {
            float runTime = lifetime;
            if (runTime >= 0f && runTime < 2.25f)
            {
                return (float)(0.1 * Math.Floor(4 * runTime / 3));
            }
            if (runTime >= 2.25f && runTime < 4.25f)
            {
                return (float)(0.1 * Math.Floor(2 * runTime - 1.5f));
            }
            if (runTime >= 4.25f && runTime < 5.345f)
            {
                return (float)(0.1 * Math.Floor(32 * runTime / 7 - 87 / 7));
            }
            if (runTime >= 5.345f)
            {
                return 1.2f;
            }
            return 0f;
        }
    }

    public override void Update(float t)
    {
        Computer comp = Programs.getComputer(os, targetIP);
        if (lifetime >= crackTime && isExiting == false && !VPNPortOpened)
        {
            lineColor = Color.Cyan;
            comp.openPort(VPNPort, os.thisComputer.ip);
            comp.closePort(SSLPort, os.thisComputer.ip);
            os.write("VPN Tunnel Established.");
            VPNPortOpened = true;
        }

        float minRamCost = 220f;
        float maxRamCost = 360f;
        float reduceDuration = 5f;
        float elapsed = lifetime - crackTime;
        if (elapsed < 0f) elapsed = 0f;
        if (elapsed < reduceDuration)
        {
            float tNorm = elapsed / reduceDuration;
            ramCost = (int)(maxRamCost - (maxRamCost - minRamCost) * tNorm);
        }
        else
        {
            ramCost = (int)minRamCost;
        }
        bounds.Height = ramCost;
        lifetime += t;

        if (lifetime < 7f && lifetime >= 0f)
        {
            Radius = 80f;
        }
        if (lifetime >= 8f && lifetime < 14f)
        {
            Radius -= 12f * t;
        }
        if (lifetime >= 14f)
        {
            Radius = 8f;
        }
    }

    public override void Killed()
    {
        if (VPNPortOpened)
        {
            var comp = Programs.getComputer(os, targetIP);
            if (comp != null)
            {
                comp.closePort(SSLPort, os.thisComputer.ip);
                comp.closePort(VPNPort, os.thisComputer.ip);
                os.write("VPN Tunnel Closed.");
            }
            VPNPortOpened = false;
        }
        base.Killed();
    }



}



