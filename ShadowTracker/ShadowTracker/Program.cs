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
        private static Menu _MainMenu;
        private static Obj_AI_Hero Player;        
        static Vector3 Enemy_Flash_Start, Enemy_Flash_End, Enemy_Ezreal_Start, Enemy_Ezreal_End;
        static float Enemy_Flash_Time, Enemy_Ezreal_Time;
        static int Spell_Flash = 450, EzrealArcaneShift = 475;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;            
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
                //var message = "\n" + "Name: " + sender.BaseSkinName + "\n" + "SpellName: " + args.SData.Name + "\n" + "Start: " + args.Start + "\n" + "End: " + args.End;
                //Console.Write(message);
            
            
            foreach (var enemy in HeroManager.Enemies)
            {                
                if (enemy.BaseSkinName == sender.BaseSkinName && _MainMenu.Item(enemy.BaseSkinName).GetValue<bool>())   // 스펠사용자가 true인 적과 같은지 확인
                {
                    if(args.SData.Name == "summonerflash")      // 스펠이 점멸이면.
                    {
                        Enemy_Flash_Start = args.Start;
                        Enemy_Flash_End = args.End;
                        Enemy_Flash_Time = Game.ClockTime + 2;
                    }

                    if(args.SData.Name == "EzrealArcaneShift")
                    {
                        Enemy_Ezreal_Start = args.Start;
                        Enemy_Ezreal_End = args.End;
                        Enemy_Ezreal_Time = Game.ClockTime + 2;
                    }
                }
            }
            //if (sender.IsMe){Enemy_Flash_Start = args.Start;Enemy_Flash_End = args.End;Enemy_Flash_Time = Game.ClockTime + 3;} //Test IsMe
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _MainMenu = new Menu("ShadowTracker", "ShadowTracker", true);
            var Draw = new Menu("Draw", "Draw");
            {
                Draw.AddItem(new MenuItem("Deburg", "Deburg").SetValue(true));
            }
            _MainMenu.AddSubMenu(Draw);

            var Enemy = new Menu("Enemy", "Enemy");
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    Enemy.AddItem(new MenuItem(enemy.BaseSkinName.ToString(), enemy.BaseSkinName.ToString()).SetValue(true));
                }
            }
            _MainMenu.AddSubMenu(Enemy);

            _MainMenu.AddToMainMenu();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Enemy_Flash_Time > Game.ClockTime)  // 점멸 도착 위치 표시
            {
                //Console.Write("\n"+Enemy_Flash_Start.Distance(Enemy_Flash_End));
                if (Enemy_Flash_Start.Distance(Enemy_Flash_End) > Spell_Flash)
                {                    
                    var dis = Enemy_Flash_Start.Distance(Enemy_Flash_End) - Spell_Flash;
                    Enemy_Flash_End = Enemy_Flash_End.Extend(Enemy_Flash_Start, +dis);
                }
                Render.Circle.DrawCircle(Enemy_Flash_Start, 50, System.Drawing.Color.PaleVioletRed, 1);
                Render.Circle.DrawCircle(Enemy_Flash_End, 50, System.Drawing.Color.LawnGreen, 1);                
                var from = Drawing.WorldToScreen(Enemy_Flash_Start);
                var to = Drawing.WorldToScreen(Enemy_Flash_End);
                Drawing.DrawLine(from[0], from[1], to[0], to[1], 1, System.Drawing.Color.LawnGreen);
                Drawing.DrawText(from[0], from[1], System.Drawing.Color.PaleVioletRed, "Start");
                Drawing.DrawText(to[0], to[1], System.Drawing.Color.LawnGreen, "End");
            }

            if (Enemy_Ezreal_Time > Game.ClockTime) // 이즈리얼 비전이동 표시
            {
                if (Enemy_Ezreal_Start.Distance(Enemy_Ezreal_End) > EzrealArcaneShift)
                {
                    var dis = Enemy_Ezreal_Start.Distance(Enemy_Ezreal_End) - EzrealArcaneShift;
                    Enemy_Ezreal_End = Enemy_Ezreal_End.Extend(Enemy_Ezreal_Start, +dis);
                }
                Render.Circle.DrawCircle(Enemy_Ezreal_Start, 50, System.Drawing.Color.PaleVioletRed, 1);
                Render.Circle.DrawCircle(Enemy_Ezreal_End, 50, System.Drawing.Color.LawnGreen, 1);
                var from = Drawing.WorldToScreen(Enemy_Ezreal_Start);
                var to = Drawing.WorldToScreen(Enemy_Ezreal_End);
                Drawing.DrawLine(from[0], from[1], to[0], to[1], 1, System.Drawing.Color.LawnGreen);
                Drawing.DrawText(from[0], from[1], System.Drawing.Color.PaleVioletRed, "Start");
                Drawing.DrawText(to[0], to[1], System.Drawing.Color.LawnGreen, "End");
            }

            // 샤코 디코이
            // 르블랑 분신, 샤코 분신
            // 렝가 은신, 트위치 은신, 아칼리장막            
            // 핑 여부(렉사이궁, 쉔궁)
            
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            
        }        
    }
}
