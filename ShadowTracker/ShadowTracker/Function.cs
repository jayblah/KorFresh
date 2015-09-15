using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShadowTracker
{
    class Function
    {
        public static void Move_Draw(Vector3 Spell_Start, Vector3 Spell_End, double Spell_Time, int Distance, int Type)
        {            
            if (Spell_Start.Distance(Spell_End) > Distance){var dis = Spell_Start.Distance(Spell_End) - Distance;Spell_End = Spell_End.Extend(Spell_Start, +dis);}
            Render.Circle.DrawCircle(Spell_Start, 50, System.Drawing.Color.PaleVioletRed, 1);
            Render.Circle.DrawCircle(Spell_End, 50, System.Drawing.Color.LawnGreen, 1);
            var from = Drawing.WorldToScreen(Spell_Start);var to = Drawing.WorldToScreen(Spell_End);
            Drawing.DrawLine(from[0], from[1], to[0], to[1], 1, System.Drawing.Color.LawnGreen);
            Drawing.DrawText(from[0], from[1], System.Drawing.Color.PaleVioletRed, "Start");
            Drawing.DrawText(to[0], to[1], System.Drawing.Color.PaleVioletRed, "End");
        }

        public static void Shadow_Draw(Obj_AI_Hero Position)
        {
            Render.Circle.DrawCircle(Position.Position, 75, System.Drawing.Color.Red, 5);
            var Body = Drawing.WorldToScreen(Position.Position);
            Drawing.DrawText(Body[0] - 15, Body[1] - 70, System.Drawing.Color.Red, "This !");

            Render.Circle.DrawCircle(Position.Pet.Position, 75, System.Drawing.Color.LightGreen, 5);
            var Body1 = Drawing.WorldToScreen(Position.Pet.Position);
            Drawing.DrawText(Body1[0] - 15, Body1[1] - 70, System.Drawing.Color.LightGreen, "No");
        }
        
        public static void No_Target(Vector3 Position)
        {
            Render.Circle.DrawCircle(Position, 70, System.Drawing.Color.Red, 5);
            var monkey = Drawing.WorldToScreen(Position);
            Drawing.DrawText(monkey[0] - 25, monkey[1] - 60, System.Drawing.Color.Red, "No !");
        }
    }
}
