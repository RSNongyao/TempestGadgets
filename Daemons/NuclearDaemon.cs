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
        }
    }













}