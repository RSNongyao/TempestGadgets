using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using TempestGadgets.Utils;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Hacknet.Effects;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Pathfinder.Port;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Hacknet.Gui;



namespace TempestGadgets.Executables
{
    public class EnBreakerExe : Pathfinder.Executable.BaseExecutable
    {
        bool isEnsec, isUnbreakable, isPortsCracked;
        float probability, lifetime, lifetime1, crackTime = 60f;
        int unableCrackLimit;
        List<PortState> portStates = new(), portsNeeded = new();
        List<MatrixRainColumn> matrixRainColumns;
        HashSet<float> warnedForkbombTimes;
        static readonly Random forkbombRand = new();
        List<float> forkbombOpenTime = new();
        bool forkbomb5_25, forkbomb10_35, forkbomb25_50, forkbomb30_55;
        float lastForkbombTriggerTime = -10f;
        const float forkbombTriggerCooldown = 0.2f;

        public EnBreakerExe(Rectangle location, OS os, string[] args) : base(location, os, args)
        {
            ramCost = 320;
            IdentifierName = name = "EnSec Breaker";
            needsProxyAccess = true;
        }

        public override void LoadContent()
        {
            var c = ComputerLookup.FindByIp(targetIP);
            c.hostileActionTaken();
            if (c.firewall != null && !c.firewall.solved)
            {
                needsRemoval = true;
                os.terminal.writeLine("Firewall Activated\nExecution failed");
                return;
            }
            portStates = c.GetAllPortStates();
            unableCrackLimit = portStates.Count + 1;
            if (c.portsNeededForCrack > 100)
            {
                isUnbreakable = c.portsNeededForCrack >= 65536;
                forkbombOpenTime.Clear();
                portsNeeded.Clear();
                if (portStates.Count > 0)
                {
                    var shuffled = portStates.OrderBy(_ => forkbombRand.Next()).ToList();
                    portsNeeded.AddRange(shuffled.Take(Math.Min(4, shuffled.Count)));
                }
                os.warningFlash();
                os.beepSound.Play();
                os.terminal.writeLine("INVIOLABILITY Detected !!! Processing...");
                isEnsec = true;
            }
            base.LoadContent();
        }

        public override void Draw(float t)
        {
            drawOutline();
            drawTarget("app:");
            var drawArea = Hacknet.Utils.InsetRectangle(new Rectangle(bounds.X, bounds.Y + Module.PANEL_HEIGHT, bounds.Width, bounds.Height - Module.PANEL_HEIGHT), 2);
            var textArea = new Rectangle(drawArea.X, drawArea.Y, 2 * drawArea.Width / 3 + 3, drawArea.Height / 4);
            var resultArea = new Rectangle(drawArea.X, drawArea.Y + drawArea.Height / 4, drawArea.Width, drawArea.Height / 4);
            var changingArea = new Rectangle(drawArea.X, drawArea.Y + 3 * drawArea.Height / 8, drawArea.Width, drawArea.Height / 2);
            var c = ComputerLookup.FindByIp(targetIP);
            var color = isEnsec ? os.lockedColor : os.unlockedColor;
            PatternDrawer.draw(drawArea, 1f, Color.Black * 0.1f, color * 0.4f, GuiData.spriteBatch, isEnsec ? PatternDrawer.errorTile : PatternDrawer.thinStripe);
            DrawMatrixRain(changingArea, t);

            if (isEnsec && !isUnbreakable)
            {
                var text = "INVIOLABILITY\nDETECTED";
                float amp = (lifetime < crackTime || !isPortsCracked) ? (0.05f + 0.95f * lifetime / crackTime) : 1f;
                Utils.FlickeringTextEffect.DrawLinedFlickeringText(textArea, text, 2, amp, GuiData.titlefont, os, Hacknet.Utils.AddativeWhite);
                var crackStr = c.portsNeededForCrack.ToString();
                var workingText = new StringBuilder(crackStr);
                var rand = new Random((int)(t * 1000) + c.portsNeededForCrack);
                for (int i = 0; i < workingText.Length; i++)
                {
                    probability = (lifetime < crackTime && isPortsCracked) ? (0.25f + 0.75f * lifetime / crackTime) : 1f;
                    if (rand.NextDouble() >= probability)
                        workingText[i] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()-_=+[]{}|;:',.<>/?"[rand.Next(54)];
                }
                var portTextColor = isPortsCracked ? Color.Lerp(os.brightLockedColor, Hacknet.Utils.AddativeWhite, Math.Min(lifetime / crackTime, 1f)) : os.brightLockedColor;
                GuiData.spriteBatch.DrawString(GuiData.font, $"Ports for Crack: {(lifetime <= crackTime ? workingText.ToString() : c.portsNeededForCrack.ToString())}", new Vector2(resultArea.X, resultArea.Y + 7f), lifetime <= crackTime ? portTextColor : Hacknet.Utils.AddativeWhite, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1);

                DrawPortRects(changingArea, portsNeeded.Count > 0 ? portsNeeded.Count : 4, portsNeeded, lifetime1, true);
            }
            else
            {
                var text = isUnbreakable ? "INVIOLABILITY\nUNBREAKABLE" : "INVIOLABILITY\nDISABLED";
                GuiData.spriteBatch.DrawString(GuiData.titlefont, text, new Vector2(textArea.X, textArea.Y + 12f), isUnbreakable ? Color.Red : Hacknet.Utils.AddativeWhite, 0f, Vector2.Zero, 0.225f, SpriteEffects.None, 1);
                GuiData.spriteBatch.DrawString(GuiData.font, $"Ports for Crack: {(isUnbreakable ? "Error" : c.portsNeededForCrack + 1)}", new Vector2(resultArea.X, resultArea.Y + 7f), isUnbreakable ? os.brightLockedColor : Hacknet.Utils.AddativeWhite, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1);
                DrawPortRects(changingArea, 4, null, lifetime - crackTime, false);
            }

            if (Button.doButton(192018, bounds.Center.X + 40, (bounds.Height + bounds.Y) - 30, 76, 20, "Exit", color))
                isExiting = true;

            base.Draw(t);
        }

        void DrawPortRects(Rectangle area, int rectCount, List<PortState> ports, float animTime, bool expand)
        {
            int w = 100, h = 24, spacing = 10;
            int totalH = rectCount * h + (rectCount - 1) * spacing;
            int startY = 10 + area.Y + (area.Height - totalH) / 2, rectX = area.X;
            float interval = 0.15f, duration = rectCount * interval;
            float time = expand ? animTime : Math.Min(animTime, duration);
            for (int i = 0; i < rectCount; i++)
            {
                int rectY = startY + i * (h + spacing);
                float animStart = i * interval, animEnd = animStart + interval, width = 0;
                if (expand)
                {
                    if (time >= animEnd) width = w;
                    else if (time >= animStart) width = w * MathHelper.Clamp(-((time - animStart) / interval) * ((time - animStart) / interval) + 2 * ((time - animStart) / interval), 0f, 1f);
                }
                else
                {
                    float shrinkTime = Math.Max(0, duration - time);
                    if (shrinkTime >= animStart)
                        width = w * MathHelper.Clamp(-((shrinkTime - animStart) / interval) * ((shrinkTime - animStart) / interval) + 2 * ((shrinkTime - animStart) / interval), 0f, 1f);
                }
                var smallRect = new Rectangle(rectX, rectY, (int)width, h);
                GuiData.spriteBatch.Draw(Hacknet.Utils.white, smallRect, isPortsCracked ? os.thisComputerNode : os.moduleColorSolidDefault);
                if (width >= w - 10)
                {
                    string portText = ports != null && ports.Count > i ? ports[i].PortNumber.ToString() : "???";
                    var textPos = new Vector2(smallRect.X, smallRect.Y + h / 8);
                    GuiData.spriteBatch.DrawString(GuiData.smallfont, $"Port: {portText}", textPos, Hacknet.Utils.AddativeWhite);
                }
            }
        }

        void DrawMatrixRain(Rectangle area, float t)
        {
            if (matrixRainColumns == null)
            {
                matrixRainColumns = new();
                int fontH = (int)(GuiData.font.MeasureString("0").Y * 0.85f);
                int fontW = (int)(GuiData.font.MeasureString("0").X * 0.85f);
                int cols = area.Width / fontW;
                var rand = new Random((int)(lifetime1 * 1000));
                for (int i = 0; i < cols; i++)
                {
                    float x = area.X + i * fontW, y = area.Y + rand.Next(area.Height);
                    float speed = 60f + (float)rand.NextDouble() * 80f;
                    int len = 6 + rand.Next(6);
                    var chars = Enumerable.Range(0, len).Select(_ => GetMatrixChar(rand)).ToList();
                    matrixRainColumns.Add(new MatrixRainColumn(x, y, speed, len, chars));
                }
            }
            int fH = (int)(GuiData.font.MeasureString("0").Y * 0.85f);
            var rand2 = new Random((int)(lifetime1 * 1000) + 42);
            foreach (var col in matrixRainColumns)
            {
                col.Y += col.Speed * t;
                col.Timer += t;
                if (col.Timer > 0.08f)
                {
                    for (int j = 0; j < col.Length; j++)
                        if (rand2.NextDouble() < 0.25) col.Chars[j] = GetMatrixChar(rand2);
                    col.Timer = 0f;
                }
                if (col.Y > area.Y + area.Height)
                {
                    col.Y = area.Y - col.Length * fH;
                    col.Speed = 60f + (float)rand2.NextDouble() * 80f;
                    col.Length = 6 + rand2.Next(6);
                    col.Chars = Enumerable.Range(0, col.Length).Select(_ => GetMatrixChar(rand2)).ToList();
                }
                for (int j = 0; j < col.Length; j++)
                {
                    float drawY = col.Y + j * fH;
                    if (drawY < area.Y || drawY > area.Y + area.Height) continue;
                    var baseColor = isEnsec ? Color.Red : os.brightUnlockedColor;
                    var c = (j == col.Length - 1) ? Color.White : baseColor * (0.7f - 0.05f * (col.Length - j));
                    GuiData.spriteBatch.DrawString(GuiData.detailfont, col.Chars[j].ToString(), new Vector2(col.X, drawY), c, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
            }
        }

        static char GetMatrixChar(Random rand) => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@#$%&"[rand.Next(62)];

        public override void Update(float t)
        {
            lifetime1 += t;
            var c = ComputerLookup.FindByIp(targetIP);
            var comp = ComputerLookup.FindByIp("#PLAYER_IP#");
            if (!isPortsCracked && (portsNeeded.Count > 0 && portsNeeded.All(p => p.Cracked) || portsNeeded.Count == 0))
            {
                isPortsCracked = true;
                os.beepSound.Play();
                os.terminal.writeLine("All Required Ports Cracked,Processing...");
            }
            else if (isPortsCracked) lifetime += t;

            if (isEnsec && !isUnbreakable && isPortsCracked)
            {
                if (!forkbomb5_25 && RandomAddForkbomb(lifetime, 5f, 25f)) forkbomb5_25 = true;
                if (!forkbomb10_35 && RandomAddForkbomb(lifetime, 10f, 25f)) forkbomb10_35 = true;
                if (!forkbomb25_50 && RandomAddForkbomb(lifetime, 25f, 25f)) forkbomb25_50 = true;
                if (!forkbomb30_55 && RandomAddForkbomb(lifetime, 30f, 25f)) forkbomb30_55 = true;
            }

            if (lifetime > crackTime && isEnsec && !isUnbreakable && isPortsCracked)
            {
                isEnsec = false;
                c.portsNeededForCrack = unableCrackLimit - 2;
                var ports = c.GetAllPortStates();
                double rand = forkbombRand.NextDouble();
                if (rand < 0.25)
                    foreach (var port in ports) port.SetCracked(false, "LOCAL_ADMIN");
                for (int i = 0; i < 3; i++) { os.beepSound.Play(); os.warningFlash(); }
                os.terminal.writeLine($"INVIOLABILITY disabled on {targetIP} !!!");
            }

            warnedForkbombTimes ??= new();
            var triggered = new List<float>();
            foreach (var openTime in forkbombOpenTime.ToList())
            {
                if (openTime - lifetime > 0 && openTime - lifetime <= 3.0f && !warnedForkbombTimes.Contains(openTime))
                {
                    Multiplayer.parseInputMessage(HackerScriptExecuter.getBasicNetworkCommand("cConnection", comp, c), os);
                    os.IncConnectionOverlay.Activate();
                    os.terminal.writeLine("INCOMING ForkBomb !!!");
                    warnedForkbombTimes.Add(openTime);
                }
                if (openTime - lifetime < 0.001f && lifetime - lastForkbombTriggerTime >= forkbombTriggerCooldown)
                {
                    Multiplayer.parseInputMessage(HackerScriptExecuter.getBasicNetworkCommand("eForkBomb", comp, c), os);
                    comp.disconnecting(c.ip, true);
                    lastForkbombTriggerTime = lifetime;
                    triggered.Add(openTime);
                }
            }
            foreach (var tTime in triggered)
            {
                forkbombOpenTime.Remove(tTime);
                warnedForkbombTimes.Remove(tTime);
            }
            base.Update(t);
        }

        public bool RandomAddForkbomb(float timer, float start, float duration)
        {
            if (timer > start && timer <= (start + duration))
            {
                if (forkbombRand.NextDouble() < 0.75)
                {
                    float openTime = forkbombRand.Next((int)(start * 1000), (int)((start + duration) * 1000)) / 1000f;
                    forkbombOpenTime.Add(openTime);
                }
                return true;
            }
            return false;
        }

        class MatrixRainColumn
        {
            public float X, Y, Speed, Timer;
            public int Length;
            public List<char> Chars;
            public MatrixRainColumn(float x, float y, float speed, int length, List<char> chars)
            { X = x; Y = y; Speed = speed; Length = length; Chars = chars; Timer = 0f; }
        }
    }
}



