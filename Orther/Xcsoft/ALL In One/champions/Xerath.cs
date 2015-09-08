using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Xerath// By RL244 
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static bool RM {get{return Menu.Item("Combo.Use MR").GetValue<KeyBind>().Active; }}
        static float LastPingTime = 0;
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1550, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 1000, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 1100, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 675, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.Use MR", "Use R(Manual)")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana(20);

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana(20);

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("Ping Notify on R killable enemies (local/client side)", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
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
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
            R.Range = 2000 + R.Level * 1200;
            
            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q);
                AIO_Func.SC(W);
                AIO_Func.SC(E,0,0);
            }
            
            if(Player.HasBuff2("XerathLocusOfPower2", true) ||(Player.LastCastedSpellName() == "XerathLocusOfPower2" && Utils.GameTimeTickCount - Player.LastCastedSpellT() < 500))
            AIO_Func.SC(R);

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            #endregion
            ManualR();
            
            #region Ping Notify on R killable enemies
            if (R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Ping Notify on R killable enemies (local/client side)"))
            {
                if (LastPingTime + 333 < Utils.GameTimeTickCount) //궁 2방으로 잡을수 있는 적 핑찍기.
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && AIO_Func.isKillable(x, R.GetDamage2(x)*2)))
                        Game.ShowPing(PingCategory.Normal, target.Position, true);

                    LastPingTime = Utils.GameTimeTickCount;
                }
            } 
            #endregion
        }
        
        static void ManualR()
        {
            var RTarget = TargetSelector.GetTarget(R.Range, R.DamageType, true);
            if(RM && RTarget != null && R.IsReady() && Player.HasBuff2("XerathLocusOfPower2", true) ||(Player.LastCastedSpellName() == "XerathLocusOfPower2" && Utils.GameTimeTickCount - Player.LastCastedSpellT() < 500))
            {
                R.CCast(RTarget);
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

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.End);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.CanCast(sender))
                E.Cast(sender);
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.CCast(target);
            }
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.LCast(target,0,0);
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
                damage += R.GetDamage2(enemy)*3;
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
