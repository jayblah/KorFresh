using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Ekko// By RL244
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}
        static Obj_AI_Base Clone; // 에코 클론.
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1050f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 1600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 325f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 400f) {Delay = 0.25f};

            Q.SetSkillshot(0.25f, 60f, 1700f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(3.0f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Flee.addUseE();
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(0, 0, 150));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("R Cast for Heal", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                Q.SC(QD);
                E.MouseSC();
                W.SC();
            }
            
            E.FleeToPosition();
            
            #region R.Cast()
            var RT = (Clone != null ? HeroManager.Enemies.Where(x => x.Distance(Clone.ServerPosition) <= R.Range).FirstOrDefault() : null);
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseR && Clone.ECTarget(R.Range) > 0 && R.IsReady())
                R.Cast();
                
            if(Player.HealthPercent < 30 && (RT != null && AIO_Func.isKillable(RT,R.GetDamage2(RT)) || RT == null && 1000f.EnemyCount() > 0) && R.IsReady()
            && AIO_Menu.Champion.Misc.getBoolValue("R Cast for Heal"))
                R.Cast();
            #endregion
            
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
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active && Clone != null)
                Render.Circle.DrawCircle(Clone.Position, R.Range, drawR.Color);
        }
        
        static void OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Base) || !sender.IsAlly)
                return;

            if (sender.IsAlly && sender.Name == "Ekko")
                Clone = (Obj_AI_Base)sender;
        }
        
        static void OnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Base) || !sender.IsAlly)
                return;

            if (sender.IsAlly && sender.Name == "Ekko")
                Clone = null;
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            return; //당장은 쓸계획이 없음.
            /*var Sender = (Obj_AI_Base) sender;
            var STarget = (Obj_AI_Hero) args.Target;
            if (!sender.IsMe || Player.IsDead) // 갈리오 W 로직 미완성
                return;
            if (args.Target.IsMe && !sender.IsAlly && W.IsReady() && Player.HealthPercent < 80 //args.Target.IsMe && AIO_Menu.Champion.Misc.getBoolValue("R Myself Only")
                && Player.Distance(args.End) < 150 && AIO_Menu.Champion.Combo.UseW)
                W.Cast(Player);
            if (!sender.IsAlly && W.IsReady() && Player.HealthPercent < 80 && Player.Distance(args.End) < 150 &&
                Sender.Distance(Player.ServerPosition) <= 1000f && AIO_Menu.Champion.Combo.UseW)
                W.Cast(Player);
            if (!sender.IsAlly && W.IsReady() && Player.HealthPercent < 80 &&
                Sender.Distance(Player.ServerPosition) <= 700f && AIO_Menu.Champion.Combo.UseW)
                W.Cast(Player);*/
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.LCast(target,QD);
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
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
