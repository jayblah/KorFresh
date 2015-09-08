using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Ezreal// By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} // 확실히 필요함. 특히 논타겟 챔프 타겟 선정 범위 사용자가 설정하게 하기 위해서도.
        static float LastPingTime = 0;
        static bool RM {get{return Menu.Item("Combo.Use MR").GetValue<KeyBind>().Active; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1150f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 475f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 3000f, TargetSelector.DamageType.Magical);

            E.SetSkillshot(0.25f, 750f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            Q.SetSkillshot(0.25f, 60f, 2000f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.0f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE(false);
            AIO_Menu.Champion.Combo.addUseR();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.Use MR", "Use R(Manual)")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false));

            AIO_Menu.Champion.Harass.addAutoHarass();
            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana(40);
            
            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE(false);
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("Ping Notify on R killable enemies (local/client side)", true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(0, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q,QD,0f);
                AIO_Func.SC(W,QD);
                AIO_Func.SC(E);
                AIO_Func.FleeToPosition(E);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            #endregion
            
            #region Ping Notify on R killable enemies
            if (R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Ping Notify on R killable enemies (local/client side)"))
            {
                if (LastPingTime + 333 < Utils.GameTimeTickCount) //궁 80퍼 뎀지 이상으로 잡을수 있는 적 핑찍기.
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(10000f) && AIO_Func.isKillable(x, R.GetDamage2(x)*0.8f)))
                        Game.ShowPing(PingCategory.Normal, target.Position, true);

                    LastPingTime = Utils.GameTimeTickCount;
                }
            } 
            #endregion
            ManualR();
        }
        
        static void ManualR()
        {
            var RTarget = TargetSelector.GetTarget(3000f, R.DamageType, true);
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
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
            
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;

            if (!unit.IsMe || Target == null)
                return;

            AIO_Func.AALcJc(Q,QD,0f);
            AIO_Func.AACb(Q,QD,0f);
        }
        
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (R.CanCast(target) && AIO_Func.isKillable(target, getComboDamage(target)) && target.Distance(Player.ServerPosition) < 1000)
                        AIO_Func.LCast(R,target);
                    else if (R.CanCast(target) && AIO_Func.isKillable(target, R) && target.Distance(Player.ServerPosition) < 3000)
                        AIO_Func.LCast(R,target);
                }
            }
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.LCast(Q,target,QD,0f);
            }
        }
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    AIO_Func.LCast(W,target,QD);
            }
        }
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    AIO_Func.CCast(E,target);
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
                damage += E.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
