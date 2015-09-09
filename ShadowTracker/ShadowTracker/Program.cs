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
        static float EndTime;
        static Vector3 Enemy_Flash_Start, Enemy_Flash_End;
        static float Enemy_Flash_Time;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;            
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //message = "\n" + "Name: " + sender.BaseSkinName + "\n" + "SpellName: " + args.SData.Name + "\n"+"Start: " + args.Start + "\n"+ "End: " + args.End;                
            //EndTime = Game.ClockTime + 3;                
            foreach (var enemy in HeroManager.Enemies)
            {                
                if (enemy.BaseSkinName == sender.BaseSkinName && _MainMenu.Item(enemy.BaseSkinName).GetValue<bool>())   // 스펠사용자가 true인 적과 같은지 확인
                {
                    if(args.SData.Name == "summonerflash")      // 스펠이 점멸이면.
                    {
                        Enemy_Flash_Start = args.Start;
                        Enemy_Flash_End = args.End;
                        Enemy_Flash_Time = Game.ClockTime + 3;
                    }
                }
            }
            if (sender.IsMe)
            {
                Enemy_Flash_Start = args.Start;
                Enemy_Flash_End = args.End;
                Enemy_Flash_Time = Game.ClockTime + 3;
            }
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
                Console.Write("\n"+Enemy_Flash_Start.Distance(Enemy_Flash_End));
                if (Enemy_Flash_Start.Distance(Enemy_Flash_End) > 450)
                {                    
                    var dis = Enemy_Flash_Start.Distance(Enemy_Flash_End) - 450;
                    Enemy_Flash_End.Extend(Enemy_Flash_Start, -dis);
                }
                Render.Circle.DrawCircle(Enemy_Flash_Start, 50, System.Drawing.Color.HotPink, 1);
                Render.Circle.DrawCircle(Enemy_Flash_End, 50, System.Drawing.Color.Pink, 2);
                var from = Drawing.WorldToScreen(Enemy_Flash_Start);
                var to = Drawing.WorldToScreen(Enemy_Flash_End);
                Drawing.DrawLine(from[0], from[1], to[0], to[1], 3, System.Drawing.Color.White);
            }
            //Render.Circle.DrawCircle(SpellEnd, 50, System.Drawing.Color.HotPink,2);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            
        }        
    }
}
