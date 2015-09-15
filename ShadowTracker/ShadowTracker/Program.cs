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
    class Program
    {        
        public static Obj_AI_Hero Player;

        public static bool LPet_Enable, SPet_Enable = false;
        public static float LPet_Time, SPet_Time;
        public static Obj_AI_Hero Enemy_Leblanc, Enemy_Shaco;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw.Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += ShadowTracker.SpellTracker.Spell_Checker.Obj_AI_Hero_OnProcessSpellCast;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            MainMenu.Initialize();            
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {            
            Enemy_Leblanc = HeroManager.Enemies.Find(x => x.ChampionName == "Leblanc");
            
            if (Enemy_Leblanc != null && Enemy_Leblanc.HealthPercent < 41 && Enemy_Leblanc.Pet != null && LPet_Enable != true && LPet_Time < Game.ClockTime && Enemy_Leblanc.Pet.Name == Enemy_Leblanc.Name)
            {
                LPet_Time = Game.ClockTime + 8;
                LPet_Enable = true;
                Console.Write("On");
            }
            if (LPet_Enable == true && LPet_Time < Game.ClockTime)
            {
                LPet_Enable = false;
                LPet_Time = Game.ClockTime + 30;
                Console.Write("Off");
            }            

            Enemy_Shaco = HeroManager.Enemies.Find(x => x.ChampionName == "Shaco");
            if (SPet_Enable == false && Enemy_Shaco != null && Enemy_Shaco.Pet != null && Enemy_Shaco.Name == Enemy_Shaco.Pet.Name && SPet_Time < Game.ClockTime)
            {
                SPet_Time = Game.ClockTime + 18;
                SPet_Enable = true;                
            }
            if (SPet_Enable == true && SPet_Time < Game.ClockTime)
            {
                SPet_Enable = false;
                SPet_Time = Game.ClockTime + 18;
            }
        }
        // 모데카이져 분신, 요릭 분신
        // 아칼리 장막 카운터
        // 렝가, 트위치 분신 엔드타임
    }
}
