using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShadowTracker.SpellTracker
{
    class Spell_Checker
    {
        public static Vector3 Enemy_Flash_Start, Enemy_Flash_End, Enemy_Ezreal_Start, Enemy_Ezreal_End, Enemy_Shaco_Start, Enemy_Shaco_End, Enemy_Monkey_Position;
        public static double Enemy_Flash_Time, Enemy_Ezreal_Time, Enemy_Shaco_Time, Enemy_Monkey_Time;
        public static bool Enemy_Monkey_Enable;

        public static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {            
            foreach (var enemy in HeroManager.Enemies)
            {
                if (enemy.ChampionName == sender.BaseSkinName)
                {
                    if(args.SData.Name == "summonerflash")  // 점멸
                    {
                        Enemy_Flash_Start = args.Start;
                        Enemy_Flash_End = args.End;
                        Enemy_Flash_Time = Game.ClockTime + 2;
                    }

                    if (args.SData.Name == "EzrealArcaneShift")  // 이즈비전
                    {                        
                        Enemy_Ezreal_Start = args.Start;
                        Enemy_Ezreal_End = args.End;
                        Enemy_Ezreal_Time = Game.ClockTime + 2;
                    }

                    if (args.SData.Name == "Deceive")  // 샤코 디시브
                    {
                        Enemy_Shaco_Start = args.Start;
                        Enemy_Shaco_End = args.End;
                        Enemy_Shaco_Time = Game.ClockTime + 2;
                    }

                    if (args.SData.Name == "MonkeyKingDecoy" && Enemy_Monkey_Enable == false)   // 오공 디코이 시전이면,
                    {
                        Enemy_Monkey_Enable = true;
                        Enemy_Monkey_Time = Game.ClockTime + 2;
                        Enemy_Monkey_Position = enemy.Position;
                        Game.ShowPing(PingCategory.Danger, Enemy_Monkey_Position);                        
                    }
                    else if (Enemy_Monkey_Enable == true && Enemy_Monkey_Time < Game.ClockTime)
                    {
                        Enemy_Monkey_Enable = false;
                        Enemy_Monkey_Position = Vector3.Zero;
                    }
                }
            }
        }
    }
}
