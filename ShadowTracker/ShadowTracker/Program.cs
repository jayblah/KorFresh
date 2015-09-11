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
        static Vector3 Enemy_Flash_Start, Enemy_Flash_End, Enemy_Ezreal_Start, Enemy_Ezreal_End, Enemy_Leblanc_Position, Enemy_Shaco_Start, Enemy_Shaco_End, Enemy_Shaco_Position, Enemy_Monkey_Position;
        static float Enemy_Flash_Time, Enemy_Ezreal_Time, Enemy_Leblanc_Time = 0, Enemy_Leblanc_PTime, Enemy_Shaco_Time, Enemy_Shaco_PTime, Enemy_Monkey_Time;
        static int Spell_Flash = 450, EzrealArcaneShift = 475, Shaco_Q = 400;
        static bool Ping_Enable, Enemy_Monkey_Enable = false;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;            
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;            
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //if (sender.IsMe)
            //{
                //var message = "\n" + "Name: " + sender.BaseSkinName + "\n" + "SpellName: " + args.SData.Name + "\n" + "Start: " + args.Start + "\n" + "End: " + args.End;
                //Console.Write(message);
            //}
            
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

                    if(args.SData.Name == "EzrealArcaneShift")  // 이즈 비전이면.
                    {
                        Enemy_Ezreal_Start = args.Start;
                        Enemy_Ezreal_End = args.End;
                        Enemy_Ezreal_Time = Game.ClockTime + 2;
                    }

                    if (args.SData.Name == "Deceive")  // 샤코 디시브이면,
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
                        Game.ShowPing(PingCategory.Danger, enemy.Position);
                    } else if(Enemy_Monkey_Enable == true && Enemy_Monkey_Time < Game.ClockTime)
                    {
                        Enemy_Monkey_Enable = false;
                        Enemy_Monkey_Position = Vector3.Zero;
                    }
                }
            }
            //if (sender.IsMe){Enemy_Flash_Start = args.Start;Enemy_Flash_End = args.End;Enemy_Flash_Time = Game.ClockTime + 3;} //Test IsMe
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            
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

            if (Enemy_Shaco_Time > Game.ClockTime) // 샤코 디시브 표시
            {                
                if (Enemy_Shaco_Start.Distance(Enemy_Shaco_End) > Shaco_Q)
                {
                    var dis = Enemy_Shaco_Start.Distance(Enemy_Shaco_End) - Shaco_Q;
                    Enemy_Shaco_End = Enemy_Shaco_End.Extend(Enemy_Shaco_Start, +dis);
                }
                Render.Circle.DrawCircle(Enemy_Shaco_Start, 50, System.Drawing.Color.PaleVioletRed, 1);
                Render.Circle.DrawCircle(Enemy_Shaco_End, 50, System.Drawing.Color.LawnGreen, 1);
                var from = Drawing.WorldToScreen(Enemy_Shaco_Start);
                var to = Drawing.WorldToScreen(Enemy_Shaco_End);
                Drawing.DrawLine(from[0], from[1], to[0], to[1], 1, System.Drawing.Color.LawnGreen);
                Drawing.DrawText(from[0], from[1], System.Drawing.Color.PaleVioletRed, "Start");
                Drawing.DrawText(to[0], to[1], System.Drawing.Color.LawnGreen, "End");
            }

            if (Enemy_Leblanc_Time > Game.ClockTime)
            {
                Render.Circle.DrawCircle(Enemy_Leblanc_Position, 50, System.Drawing.Color.Green, 4);
            }

            if (Enemy_Shaco_Time > Game.ClockTime)
            {
                Render.Circle.DrawCircle(Enemy_Shaco_Position, 50, System.Drawing.Color.Green, 4);
            }

            if (Enemy_Monkey_Time > Game.ClockTime && Enemy_Monkey_Enable == true)
            {
                Render.Circle.DrawCircle(Enemy_Monkey_Position, 100, System.Drawing.Color.Green, 4);
                var to = Drawing.WorldToScreen(Enemy_Monkey_Position);
                Drawing.DrawText(to[0], to[1], System.Drawing.Color.Red, "No Body");
            }

            // 오공 디코이
            // 핑 여부(렉사이궁, 쉔궁)
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var Enemy_Leblanc = HeroManager.Enemies.Find(x => x.ChampionName == "Leblanc");   // 르블랑 패시브 시작
            if (Enemy_Leblanc != null && Enemy_Leblanc.Pet != null)
            {
                Enemy_Leblanc_Position = Enemy_Leblanc.Position;
                if (Enemy_Leblanc_Time+8 < Game.ClockTime)
                {
                    Enemy_Leblanc_Time = Game.ClockTime + 8;
                    Ping_Enable = true;
                    Enemy_Leblanc_PTime = Game.ClockTime + 2;
                }
                if(Ping_Enable == true && Enemy_Leblanc_PTime < Game.ClockTime)
                {
                    Ping_Enable = false;
                    Game.ShowPing(PingCategory.Normal, Enemy_Leblanc);
                }
                
            }
            if(Enemy_Leblanc_Time < Game.ClockTime)
            {
                Enemy_Leblanc_Position = Vector3.Zero;
            }       // 르블랑 패시브 끝

            var Enemy_Shaco = HeroManager.Enemies.Find(x => x.ChampionName == "Shaco");   // 샤코 궁 시작
            if (Enemy_Shaco != null && Enemy_Shaco.Pet != null)
            {
                Enemy_Shaco_Position = Enemy_Shaco.Position;
                if (Enemy_Shaco_Time + 8 < Game.ClockTime)
                {
                    Enemy_Shaco_Time = Game.ClockTime + 18;
                    Ping_Enable = true;
                    Enemy_Shaco_PTime = Game.ClockTime + 2;
                }
                if (Ping_Enable == true && Enemy_Shaco_PTime < Game.ClockTime)
                {
                    Ping_Enable = false;
                    Game.ShowPing(PingCategory.Normal, Enemy_Shaco);
                }

            }
            if (Enemy_Shaco_Time < Game.ClockTime)
            {
                Enemy_Shaco_Position = Vector3.Zero;
            }       // 샤코 궁 끝            
        }        
    }
}
