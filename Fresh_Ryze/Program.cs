using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace FreshRyze
{
    class Program
    {
        public const string ChampName = "Ryze";
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static void Main(string[] args)
        {
            Q = new Spell(SpellSlot.Q, 900);
            Q.SetSkillshot(0.25f, 50f, 1700, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E,600);
            R = new Spell(SpellSlot.R);

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != ChampName)
            {
                Game.PrintChat(Player.ChampionName + ": No Support Champion");
                return;
            }

            MainMenu.Initialize();

            Drawing.OnDraw += OnDraw.Drawing_OnDraw;
            Game.OnUpdate += OnUpdate.OnGameUpdate;
        }
    }
}