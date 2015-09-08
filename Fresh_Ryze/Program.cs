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
        private static Obj_AI_Hero Player;

        static void Main(string[] args)
        {            
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if(Player.BaseSkinName != ChampName)
            {
                Game.PrintChat("No Support Champion");
                return;
            }

            new MainMenu();
        }        
    }
}