using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Karma// By RL244 WIP
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static bool Mantra {get{return Player.HasBuff2("KarmaMantra"); }}
        static Spell Q, W, E, R;
        static int pastTime = 0;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}

        public static void Load() //KarmaSpiritBind(Target) KarmaMantra karmaqmissileslow(Target) KarmaSolKimShield
        {
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 675f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R);
            
            Q.SetSkillshot(0.25f, 60f, 1700f, false, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, float.MaxValue);

            AIO_Menu.Champion.Flee.addUseE();
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Qrange")).SetValue(new Slider(0, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
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
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
            if (Utils.GameTimeTickCount - pastTime <= Game.Ping / 2 + 20) return; // 얘가 스펠 인식을 제대로 못해서 넣어줌 -_-;
            pastTime = Utils.GameTimeTickCount;
            


            if(!Mantra)
            {
                if(AIO_Menu.Champion.Combo.UseE && Player.ManaPercent > 20)
                E.Shield();
            
                var ETarget = TargetSelector.GetTarget(Q.Range*1.5f, Q.DamageType, true);
                if(ETarget != null && ETarget.Distance(Player.ServerPosition) > W.Range && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseE && E.IsReady() && E.Instance.Name == "KarmaSolKimShield")
                    E.Cast(Player);
                
                var QTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
                var pred = Prediction.GetPrediction(QTarget, Q.Delay, Q.Width/2, Q.Speed); //spell.Width/2
                var collision = Q.GetCollision(Player.ServerPosition.To2D(), new List<SharpDX.Vector2> { pred.CastPosition.To2D() });
                var minioncol = collision.Count(x => x.IsMinion);
                if(QTarget != null && !Mantra && Player.HealthPercent > 60 && Q.IsReady() && AIO_Menu.Champion.Combo.UseR && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && minioncol == 0)
                    R.Cast();
                    
                var Target = TargetSelector.GetTarget(W.Range, W.DamageType, true);
                if(Player.HealthPercent <= 40 && W.IsReady() && Target != null && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseR)
                    R.Cast();
            }
            
            if(Player.HealthPercent > 40 && !Mantra)
                AIO_Func.SC(W);
            else if(Player.HealthPercent <= 40 && Mantra)
                AIO_Func.SC(W);
            
            if(Player.HealthPercent >= 40)
                AIO_Func.SC(Q,QD,0f,1f,250f);
            else if(Mantra && !W.IsReady())
                AIO_Func.SC(Q,QD,0f,1f,250f);
            else if(!Mantra)
                AIO_Func.SC(Q,QD,0f,1f,250f);
            
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee && E.IsReady() && AIO_Menu.Champion.Flee.UseE)
                E.Cast(Player);

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var Qtarget = TargetSelector.GetTarget(Q.Range + Player.MoveSpeed * Q.Delay, TargetSelector.DamageType.Magical);

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

            if (Q.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= Q.Range)
                Q.Cast((Vector3)gapcloser.End);
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                AIO_Func.LCast(Q,target,QD,0);
            }
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (target.IsValidTarget(W.Range) && AIO_Func.isKillable(target, W.GetDamage2(target,1)))
                W.Cast(target);
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady() && !R.IsReady() && !Mantra)
                damage += Q.GetDamage2(enemy);
                
            if (Q.IsReady() && (R.IsReady() || Mantra))
                damage += Q.GetDamage2(enemy,1);
                
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
               
                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
