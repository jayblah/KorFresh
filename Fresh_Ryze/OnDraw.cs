using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace FreshRyze
{
    public static class OnDraw
    {
        public static void Drawing_OnDraw(EventArgs args)
        {
            if (MainMenu._MainMenu.Item("QRange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Program.Player.Position, Program.Q.Range, System.Drawing.Color.White, 2);
            }
            if (MainMenu._MainMenu.Item("WRange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Program.Player.Position, Program.W.Range, System.Drawing.Color.White, 2);
            }
            if (MainMenu._MainMenu.Item("ERange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Program.Player.Position, Program.E.Range, System.Drawing.Color.White, 2);
            }
        }
    }
}
