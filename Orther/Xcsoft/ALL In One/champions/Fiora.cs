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
    class Fiora // By RL244
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} // 피오라.cs update.
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float LastPingTime = 0;

        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "fioraqcd"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "FioraRiposte"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "FioraFlurry"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400f, TargetSelector.DamageType.Physical);

            Q.SetTargetted(0.25f, 3000f);

            AIO_Menu.Champion.Combo.addUseQ();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.QD", "Q Distance")).SetValue(new Slider(150, 0, 600));
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addItem("R Usage For Solo Target", false);
            
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(40);
            
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana(0);

            AIO_Menu.Champion.Misc.addItem("Ping Notify on R killable enemies (local)", true);
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.Blue));
            AIO_Menu.Champion.Drawings.addItem("W Timer", new Circle(true, Color.Black));
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.Red));

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) // 하레스 넣을 필요가 없어서 뺌. (AA에 하레스 있음)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) // 라인클리어 - 라인클리어도 필요없어서 뺌. (AAJcLc에 포함)
                    Jungleclear();
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            #endregion
            
            #region Ping Notify on R killable enemies
            if (R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Ping Notify on R killable enemies (local)"))
            {
                if (LastPingTime + 333 < Utils.GameTimeTickCount) //1:1 상황에서 궁으로 킬 가능시 핑찍기.
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(1200f) && AIO_Func.isKillable(x, R.GetDamage2(x)) && AIO_Func.ECTarget(x,1000f) == 1))
                        Game.ShowPing(PingCategory.Normal, target.Position, true);

                    LastPingTime = Utils.GameTimeTickCount;
                }
            } 
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var drawQTimer = AIO_Menu.Champion.Drawings.getCircleValue("Q Timer");
            var drawWTimer = AIO_Menu.Champion.Drawings.getCircleValue("W Timer");
            var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);

            if (drawQTimer.Active && getQBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));
            if (drawWTimer.Active && getWBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawWTimer.Color, "W: " + getWBuffDuration.ToString("0.00"));
            if (drawETimer.Active && getEBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));
        }
        
        static readonly string[] Attacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };
        static readonly string[] NoAttacks = { "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        static readonly string[] OHSP = { "parley", "ezrealmysticshot"};
        static readonly string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };

        static bool IsOnHit(string name)
        {
            return !name.ToLower().Contains("tower") &&!name.ToLower().Contains("turret") && !name.ToLower().Contains("mini") && !name.ToLower().Contains("minion") && name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower()) ||
            Attacks.Contains(name.ToLower()) || AttackResets.Contains(name.ToLower()) || OHSP.Contains(name.ToLower());
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var HeroTargets = sender as Obj_AI_Hero;
            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Mobs.Count >= 1 &&
            (!AIO_Menu.Champion.Jungleclear.UseW || !(Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))) ||
            Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && !AIO_Menu.Champion.Combo.UseW || !AIO_Menu.Champion.Harass.UseW ||
            Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && !(Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana))
            return;
            if (HeroTargets == null && (Player.Level == 1 && Player.HealthPercent < 100 && Mobs.Count >= 1 || Player.Level > 1 && Mobs.Count >= 1) && IsOnHit(args.SData.Name) && args.Target.IsMe && !sender.IsAlly && W.IsReady() && Player.Distance(args.End) < 40 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            W.Cast(); //1렙일때 만피로 정글에 W쓰는건 정글링 효율 떨어지기에 이렇게함.
            if (HeroTargets != null && IsOnHit(args.SData.Name) && args.Target.IsMe && !sender.IsAlly && W.IsReady())
            W.Cast(); //1렙일때 만피로 정글에 W쓰는건 정글링 효율 떨어지기에 이렇게함.
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

                AIO_Func.AALcJc(E);
                AIO_Func.AACb(E);
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady()) 
            {
                var qd = Menu.Item("Combo.QD").GetValue<Slider>().Value;
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                var Q2T = TargetSelector.GetTarget(Q.Range * 2, Q.DamageType);
                var QM = (Q2T != null ? MinionManager.GetMinions(Q2T.ServerPosition, 550f, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(x => x.Distance(Player.ServerPosition) <= Q.Range && x.Distance(Player.ServerPosition) >= Q.Range-150f) : null);

                if(qTarget != null && (qTarget.Distance(Player.ServerPosition) >= qd || getQBuffDuration < 1))
                    Q.Cast(qTarget);
                if(!Player.HasBuff2("fioraqcd") && Q2T != null && QM != null && Q2T.HealthPercent < Player.HealthPercent - 10 && AIO_Func.ECTarget(Q2T,600f,Player.HealthPercent - 9) == 0)
                    Q.Cast(QM);
            }
        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
                Q.Cast(Mobs[0]);
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (!Q.IsReady() && target.Distance(Player.ServerPosition) > R.Range - 100f && R.CanCast(target) && AIO_Func.ECTarget(target,800f) == 1 && AIO_Func.isKillable(target, R.GetDamage2(target) + (float)Player.GetAutoAttackDamage2(target, true)))
                {
                    if(AIO_Menu.Champion.Combo.getBoolValue("R Usage For Solo Target"))
                        R.Cast(target);
                }
                if (R.CanCast(target) && AIO_Func.ECTarget(target,800f) >= 2 && AIO_Func.isKillable(target, R.GetDamage2(target,3)))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;
            var Target = enemy as Obj_AI_Hero;

            if (Q.IsReady() && !Player.HasBuff2("fioraqcd"))
                damage += Q.GetDamage2(enemy) * 2;
            else if (Q.IsReady() && Player.HasBuff2("fioraqcd"))
                damage += Q.GetDamage2(enemy);
                
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                                
            if (E.IsReady())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true) * 2;
                
            if (R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("KillstealR") && AIO_Func.ECTarget(Target,800f) == 1)
                damage += R.GetDamage2(enemy);
            else if (R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("KillstealR") && AIO_Func.ECTarget(Target,800f) > 1)
                damage += R.GetDamage2(enemy,3);

            if (!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}

