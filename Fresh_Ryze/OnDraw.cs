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

            if (MainMenu._MainMenu.Item("DisplayStack").GetValue<bool>())
            {
                if (OnUpdate.Ryzepassivecharged != null)
                {
                    var RyzePosition = Drawing.WorldToScreen(Program.Player.Position);
                    var end = OnUpdate.Ryzepassivecharged.EndTime - Game.ClockTime;
                    var DisColor = end < 2 ? System.Drawing.Color.Red : System.Drawing.Color.White;
                    Drawing.DrawText(RyzePosition[0]-30, RyzePosition[1]+15, DisColor, "Charge End: " + end);
                }
                if (OnUpdate.RyzePassive.Count != null)
                {
                    var RyzePosition = Drawing.WorldToScreen(Program.Player.Position);
                    Drawing.DrawText(RyzePosition[0] - 30, RyzePosition[1]+15, System.Drawing.Color.White, "Stack: " + OnUpdate.RyzePassive.Count);
                }                
            }            
            if (MainMenu._MainMenu.Item("DisplayTime").GetValue<bool>() && OnUpdate.RyzePassive.Count != null)
            {                
                var RyzeEndTime = Drawing.WorldToScreen(Program.Player.Position);
                var PassiveEnd = OnUpdate.RyzePassive.EndTime - Game.ClockTime;
                var DisColor = PassiveEnd < 2 ? System.Drawing.Color.Red : System.Drawing.Color.White;
                Drawing.DrawText(RyzeEndTime[0] - 30, RyzeEndTime[1] + 30, DisColor, "Passive End: " + PassiveEnd);
            }
        }
    }
}
