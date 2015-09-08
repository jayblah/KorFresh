using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Varus// By RL244 
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float WM {get{return Menu.Item("Combo.WM").GetValue<Slider>().Value; }}
        static bool RM {get{return Menu.Item("Combo.Use MR").GetValue<KeyBind>().Active; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 925f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1200f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(0.25f, 70f, 1650f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.50f, 250f, 1400f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 120f, 1950f, false, SkillshotType.SkillshotLine);

            Q.SetCharged("VarusQ", "VarusQ", 250, 1600, 1.2f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.WM", "W Min Stacks")).SetValue(new Slider(2, 1, 3));
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR(false);
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.Use MR", "Use R(Manual)")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && (float)AIO_Func.getBuffInstance(target, "varuswdebuff").Count >= WM)
                    {
                        AIO_Func.SC(Q);
                        AIO_Func.SC(E);
                    }
                    else if(!target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                    {
                        AIO_Func.SC(Q);
                        AIO_Func.SC(E);
                    }
                    if(Q.IsCharging) //차징일 경우 위와 상관없이 되도
                    AIO_Func.SC(Q);
                }
                AIO_Func.SC(R);
            }
            
            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            #endregion
            ManualR();
        }
        static void ManualR()
        {
            var RTarget = TargetSelector.GetTarget(R.Range, R.DamageType, true);
            if(RM && RTarget != null && R.IsReady())
            {
                AIO_Func.LCast(R,RTarget);
            }
        }
        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (R.CanCast(gapcloser.Sender))
                R.Cast(gapcloser.End);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender))
                R.Cast(sender);
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.CCast(target);
            }
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                
            if (E.IsReady())
                damage += E.GetDamage2(enemy);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
