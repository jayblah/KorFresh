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
    class OnDraw
    {
        public static void Drawing_OnDraw(EventArgs args)
        {            
            if (MainMenu._MainMenu.Item("Spell").GetValue<bool>() && ShadowTracker.SpellTracker.Spell_Checker.Enemy_Flash_Time > Game.ClockTime) // 점멸
            {
                Function.Move_Draw(ShadowTracker.SpellTracker.Spell_Checker.Enemy_Flash_Start, ShadowTracker.SpellTracker.Spell_Checker.Enemy_Flash_End, 
                    ShadowTracker.SpellTracker.Spell_Checker.Enemy_Flash_Time, 450, 3);
            }

            if (MainMenu._MainMenu.Item("Skill").GetValue<bool>() && ShadowTracker.SpellTracker.Spell_Checker.Enemy_Ezreal_Time > Game.ClockTime) // 이즈 비전
            {                
                Function.Move_Draw(ShadowTracker.SpellTracker.Spell_Checker.Enemy_Ezreal_Start, ShadowTracker.SpellTracker.Spell_Checker.Enemy_Ezreal_End,
                    ShadowTracker.SpellTracker.Spell_Checker.Enemy_Ezreal_Time, 475, 3);
            }

            if (MainMenu._MainMenu.Item("Skill").GetValue<bool>() && ShadowTracker.SpellTracker.Spell_Checker.Enemy_Shaco_Time > Game.ClockTime) // 샤코 디시브
            {
                Function.Move_Draw(ShadowTracker.SpellTracker.Spell_Checker.Enemy_Shaco_Start, ShadowTracker.SpellTracker.Spell_Checker.Enemy_Shaco_End,
                    ShadowTracker.SpellTracker.Spell_Checker.Enemy_Shaco_Time, 400, 3);
            }

            if (MainMenu._MainMenu.Item("Skill").GetValue<bool>() && Program.LPet_Time > Game.ClockTime && Program.LPet_Enable == true)  // 르블랑 패시브
            {
                Function.Shadow_Draw(Program.Enemy_Leblanc);
            }

            if (MainMenu._MainMenu.Item("Skill").GetValue<bool>() && Program.SPet_Time > Game.ClockTime && Program.SPet_Enable == true)  // 샤코 궁
            {
                Function.Shadow_Draw(Program.Enemy_Shaco);
            }

            if (MainMenu._MainMenu.Item("Skill").GetValue<bool>() && ShadowTracker.SpellTracker.Spell_Checker.Enemy_Monkey_Enable == true)   // 오공 디코이
            {
                Function.No_Target(ShadowTracker.SpellTracker.Spell_Checker.Enemy_Monkey_Position);
            }
        }
    }
}
