using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Alistar// By RL244 
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float EM {get{return Menu.Item("Misc.EM").GetValue<Slider>().Value; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 365f, TargetSelector.DamageType.Magical){Delay = 0.25f};
            W = new Spell(SpellSlot.W, 650f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 575f);
            R = new Spell(SpellSlot.R);

            W.SetTargetted(0.25f, 1800f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseR(false);
            
            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ(false);
            AIO_Menu.Champion.Jungleclear.addUseW(false);
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("Auto E", true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.EM", "W If Mana >")).SetValue(new Slider(40, 0, 100));
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();

        
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

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q);
                AIO_Func.SC(W);
                AIO_Func.SC(R);
            }
            
            if (AIO_Menu.Champion.Misc.getBoolValue("Auto E"))
                AIO_Func.Heal(E,EM);
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseQ || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseQ))
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.W).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range + 300)))
                {
                    var Qtarget = TargetSelector.GetTarget(Q.Range + 300, Q.DamageType);
                    if(Qtarget != null)
                    Q.AOECast(Qtarget);
                }
            }
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;
            if (Q.CanCast(gapcloser.Sender))
                Q.AOECast(gapcloser.Sender);
            else if (W.CanCast(gapcloser.Sender))
                W.Cast(gapcloser.Sender);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;
            if (Q.CanCast(sender))
                Q.AOECast(sender);
            else if (W.CanCast(sender))
                W.Cast(sender);
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
