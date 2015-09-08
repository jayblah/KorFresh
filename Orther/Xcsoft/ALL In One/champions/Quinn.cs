using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Quinn// By RL244 QuinnQ(Target) QuinnW(Target) quinnw_cosmetic(Target) quinnwpassiveready quinnsuppressq quinnpassiveammo quinneroot(Target) quinnrtimeout
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R, RQ, RE;
        static bool Bird {get{return Player.HasBuff2("quinnrtimeout"); }}
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "quinnrtimeout"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 2100f);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Physical);
            RQ = new Spell(SpellSlot.Q, 275f, TargetSelector.DamageType.Physical) {Delay = 0.25f}; 
            RE = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 700f, TargetSelector.DamageType.Physical) {Delay = 0.25f};
            
            E.SetTargetted(0.25f, 1600f);
            RE.SetTargetted(0.25f, 1600f);
            Q.SetSkillshot(0.25f, 70f, 1500f, false, SkillshotType.SkillshotLine); //true 했다가 false로 고침. 이는 폭발 반경을 이용하기 위함. 1550하면 상대가 안맞음. 1500이 정확한듯.

            AIO_Menu.Champion.Flee.addUseR();
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            
            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ(false);
            AIO_Menu.Champion.Lasthit.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
            if (Orbwalking.CanMove(10))
            {
                if(Bird)
                {
                    AIO_Func.SC(RQ);
                    AIO_Func.SC(RE);
                    foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                    {
                        if (R.CanCast(target) && AIO_Func.isKillable(target, R.GetDamage2(target)) && !AIO_Func.isKillable(target, (float)Player.GetAutoAttackDamage2(target, true)) && Bird)
                        AIO_Func.SC(R);
                    }
                }
                else
                {
                    if(!Player.HasBuff2("quinnpassiveammo"))
                    AIO_Func.SC(E);
                    AIO_Func.SC(Q,0f,0f,1f,210f);
                    foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                    {
                        if (R.CanCast(target) && !Q.IsReady() && AIO_Func.isKillable(target, getComboDamage(target)*2) && !AIO_Func.isKillable(target, (float)Player.GetAutoAttackDamage2(target, true)) && !Bird)
                        AIO_Func.SC(R);
                    }
                }
            }
            
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee && AIO_Menu.Champion.Flee.UseR)
            {
                var Rtarget = TargetSelector.GetTarget(1000f, Q.DamageType, true); 
                if(!Bird && Rtarget != null && R.IsReady())
                R.Cast();
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();

        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            
            if (Q.IsReady() && drawQ.Active && !Bird)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (RQ.IsReady() && drawQ.Active && Bird)
                Render.Circle.DrawCircle(Player.Position, RQ.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender) && E.IsReady() && !Bird)
                E.Cast(gapcloser.Sender);
        }

        static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            return;
            if (args.Slot == SpellSlot.Q && Bird && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                if(Items.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only) && Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
                Items.UseItem((int)ItemId.Ravenous_Hydra_Melee_Only);
                if(Items.HasItem((int)ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
                Items.UseItem((int)ItemId.Tiamat_Melee_Only);
            }
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q) && !Bird)
                AIO_Func.LCast(Q,target,0f,0f,false,210f);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R) && Bird)
                R.Cast();
            }
        }

        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            if (E.IsReady())
                damage += E.GetDamage2(enemy);
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
