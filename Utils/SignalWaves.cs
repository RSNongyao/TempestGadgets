using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace TempestGadgets.Utils
{
    public class SignalWaves
    {
        private static SpriteBatch spriteBatch => GuiData.spriteBatch;
        private const int WAVE_POINTS = 3000;

        public static void DrawSignalAnimation(Rectangle contentArea, float[] waveHeights)
        {
            // 获取内容区域边界
            int contentTop = contentArea.Y;
            int contentHeight = contentArea.Height;
            int contentWidth = contentArea.Width;

            // 确保内容区域有效
            if (contentHeight <= 0 || contentWidth <= 0) return;

            // 计算波形中心位置
            float centerY = contentTop + contentHeight / 2;
            float maxWaveHeight = Math.Min(contentHeight * 0.6f, 90); // 大幅增加高度

            // 计算点间距
            float segmentWidth = (float)contentWidth / (WAVE_POINTS - 1);

            // 波形颜色 - 纯白色（不透明）
            Color waveColor = Color.White;

            // 创建纹理用于绘制波形点
            Texture2D pointTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pointTexture.SetData(new[] { Color.White });

            // 绘制波形点（使用实心矩形代替线条）
            for (int i = 0; i < WAVE_POINTS; i++)
            {
                float x = contentArea.X + i * segmentWidth;
                float y = centerY + waveHeights[i] * maxWaveHeight;
                y = MathHelper.Clamp(y, contentTop, contentTop + contentHeight);

                // 绘制大尺寸波形点
                Rectangle pointRect = new Rectangle(
                    (int)x - 2, // 中心点向左偏移
                    (int)y - 2, // 中心点向上偏移
                    2, // 宽度
                    2  // 高度
                );

                spriteBatch.Draw(pointTexture, pointRect, waveColor);
            }

            // 连接波形点（使用粗线）
            Vector2 prevPoint = new Vector2(
                contentArea.X,
                MathHelper.Clamp(centerY + waveHeights[0] * maxWaveHeight, contentTop, contentTop + contentHeight)
            );

            for (int i = 1; i < WAVE_POINTS; i++)
            {
                float x = contentArea.X + i * segmentWidth;
                float y = centerY + waveHeights[i] * maxWaveHeight;
                y = MathHelper.Clamp(y, contentTop, contentTop + contentHeight);

                Vector2 currentPoint = new Vector2(x, y);

                // 绘制波形线段 - 使用非常粗的线
                Hacknet.Utils.drawLine(
                    spriteBatch,
                    prevPoint,
                    currentPoint,
                    Vector2.Zero,
                    waveColor,
                    3.0f // 非常粗的线
                );

                prevPoint = currentPoint;
            }
        }

        public static void InitializeSignalWave(float[] waveHeights)
        {
            Random rand = new Random();
            for (int i = 0; i < WAVE_POINTS; i++)
            {
                waveHeights[i] = (float)(rand.NextDouble() * 2 - 1);
            }
        }






    }
}