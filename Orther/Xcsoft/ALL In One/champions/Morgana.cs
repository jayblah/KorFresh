using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Morgana// By RL244 
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "asheqattack"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } } // 차후 수
        static float RM {get{return Menu.Item("Combo.RM").GetValue<Slider>().Value; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1250f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 750f);
            R = new Spell(SpellSlot.R, 625f, TargetSelector.DamageType.Magical){Delay = 0.5f};

            Q.SetSkillshot(0.5f, 80f, 1200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.65f, 175f, float.MaxValue, false, SkillshotType.SkillshotCircle); //원래 데이터는 0.25이지만 더 오래 딜을 넣을수 있도록 딜레이가 마치 긴것처럼 수정함.
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseR();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.RM", "R Min Targets")).SetValue(new Slider(1, 1, 5));
            
            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(0, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.LightPink));

        
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

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q,QD,0f);
                AIO_Func.SC(W);
                if(R.Range.EnemyCount() >= RM)
                AIO_Func.SC(R);
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
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (drawETimer.Active && getEBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E Buff : " + getEBuffDuration.ToString("0.00"));
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender))
                AIO_Func.LCast(Q,gapcloser.Sender,0f,0f);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (Q.CanCast(sender))
                AIO_Func.LCast(Q,sender,0f,0f);
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.LCast(Q,target,0);
            }
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy)*3;
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy)*2;
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
