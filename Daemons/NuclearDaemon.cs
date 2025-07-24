using Hacknet;

using Pathfinder.Daemon;
using Pathfinder.Util;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TempestGadgets.Daemons
{
    public class NuclearDaemon : BaseDaemon
    {
        public NuclearDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem) { }

        public override string Identifier => "Nuclear Daemon";

        [XMLStorage]
        public string DisplayString;

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);

            var center = os.display.bounds.Center;
            TextItem.doLabel(new Vector2(center.X, center.Y), DisplayString, Color.Aquamarine);

            // »­Ô²
            int radius = 50;
            int segments = 100;
            Texture2D pixel = new Texture2D(sb.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            float angleStep = (float)(Math.PI * 2 / segments);
            Vector2 prevPoint = new Vector2(center.X + radius, center.Y);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep;
                Vector2 newPoint = new Vector2(
                    center.X + (float)Math.Cos(angle) * radius,
                    center.Y + (float)Math.Sin(angle) * radius
                );
                sb.Draw(pixel, new Rectangle((int)prevPoint.X, (int)prevPoint.Y, (int)(newPoint - prevPoint).Length(), 1),
                    null, Color.Aquamarine, (float)Math.Atan2(newPoint.Y - prevPoint.Y, newPoint.X - prevPoint.X),
                    Vector2.Zero, SpriteEffects.None, 0);
                prevPoint = newPoint;
            }
        }
    }













}