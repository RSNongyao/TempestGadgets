using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Hacknet.Gui;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics.Eventing.Reader;
using TempestGadgets.Utils;


public class NetSpoofExe : Pathfinder.Executable.BaseExecutable
{
    private int TRANSPort;
    private float lifetime = 0f;
    public NetSpoofExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = 350;
        IdentifierName = "NetSpoof";
        needsProxyAccess = true;
        name = "NetSpoof";

    }

    public override void LoadContent()
    {
        Computer c = ComputerLookup.FindByIp(targetIP);
        TRANSPort = c.GetDisplayPortNumberFromCodePort(211);
        bool isPortExist = PortDetect.IsHasPort(c, TRANSPort);


        if (Args.Length < 2)
        {
            os.write("No port number Provided");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }
        else if (Int32.Parse(Args[1]) != TRANSPort || !isPortExist)
        {
            os.write("Target Port is Closed");
            os.write("Execution failed");
            needsRemoval = true;
            return;
        }
        base.LoadContent();
    }

    public override void Draw(float t)
    {
        drawOutline();
        drawTarget("app:");

        Rectangle drawArea = Utils.InsetRectangle(new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width, this.bounds.Height - Module.PANEL_HEIGHT), 2);
        int CentralWidth = drawArea.Width / 17;
        int CentralHeight = drawArea.Height / 3;
        Rectangle CentralRect = new Rectangle(drawArea.Center.X - CentralWidth / 2, drawArea.Center.Y - CentralHeight / 2, CentralWidth, CentralHeight);
        spriteBatch.Draw(Utils.white, CentralRect, Color.Coral);
        Vector2 lineStart = new Vector2(CentralRect.Right, CentralRect.Y + CentralHeight / 2);

        int gapToCentral = 5 * drawArea.Width / 34;
        int rightRectMaxWidth = (drawArea.Right - gapToCentral - CentralWidth - CentralRect.Left);
        int gapBetweenRects = 4;
        float leftHeight = drawArea.Height - (7 * gapBetweenRects);
        int rightRectHeight = (int)(leftHeight / 8);
        int totalRectsHeight = 8 * rightRectHeight + 7 * gapBetweenRects;
        int startY = drawArea.Y;


        Color lineColor = Color.White;
        Vector2 Anchor1 = new Vector2(CentralRect.Right + gapToCentral / 2, CentralRect.Y + CentralHeight / 2);
        Utils.drawLine(GuiData.spriteBatch, lineStart, Anchor1, new Vector2(0, 0), Color.White, 0f);

        List<Rectangle> userRect = new List<Rectangle>();

        float[] widthFactors = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1f, 0.75f, 0.5f, 0.25f };
        float[] lineFactors = new float[] { 1.3f, 0.85f, 0.55f, 0.20f, 0.20f, 0.55f, 0.85f, 1.3f };

        for (int i = 0; i < 8; i++)
        {
            int rectY = startY + i * (rightRectHeight + gapBetweenRects);
            int rectWidth = (int)(rightRectMaxWidth * widthFactors[i]);
            int rectX = drawArea.Right - rectWidth;
            Rectangle rightRect = new Rectangle(rectX, rectY, rectWidth, rightRectHeight);
            Vector2 lineEnd = new Vector2(rightRect.Left, rightRect.Top + rightRect.Height / 2);
            userRect.Add(rightRect);
            Vector2 Anchor2 = new Vector2(rightRect.Left - gapToCentral * lineFactors[i], lineEnd.Y);
            Utils.drawLine(GuiData.spriteBatch, Anchor1, Anchor2, new Vector2(0, 0), lineColor, 0f);
            Utils.drawLine(GuiData.spriteBatch, Anchor2, lineEnd, new Vector2(0, 0), lineColor, 0f);
        }

        Color RectColor;
        if (lifetime <= 3f)
        {
            RectColor = GreenToRed(0f, 3f);
            foreach (Rectangle rectangle in userRect.GetRange(1, 6))
            {
                spriteBatch.Draw(Utils.white, rectangle, os.brightUnlockedColor);
            }
            spriteBatch.Draw(Utils.white, userRect[0], RectColor);
            spriteBatch.Draw(Utils.white, userRect[7], RectColor);

        }
        else if (lifetime > 3f && lifetime <= 8f)
        {
            RectColor = GreenToRed(3f, 5f);
            foreach (Rectangle rectangle in userRect.GetRange(2, 5))
            {
                spriteBatch.Draw(Utils.white, rectangle, os.brightUnlockedColor);
            }
            spriteBatch.Draw(Utils.white, userRect[0], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[7], os.brightLockedColor);

            spriteBatch.Draw(Utils.white, userRect[1], RectColor);
            spriteBatch.Draw(Utils.white, userRect[6], RectColor);

        }
        else if (lifetime > 8f && lifetime <= 14.5f)
        {
            RectColor = GreenToRed(8f, 6.5f);
            foreach (Rectangle rectangle in userRect.GetRange(3, 4))
            {
                spriteBatch.Draw(Utils.white, rectangle, os.brightUnlockedColor);
            }
            spriteBatch.Draw(Utils.white, userRect[0], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[1], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[6], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[7], os.brightLockedColor);

            spriteBatch.Draw(Utils.white, userRect[2], RectColor);
            spriteBatch.Draw(Utils.white, userRect[5], RectColor);

        }
        else if (lifetime > 14.5f)
        {
            RectColor = GreenToRed(14.5f, 8f);
            spriteBatch.Draw(Utils.white, userRect[0], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[1], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[2], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[5], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[6], os.brightLockedColor);
            spriteBatch.Draw(Utils.white, userRect[7], os.brightLockedColor);


            spriteBatch.Draw(Utils.white, userRect[3], RectColor);
            spriteBatch.Draw(Utils.white, userRect[4], RectColor);

        }

        Rectangle attackArea = new Rectangle(drawArea.X, drawArea.Y, CentralRect.Left - gapToCentral, drawArea.Height);
        Vector2 attackPoint = new Vector2(CentralRect.Center.X, CentralRect.Top + CentralRect.Height / 2);

        int rows = 30;
        int cols = 4;
        int gap = 2;
        int totalGapWidth = (cols - 1) * gap;
        int totalGapHeight = (rows - 1) * gap;
        int cellWidth = (attackArea.Width - totalGapWidth) / cols;
        int cellHeight = (attackArea.Height - totalGapHeight) / rows;

        Rectangle[,] attackGridRects = new Rectangle[rows, cols];
        Vector2[,] attackStart = new Vector2[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int x = attackArea.X + col * (cellWidth + gap);
                int y = attackArea.Y + row * (cellHeight + gap);
                attackGridRects[row, col] = new Rectangle(x, y, cellWidth, cellHeight);
                attackStart[row, col] = new Vector2(attackGridRects[row, col].Center.X, attackGridRects[row, col].Center.Y);
            }
        }

        int centerRow = 15;

        // 将以下参数调整：
        float shootDuration = 0.2f; // 每次射击动画时长（加长）
        float shootInterval = 0.01f; // 每次射击间隔（缩短）
        float shootCycle = shootInterval; // 每个小矩形的完整射击周期

        int minExpandRow = centerRow - 1;
        int maxExpandRow = centerRow;

        if (lifetime < 2.0f)
        {
            float lerp = MathHelper.Clamp(lifetime / 2.0f, 0f, 1f);
            int minCol = 1 - (int)(lerp * 1.0f + 0.5f);
            int maxCol = 2 + (int)(lerp * 1.0f + 0.5f);
            minCol = Math.Max(0, minCol);
            maxCol = Math.Min(cols - 1, maxCol);
            for (int row = centerRow - 1; row <= centerRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    spriteBatch.Draw(Utils.white, attackGridRects[row, col], Color.Orange);

                    // 顺序射击：每个小矩形的射击时间依次递增
                    int rectIndex = (row - (centerRow - 1)) * (maxCol - minCol + 1) + (col - minCol);
                    float rectStartTime = rectIndex * shootCycle;
                    float rectElapsed = lifetime - rectStartTime;
                    bool isShooting = rectElapsed >= 0 && rectElapsed < shootDuration && lifetime < 22.5f;
                    float shootLerp = isShooting ? (rectElapsed / shootDuration) : 1f;

                    if (isShooting)
                    {
                        Vector2 start = attackStart[row, col];
                        Vector2 end = Vector2.Lerp(start, attackPoint, shootLerp);
                        Utils.drawLine(GuiData.spriteBatch, start, end, new Vector2(0, 0), os.lockedColor, 0f);
                    }
                }
            }
        }
        else
        {
            if (lifetime >= 2.0f && lifetime < 8.0f)
            {
                shootDuration = 0.2f; 
                shootInterval = 0.01f; 
                int expanded = (int)((lifetime - 2.0f) / 1.5f);
                minExpandRow = Math.Max(0, (centerRow - 1) - expanded);
                maxExpandRow = Math.Min(rows - 1, centerRow + expanded);
            }
            else if (lifetime >= 8.0f && lifetime < 12.0f)
            {
                shootDuration = 0.3f;
                shootInterval = 0.005f;
                int expanded = (int)((8.0f - 2.0f) / 1.5f);
                int extra = (int)((lifetime - 8.0f) / 1.0f);
                int totalExpand = expanded + extra;
                minExpandRow = Math.Max(0, (centerRow - 1) - totalExpand);
                maxExpandRow = Math.Min(rows - 1, centerRow + totalExpand);
            }
            else if (lifetime >= 12.0f)
            {
                shootDuration = 0.4f;
                shootInterval = 0.001f;
                int expanded = (int)((8.0f - 2.0f) / 1.5f);
                int extra = (int)((12.0f - 8.0f) / 1.0f);
                int more = (int)((lifetime - 12.0f) / 0.5f);
                int totalExpand = expanded + extra + more;
                minExpandRow = Math.Max(0, (centerRow - 1) - totalExpand);
                maxExpandRow = Math.Min(rows - 1, centerRow + totalExpand);
            }
            if (lifetime >= 22.5f)
            {
                for (int row = minExpandRow; row <= maxExpandRow; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        spriteBatch.Draw(Utils.white, attackGridRects[row, col], Color.Orange);
                        Vector2 start = attackStart[row, col];
                        Utils.drawLine(GuiData.spriteBatch, start, attackPoint, new Vector2(0, 0), os.lockedColor, 0f);
                    }
                }
            }

            int rectCount = (maxExpandRow - minExpandRow + 1) * cols;
            int rectIdx = 0;
            float totalCycle = rectCount * shootCycle;
            for (int row = minExpandRow; row <= maxExpandRow; row++)
            {
                for (int col = 0; col < cols; col++, rectIdx++)
                {
                    spriteBatch.Draw(Utils.white, attackGridRects[row, col], Color.Orange);

                    // 循环射击：每个小矩形的射击时间在总周期内循环
                    float rectStartTime = rectIdx * shootCycle;
                    float timeInCycle = (lifetime - rectStartTime) % totalCycle;
                    if (timeInCycle < 0) timeInCycle += totalCycle;
                    bool isShooting = timeInCycle >= 0 && timeInCycle < shootDuration && lifetime < 22.5f;
                    float shootLerp = isShooting ? (timeInCycle / shootDuration) : 1f;

                    if (isShooting)
                    {
                        Vector2 start = attackStart[row, col];
                        Vector2 end = Vector2.Lerp(start, attackPoint, shootLerp);
                        Utils.drawLine(GuiData.spriteBatch, start, end, new Vector2(0, 0), os.lockedColor, 0f);
                    }
                }
            }
        }

    }

    public Color GreenToRed(float startTime, float totalDuration)
    {
        float t = MathHelper.Clamp(lifetime - startTime, 0f, totalDuration);
        float halfDuration = totalDuration / 2f;

        if (t <= halfDuration)
        {

             float lerp = t / halfDuration;
             return Color.Lerp(os.brightUnlockedColor, Color.Gold, lerp);
        }
        else
        { 
             float lerp = (t - halfDuration) / halfDuration;
             return Color.Lerp(Color.Gold, os.brightLockedColor, lerp);
        }
    }







    public override void Update(float t)
    {
        Computer comp = Programs.getComputer(os, targetIP);
        lifetime += t;
        if (lifetime >= 23f && !isExiting)
        {
            Completed();
            comp.openPort(TRANSPort, os.thisComputer.ip);
            isExiting = true;
        }
        base.Update(t);
    }


}
