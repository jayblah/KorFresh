﻿using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Janna
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1100f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R, 550f);

            Q.SetSkillshot(0.25f, 120f, 900f, false, SkillshotType.SkillshotLine);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            //AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ(false);
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Misc.addItem("Auto E", true);

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
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (Q.CanCast(sender))
                Q.Cast(sender);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (AIO_Menu.Champion.Misc.getBoolValue("Auto E") && E.IsReady() && sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Hero && args.Target.IsAlly && Player.ServerPosition.Distance(args.Target.Position) <= E.Range && args.Target.IsValid<Obj_AI_Hero>())
                Utility.DelayAction.Add(100, () => E.Cast((Obj_AI_Hero)args.Target));
        }

        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
                W.CastOnBestTarget();

            //if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            //{ }
        }

        static void Harass()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
                W.CastOnBestTarget();
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);

            return damage;
        }
    }
}
