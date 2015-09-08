using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Talon // By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TalonNoxianDiplomacyBuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TalonDisappear"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 720f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 600f, TargetSelector.DamageType.Physical) {Delay = 0.1f, Speed = 902f*2};

            W.SetSkillshot(0.25f, 52f * (float)Math.PI / 180, 902f*2, false, SkillshotType.SkillshotCone);
            E.SetTargetted(0.25f, float.MaxValue);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseKillsteal();

            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.LightGreen));
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.LightGreen));
            
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            var drawQTimer = AIO_Menu.Champion.Drawings.getCircleValue("Q Timer");
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
    
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);

            var pos_temp = Drawing.WorldToScreen(Player.Position);

            if (drawQTimer.Active && getQBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));

            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || target == null)
                return;

            AIO_Func.AALcJc(Q);
            AIO_Func.AACb(Q);
        }

        static void Combo()
        {
            var FullComboTarget = TargetSelector.GetTarget(740, TargetSelector.DamageType.Physical);

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if (!FullComboTarget.IsValidTarget(300f) && FullComboTarget.IsValidTarget(E.Range))
                    E.Cast(FullComboTarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && (FullComboTarget.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && !Q.IsReady() && !Player.HasBuff2("TalonNoxianDiplomacyBuff")))
                W.ConeCast(FullComboTarget);

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady() && HeroManager.Enemies.Any(x => FullComboTarget.IsValidTarget(300f) && !Q.IsReady() && x.HasBuffOfType(BuffType.Slow)) && !Player.HasBuff2("TalonNoxianDiplomacyBuff"))
                R.Cast();
        }

        static void Harass()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);

                if(Wtarget != null)
                    W.ConeCast(Wtarget,50f);
            }
        }
        
        static void Laneclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                List<SharpDX.Vector2> minionVec2List = new List<SharpDX.Vector2>();

                foreach (var item in Minions)
                    minionVec2List.Add(item.ServerPosition.To2D());

                var wloc = MinionManager.GetBestCircularFarmLocation(minionVec2List, 200f, W.Range);

                if (wloc.MinionsHit >= 3)
                    W.Cast(wloc.Position);
            }
        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
                W.Cast(Mobs[0]);
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target, false, true);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R.GetDamage(target) * 2))
                    R.Cast();
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true) * 1.1f;
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy);

            if (R.IsReady() && AIO_Menu.Champion.Combo.UseR)
                damage += R.GetDamage2(enemy) * 2;

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true) * 1.1f;

            if (E.IsReady())
                damage = damage *(1 + 0.03f * E.Level);
                
            return damage;
        }
    }
}
