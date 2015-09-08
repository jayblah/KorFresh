using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Teemo// By RL244
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 680f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 230f, TargetSelector.DamageType.Magical);
            
            Q.SetTargetted(0.25f, 1800f);
            R.SetSkillshot(0.25f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Flee.addUseW();
            AIO_Menu.Champion.Flee.addIfMana();
            
            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;

            if (!unit.IsMe || Target == null)
                return;

            AIO_Func.AACb(Q);
            AIO_Func.AALcJc(Q);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.Sender);

            if (W.IsReady())
                W.Cast();
        }


        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && HeroManager.Enemies.Any(x => x.IsValidTarget(1000f) && !x.IsFacing(Player)))
                W.Cast();
        }
        static void Harass()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget();
        }


        static void Laneclear()
        {
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var bigminion = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy).Where(x => Q.IsKillable(x) && (x.SkinName.ToLower().Contains("siege") || x.SkinName.ToLower().Contains("super"))).FirstOrDefault();

                if (bigminion != null)
                    Q.CastOnUnit(bigminion);
            }
        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Mobs[0] != null)
                    Q.CastOnUnit(Mobs[0]);
            }
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.CastOnUnit(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            return damage;
        }
    }
}
