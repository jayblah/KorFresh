using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Braum// By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 650f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 1200f, TargetSelector.DamageType.Magical);
            
            W.SetTargetted(0.25f, 1600f);
            Q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine); //
            R.SetSkillshot(0.25f, 115f, 1400f, false, SkillshotType.SkillshotLine); //
            
            AIO_Menu.Champion.Flee.addUseW();
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("R Cast", new KeyBind('T', KeyBindType.Press));


            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if(AIO_Menu.Champion.Combo.UseW)
                W.Shield();
                
            if(AIO_Menu.Champion.Combo.UseE)
                E.Shield();
                
            if(AIO_Menu.Champion.Combo.getKeyBindValue("R Cast").Active)
                AIO_Func.SC(R,0f);
                AIO_Func.SC(Q,0f,0f);

            

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender) && Q.IsReady())
                Q.LCast(gapcloser.Sender,0f,0f);
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                Q.LCast(target,0f,0f);
            }
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
