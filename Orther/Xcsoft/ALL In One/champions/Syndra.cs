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
    class Syndra// By RL244 WIP syndraqremoval syndrawtooltip (<- w들면 저 버프 생김)
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static List<Obj_AI_Base> SyndraQ { get { return ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsAlly && x.Name == "k" && x.Position.Distance(Player.ServerPosition) <= W.Range).ToList(); } }
        static float ED {get{return Menu.Item("Misc.Etg").GetValue<Slider>().Value; }}

        static Spell Q, QE, W, W2, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 800f, TargetSelector.DamageType.Magical);
            QE = new Spell(SpellSlot.Q, Q.Range + 500, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 925f, TargetSelector.DamageType.Magical);
            W2 = new Spell(SpellSlot.W, 925f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 675f, TargetSelector.DamageType.Magical);
            
            
            Q.SetSkillshot(0.3f, 65f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetTargetted(0.25f, float.MaxValue);
            W2.SetSkillshot(0.25f, 50f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, 2500f, false, SkillshotType.SkillshotCone);
            R.SetTargetted(0.75f, 2500f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("QE Cast", new KeyBind('T', KeyBindType.Press));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE(false);
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Etg", "Additional Erange")).SetValue(new Slider(0, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", false);
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
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
            if(3 == R.Level)
            R.Range = 750f;
            
            if(3 == E.Level)
            E.Width = 60f * (float)Math.PI / 180 * 1.5f;
                
            AIO_Func.SC(Q);
            if(Player.HasBuff("syndrawtooltip"))
                AIO_Func.SC(W2,0f,float.MaxValue,0f); //w2타는 마나가 안드므로 노코스트처럼 인식시켰음.
                
            var Minions = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            var Mobs = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType, true); 
            var MiniTarget = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly);

            if(W.IsReady() && !Player.HasBuff("syndrawtooltip"))
            {
                if((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseW || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseW && Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana) && Wtarget != null || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && (Minions.Count > 0 && AIO_Menu.Champion.Laneclear.UseW && Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana|| Mobs.Count > 0 && AIO_Menu.Champion.Jungleclear.UseW && Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))
                {
                    if(SyndraQ.Count > 0)
                        W.Cast(SyndraQ[0].ServerPosition);
                    else if (MiniTarget.Count > 0)
                        W.Cast(MiniTarget[0]);
                }
            }
            AIO_Func.SC(E);

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
                
                
            if(AIO_Menu.Champion.Combo.getKeyBindValue("QE Cast").Active)
                QECast();
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseE || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseE))
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(QE.Range)) && E.IsReady())
                {
                    var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                    E.ConeCast(Etarget);
                }
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
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
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= E.Range)
                E.Cast((Vector3)gapcloser.End);
        }

        static void QECast()
        {
            if(Q.IsReady() && E.IsReady())
            {
                Q.NMouse();
                Utility.DelayAction.Add(
                        (int)(Q.Delay), () => E.NMouse());
            }
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                AIO_Func.CCast(Q,target);
            }
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W) && Player.HasBuff("syndrawtooltip"))
                AIO_Func.CCast(W,target);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (target.IsValidTarget(R.Range) && AIO_Func.isKillable(target, R.GetDamage2(target) + R.GetDamage2(target,1) * SyndraQ.Count()))
                R.Cast(target);
            }
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                AIO_Func.LCast(E,target,ED,float.MaxValue);
            }
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
            {
                damage += Q.GetDamage2(enemy);
            }
                
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                
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
