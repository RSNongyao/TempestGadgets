using BepInEx;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder.Util;
using Hacknet;
using Microsoft.Xna.Framework.Graphics;
using TempestGadgets.Utils;
using Hacknet.Effects;

// THANKS to April_Crystal for graphics and code!!!

namespace TempestGadgets.Executables
{
    public class SignalFilterExe : Pathfinder.Executable.BaseExecutable
    {
        private bool isComplete = false;
        private float lifetime;
        private int CompleteRamCost = 220;
        private float preciseRamCost = 350f;
        private int SIGNALPort;

        // 信号波形变量
        private const int WAVE_POINTS = 3000;
        private float[] waveHeights = new float[WAVE_POINTS];
        private float waveAmplitude = 2.0f; // 大幅增加振幅
        private float signalIntegrity = 0f;
        private float pulseTimer = 0f;

        // 横幅显示文本
        private const string WARNING_TEXT = "ACTIVE PORT CRACKING";
        private const string SUCCESS_LINE1 = "Success";
        private const string SUCCESS_LINE2 = "Keep this running!";
        private float warningPulse = 0f;

        // 背景颜色相关
        private Color backgroundColor = Color.Transparent;
        private float bgAlpha = 0f;

        List<int> operationPorts = new List<int>();

        public SignalFilterExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
        {
            name = "RtSpSignalFilter";
            ramCost = 350;
            preciseRamCost = ramCost;
            IdentifierName = "RtSp Signal Filter";

            SignalWaves.InitializeSignalWave(waveHeights);
        }


        public override void LoadContent()
        {

            foreach (var exe in os.exes)
            {
                if (exe is SignalFilterExe filterExe && filterExe != this && filterExe.targetIP == this.targetIP)
                {
                    os.write("SignalFilter is running on this node!");
                    os.write("Execution failed");
                    needsRemoval = true;
                    return;
                }
            }

            Computer c = ComputerLookup.FindByIp(targetIP);
            SIGNALPort = c.GetDisplayPortNumberFromCodePort(32);
            bool isPortExisit = PortDetect.IsHasPort(c, SIGNALPort);

            if (Args.Length < 2)
            {
                os.write("No port number Provided");
                os.write("Execution failed");
                needsRemoval = true;
                return;
            }
            else if (Int32.Parse(Args[1]) != SIGNALPort || !isPortExisit)
            {
                os.write("Target Port is Closed");
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
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width, this.bounds.Height - Module.PANEL_HEIGHT), 2);

            // 1. 首先绘制背景（完全不透明）
            if (bgAlpha > 0)
            {
                spriteBatch.Draw(Hacknet.Utils.white, drawArea, backgroundColor);
            }

            if (!isExiting)
            {

                SignalWaves.DrawSignalAnimation(drawArea, waveHeights);

                // 3. 绘制进度条（完全不透明）
                if (!isComplete)
                {
                    DrawSignalIntegrityBar();
                }

                // 4. 绘制横幅（完全不透明）
                if (!isComplete)
                {
                    DrawWarningBanner();
                }
                else
                {
                    DrawSuccessBanner();
                }
            }
            base.Draw(t);

        }


        private void DrawSignalIntegrityBar()
        {
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width, this.bounds.Height - Module.PANEL_HEIGHT), 2);

            // 将信号条放在窗口底部
            int barHeight = 15;
            int barY = drawArea.Y + drawArea.Height - barHeight - 10;

            // 确保在窗口边界内
            barY = Math.Min(barY, drawArea.Bottom - barHeight - 10);

            int barWidth = (int)(drawArea.Width * signalIntegrity * 0.9f);

            // 绘制背景（深灰色）
            spriteBatch.Draw(Hacknet.Utils.white,
                new Rectangle(drawArea.X + (int)(drawArea.Width * 0.05f), barY, (int)(drawArea.Width * 0.9f), barHeight),
                new Color(30, 30, 40));

            // 绘制进度
            Color progressColor = Color.Lerp(
                new Color(180, 60, 180),
                new Color(80, 180, 255),
                signalIntegrity
            );

            spriteBatch.Draw(Hacknet.Utils.white,
                new Rectangle(drawArea.X + (int)(drawArea.Width * 0.05f), barY, barWidth, barHeight),
                progressColor);

            // 绘制进度文本
            string integrityText = $"Signal Integrity: {(int)(signalIntegrity * 100)}%";
            Vector2 textSize = GuiData.tinyfont.MeasureString(integrityText);
            Vector2 textPos = new Vector2(
                drawArea.X + (drawArea.Width - textSize.X) / 2,
                barY - textSize.Y - 5
            );

            spriteBatch.DrawString(GuiData.tinyfont, integrityText, textPos, Color.LightGray);
        }

        private void DrawWarningBanner()
        {
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width, this.bounds.Height - Module.PANEL_HEIGHT), 2);
            int bannerHeight = 30;
            int bannerY = drawArea.Y + 5;

            // 确保在窗口边界内
            bannerY = Math.Max(bannerY, drawArea.Y + 5);
            bannerHeight = Math.Min(bannerHeight, drawArea.Height - 10);

            // 横幅颜色（红色）
            float blink = (float)Math.Sin(warningPulse * 10f) * 0.5f + 0.5f;
            Color bannerColor = new Color(150 + (int)(80 * blink), 40, 40);

            // 绘制横幅背景
            spriteBatch.Draw(Hacknet.Utils.white,
                new Rectangle(drawArea.X, bannerY, drawArea.Width, bannerHeight),
                bannerColor);

            // 绘制横幅文本 - 居中显示
            Vector2 textSize = GuiData.tinyfont.MeasureString(WARNING_TEXT);
            Vector2 textPos = new Vector2(
                drawArea.X + (drawArea.Width - textSize.X) / 2,
                bannerY + (bannerHeight - textSize.Y) / 2
            );

            spriteBatch.DrawString(GuiData.tinyfont, WARNING_TEXT,
                textPos,
                Color.White);
        }

        private void DrawSuccessBanner()
        {
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width, this.bounds.Height - Module.PANEL_HEIGHT), 2);

            // 在窗口中央绘制成功横幅
            int centerX = drawArea.X + drawArea.Width / 2;
            int centerY = drawArea.Y + drawArea.Height / 2;

            // 横幅尺寸
            int bannerWidth = Math.Min(450, (int)(drawArea.Width * 0.85f));
            int bannerHeight = 90;

            // 横幅位置
            int bannerX = centerX - bannerWidth / 2;
            int bannerY = centerY - bannerHeight / 2;

            // 背景颜色 - 深绿色
            float pulse = 0.6f + 0.4f * (float)Math.Sin(pulseTimer * 5f);
            Color bannerColor = new Color(0, (int)(120 * pulse), 0);

            // 绘制横幅背景
            spriteBatch.Draw(Hacknet.Utils.white,
                new Rectangle(bannerX, bannerY, bannerWidth, bannerHeight),
                bannerColor);


            // 绘制文本
            Vector2 line1Size = GuiData.font.MeasureString(SUCCESS_LINE1);
            Vector2 line2Size = GuiData.smallfont.MeasureString(SUCCESS_LINE2);

            // 计算文本位置 - 垂直居中
            float totalTextHeight = line1Size.Y + line2Size.Y;
            float textY1 = centerY - totalTextHeight / 2;
            float textY2 = textY1 + line1Size.Y;

            // 第一行文本 - "Success"
            spriteBatch.DrawString(GuiData.font, SUCCESS_LINE1,
                new Vector2(centerX - line1Size.X / 2, textY1),
                Color.LimeGreen);

            // 第二行文本 - "Keep this running"
            spriteBatch.DrawString(GuiData.smallfont, SUCCESS_LINE2,
                new Vector2(centerX - line2Size.X / 2, textY2),
                Color.LimeGreen);
        }

        public override void Update(float t)
        {
            Computer c = ComputerLookup.FindByIp(targetIP);

            pulseTimer += t;
            warningPulse += t;

            if (!isComplete)
            {
                lifetime += t;
                signalIntegrity = MathHelper.Clamp(lifetime / 15f, 0, 1);
                UpdateSignalWave(t);

                backgroundColor = new Color(60, 0, 0);
                bgAlpha = Math.Min(bgAlpha + t * 0.5f, 0.7f);

                if (lifetime >= 15f)
                {
                    isComplete = true;
                    c.openPort(SIGNALPort, os.thisComputer.ip);
                    c.GetPortState("sigscramble").SetCracked(true, os.thisComputer.ip);
                    os.write("SignalFilter Operation Complete.");
                    os.write("Keep running before disconnect!!!");

                    RandomizePorts();

                }
            }
            else
            {
                UpdateSignalWave(t);
                // 更新背景为深绿色
                backgroundColor = new Color(0, 40, 0);
                bgAlpha = Math.Min(bgAlpha + t * 0.5f, 0.7f);

                // 完成后降低内存占用，减少速度随时间二次增长
                if (ramCost > CompleteRamCost)
                {
                    float elapsed = lifetime - 15f; // 完成后经过的时间
                    float speed = 10f + 3.5f * elapsed * elapsed; // 二次函数增长
                    preciseRamCost -= t * speed;
                    ramCost = (int)preciseRamCost;
                    if (ramCost < CompleteRamCost)
                    {
                        ramCost = CompleteRamCost;
                    }
                }
            }
            base.Update(t);
        }

        private void UpdateSignalWave(float t)
        {
            for (int i = 0; i < WAVE_POINTS; i++)
            {
                if (isComplete)
                {
                    // 完成后：规律的正弦波
                    float positionFactor = i / (float)WAVE_POINTS;
                    waveHeights[i] = (float)Math.Sin(pulseTimer * 5f + positionFactor * MathHelper.TwoPi * 6) * 1.5f;
                }
                else
                {
                    // 破解中：快速变化的波形
                    float noiseStrength = (1 - signalIntegrity) * 0.8f + 0.2f;
                    float positionFactor = i / (float)WAVE_POINTS;
                    float timeFactor = pulseTimer * 12f; // 增加速度

                    // 使用多个频率组合创建波形
                    waveHeights[i] =
                        (float)Math.Sin(positionFactor * 30f + timeFactor) * 0.5f +
                        (float)Math.Sin(positionFactor * 50f + timeFactor * 2.5f) * 0.4f +
                        (float)Math.Cos(positionFactor * 15f + timeFactor * 1.0f) * 0.7f;

                    waveHeights[i] *= noiseStrength * waveAmplitude;
                }
            }
        }



        private void RandomizePorts()
        {
            Computer c = ComputerLookup.FindByIp(targetIP);
            if (c == null) return;

            var allPorts = new HashSet<int>();
            foreach (var port in c.GetAllPortStates())
            {
                allPorts.Add(port.PortNumber);
            }
            if (allPorts.Count == 0) return;

            HashSet<int> excludedPorts = new HashSet<int> { 0, 123, 3659, SIGNALPort };
            List<int> availablePorts = allPorts.Where(p => !excludedPorts.Contains(p)).ToList();
            if (availablePorts.Count == 0) return;


            Random rand = new Random();
            int OperationsCount = rand.Next(1, Math.Min(6, availablePorts.Count) + 1);
            Console.WriteLine($"Randomizing {OperationsCount} ports...");

            for (int i = 0; i < OperationsCount; i++)
            {

                int targetPort = availablePorts[rand.Next(0, availablePorts.Count)];
                operationPorts.Add(targetPort);
                Console.WriteLine($"Randomizing port {targetPort}");

                int isPortOpen = rand.Next(0, 2);
                if (isPortOpen == 1)
                {
                    c.openPort(targetPort, os.thisComputer.ip);
                }
                else
                {
                    c.closePort(targetPort, os.thisComputer.ip);
                }
            }

        }




        public override void Killed()
        {
            Computer c = ComputerLookup.FindByIp(targetIP);
            c.closePort(SIGNALPort, os.thisComputer.ip);
            c.GetPortState("sigscramble").SetCracked(false, os.thisComputer.ip);
            isExiting = true;

            foreach (var port in operationPorts)
            {
                c.closePort(port, os.thisComputer.ip);
            }
            base.Killed();
        }
    }
}
