using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Malzahar// By RL244 
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static bool RM {get{return Menu.Item("Combo.Use MR").GetValue<KeyBind>().Active; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 800f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 650f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 700f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(1f, 85f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetTargetted(0.25f, float.MaxValue);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.Use MR", "Use R(Manual)")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (target.IsValidTarget(R.Range) && AIO_Func.isKillable(target,(E.IsReady() ? E.GetDamage2(target) : 0f) + R.GetDamage2(target)))
                {
                    AIO_Func.SC(R);
                }
            }
            
            if (Orbwalking.CanMove(35))
            {

                AIO_Func.SC(Q);
                AIO_Func.SC(W);
                AIO_Func.SC(E);
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
                R.Cast(RTarget);
            }
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
                {
                    var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                    if(Qtarget != null && AIO_Func.isKillable(Qtarget, Q.GetDamage2(Qtarget) + (AIO_Menu.Champion.Combo.UseW ? W.GetDamage2(Qtarget)*4 : 0f) + (AIO_Menu.Champion.Combo.UseE ? E.GetDamage2(Qtarget) : 0f) + (AIO_Menu.Champion.Combo.UseR && R.IsReady() ? R.GetDamage2(Qtarget) : 0f) + (float)Player.GetAutoAttackDamage2(Qtarget, true)*3))
                    {
                        if(AIO_Menu.Champion.Combo.UseR && R.IsReady())
                        R.Cast(Qtarget);
                    }
                }
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.E).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
                {
                    var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                    if(Etarget != null && AIO_Func.isKillable(Etarget, E.GetDamage2(Etarget) + (AIO_Menu.Champion.Combo.UseW ? W.GetDamage2(Etarget)*4 : 0f) + (AIO_Menu.Champion.Combo.UseQ ? Q.GetDamage2(Etarget) : 0f) +(AIO_Menu.Champion.Combo.UseR && R.IsReady() ? R.GetDamage2(Etarget) : 0f) + (float)Player.GetAutoAttackDamage2(Etarget, true)*3))
                    {
                        if(AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                        Q.Cast(Etarget.ServerPosition);
                    }
                }
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.R).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
                {
                    Orbwalker.SetMovement(false);
                }
                else
                Orbwalker.SetMovement(true);
            }
        }
        
        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.End);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (Q.CanCast(sender))
                Q.Cast(sender);
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);
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
