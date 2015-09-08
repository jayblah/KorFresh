using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Zilean// By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 550f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 900f);

            E.SetTargetted(0.25f, float.MaxValue);
            Q.SetSkillshot(1.0f, 90f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, float.MaxValue);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addAutoHarass();
            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana(40);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseE();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q);
                if(!Q.IsReady())
                AIO_Func.SC(W);
                AIO_Func.SC(E);
            }
            
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee && AIO_Menu.Champion.Flee.UseE)
            {
                var Etarget = TargetSelector.GetTarget(1000f, Q.DamageType, true); 
                if(Etarget != null && E.IsReady())
                E.Cast(Player);
            }

            AutoR();
        }
        
        static void AutoR()
        {
            R.Shield(20);
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }


        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (W.IsReady())
                damage += Q.GetDamage2(enemy);
                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);

                
            return damage;
        }
    }
}
