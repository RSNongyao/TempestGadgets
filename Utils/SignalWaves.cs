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
            // ��ȡ��������߽�
            int contentTop = contentArea.Y;
            int contentHeight = contentArea.Height;
            int contentWidth = contentArea.Width;

            // ȷ������������Ч
            if (contentHeight <= 0 || contentWidth <= 0) return;

            // ���㲨������λ��
            float centerY = contentTop + contentHeight / 2;
            float maxWaveHeight = Math.Min(contentHeight * 0.6f, 90); // ������Ӹ߶�

            // �������
            float segmentWidth = (float)contentWidth / (WAVE_POINTS - 1);

            // ������ɫ - ����ɫ����͸����
            Color waveColor = Color.White;

            // �����������ڻ��Ʋ��ε�
            Texture2D pointTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pointTexture.SetData(new[] { Color.White });

            // ���Ʋ��ε㣨ʹ��ʵ�ľ��δ���������
            for (int i = 0; i < WAVE_POINTS; i++)
            {
                float x = contentArea.X + i * segmentWidth;
                float y = centerY + waveHeights[i] * maxWaveHeight;
                y = MathHelper.Clamp(y, contentTop, contentTop + contentHeight);

                // ���ƴ�ߴ粨�ε�
                Rectangle pointRect = new Rectangle(
                    (int)x - 2, // ���ĵ�����ƫ��
                    (int)y - 2, // ���ĵ�����ƫ��
                    2, // ���
                    2  // �߶�
                );

                spriteBatch.Draw(pointTexture, pointRect, waveColor);
            }

            // ���Ӳ��ε㣨ʹ�ô��ߣ�
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

                // ���Ʋ����߶� - ʹ�÷ǳ��ֵ���
                Hacknet.Utils.drawLine(
                    spriteBatch,
                    prevPoint,
                    currentPoint,
                    Vector2.Zero,
                    waveColor,
                    3.0f // �ǳ��ֵ���
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