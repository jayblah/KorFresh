﻿using System;
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
    class Zyra// By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R, P;
        static float ED { get { return AIO_Menu.Champion.Misc.getSliderValue("Additional Erange").Value; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 800f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 1100f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 700f, TargetSelector.DamageType.Magical);
            P = new Spell(SpellSlot.Q, 1470f, TargetSelector.DamageType.True);
            
            Q.SetSkillshot(0.625f, 85f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 50f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 120f, 1150f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            P.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("R Min Targets", new Slider(2, 0, 5));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW(false);
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Misc.addItem("Additional Erange", new Slider(50,0,250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addItem("E Safe Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addErange(false);
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (!ZyraDead && Player.IsDead)
                return;
                
            if (ZyraDead)
                CastP();

            if (Orbwalking.CanMove(35))
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

            Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!ZyraDead && Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawEr = AIO_Menu.Champion.Drawings.getCircleValue("E Safe Range");
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed * E.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawEr.Active && etarget != null)
                Render.Circle.DrawCircle(Player.Position, E.Range - etarget.MoveSpeed*E.Delay, drawEr.Color);
        
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static bool ZyraDead
        {
            get
            {
                return  ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name ==
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name ||
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name ==
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name;
            }
        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }
        
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                AIO_Func.CCast(Q, Q.GetTarget());

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
                AIO_Func.LCast(E, E.GetTarget(), ED, float.MaxValue);

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && (Q.IsReady() || E.IsReady()))
                AIO_Func.CCast(W, E.GetTarget());

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = R.GetTarget();

                if (Player.HealthPercent < 50 && AIO_Func.isKillable(Rtarget, getComboDamage(Rtarget) * 2 / 3) || AIO_Func.ECTarget(Rtarget, R.Width, 0, 100) >= AIO_Menu.Champion.Combo.getSliderValue("R Min Targets").Value)
                    AIO_Func.CCast(R, Rtarget);
            }
        }

        static void Harass()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana))
                return;
        
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                AIO_Func.CCast(Q, Q.GetTarget());
            
            if (AIO_Menu.Champion.Harass.UseW && W.IsReady() && (Q.IsReady() || E.IsReady()))
                AIO_Func.CCast(W, W.GetTarget());

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
                AIO_Func.LCast(E, E.GetTarget(), ED, float.MaxValue);
        }

        static void Laneclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana))
                return;
        
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                var _m = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.E))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / E.Speed), (int)(E.Delay * 1000 + Game.Ping / 2)) > 0);            
                
                if (_m != null)
                    AIO_Func.LCast(E,_m,ED,float.MaxValue);
            }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady() && (Q.IsReady() || E.IsReady()))
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range)))
                    AIO_Func.CCast(W,Minions[0]);
            }
            
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);            
                
                if (_m != null)
                    AIO_Func.CCast(Q,_m);
            }
        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))
                return;
        
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    AIO_Func.CCast(Q,Mobs.FirstOrDefault());
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady() && (Q.IsReady() || E.IsReady()))
            {
                if (Mobs.Any(x => x.IsValidTarget(W.Range)))
                    AIO_Func.CCast(W,Mobs[0]);
            }
            
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(E.Range)))
                    AIO_Func.LCast(E,Mobs[0],ED,float.MaxValue);
            }

        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q) && AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                    AIO_Func.CCast(Q,target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R) && AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                    AIO_Func.CCast(R, target);

                if (E.CanCast(target) && AIO_Func.isKillable(target, E) && AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                    AIO_Func.LCast(E, target, ED, float.MaxValue);
            }
        }
        
        static void CastP()
        {
            if(P.IsReady())
            {
                var Ptarget = TargetSelector.GetTarget(P.Range, TargetSelector.DamageType.True);

                AIO_Func.LCast(P,Ptarget,200f,float.MaxValue);
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true);
                
            if (E.IsReady())
                damage += E.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if (ZyraDead && P.IsReady())
                damage += P.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);

            return damage;
        }
    }
}
