using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Jayce// By RL244 jaycepassivedebuff(Target) jaycestancehammer jaycepassivemeleeattack jaycestancegun jaycepassiverangedattack jaycehypercharge jaycepassivehaste 
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R, RQ, RW, RE, REQ;
        static int RQTime = 0;
        static bool HAMMER {get{return Player.HasBuff2("jaycestancehammer"); }}
        static bool QEM {get{return Menu.Item("Combo.Use QEM").GetValue<KeyBind>().Active; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 285f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 300f, TargetSelector.DamageType.Magical);
            RQ = new Spell(SpellSlot.Q, 1150f, TargetSelector.DamageType.Physical); // 1050
            REQ = new Spell(SpellSlot.Q, 1610f, TargetSelector.DamageType.Physical); // 1470
            RW = new Spell(SpellSlot.W, 500f, TargetSelector.DamageType.Physical);
            RE = new Spell(SpellSlot.E, 650f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R);
            
            Q.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, float.MaxValue);
            RQ.SetSkillshot(0.25f, 80f, 1450f, false, SkillshotType.SkillshotLine); //true 했다가 false로 고침. 이는 폭발 반경을 이용하기 위함.
            REQ.SetSkillshot(0.25f, 80f, 2030f, false, SkillshotType.SkillshotLine);
            RE.SetSkillshot(0.25f, 20f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Flee.addUseE();
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.Use QEM", "Use QE(Manual)")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false));

            
            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addUseR();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ(false);
            AIO_Menu.Champion.Lasthit.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("AutoR", true);
            AIO_Menu.Champion.Misc.addItem("Parlel E", true);
            AIO_Menu.Champion.Misc.addItem("E Distance", new Slider(25, 10, 650));
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
            RW.Range = Orbwalking.GetRealAutoAttackRange(Player);

            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseE)
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (E.CanCast(target) && AIO_Func.isKillable(target, E) && HAMMER)
                    E.Cast(target);
                }
            }
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseE)
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (E.CanCast(target) && HAMMER && (R.IsReady() || !Player.HasBuff2("jaycehypercharge") && !Q.IsReady()))
                    E.Cast(target);
                }
            }
            if (Orbwalking.CanMove(10))
            {
                if(HAMMER)
                {
                    AIO_Func.SC(Q);
                    AIO_Func.SC(W);
                    if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    AIO_Func.SC(E);
                }
                else
                {
                    AIO_Func.SC(RW);
                    if(!E.IsReady())
                    AIO_Func.SC(RQ,0f,0f,1f,150f); // 폭발범위 확인필요함.
                    else
                    {
                        AIO_Func.SC(REQ,0f,0f,1f,210f);
                    }
                }
            }
            
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee && AIO_Menu.Champion.Flee.UseE)
            {
                if(HAMMER)
                R.Cast();
                else if(RE.IsReady())
                RE.Cast(getParalelVec(Game.CursorPos));
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
                
            ManualQE();
            AutoRE();
            if (AIO_Menu.Champion.Misc.getBoolValue("AutoR"))
                AutoR();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            if (Q.IsReady() && drawQ.Active && HAMMER)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active && HAMMER)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active && HAMMER)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (RQ.IsReady() && drawQ.Active && !HAMMER)
                Render.Circle.DrawCircle(Player.Position, RQ.Range, drawQ.Color);
            if (RW.IsReady() && drawW.Active && !HAMMER)
                Render.Circle.DrawCircle(Player.Position, RW.Range, drawW.Color);
            if (E.IsReady() && drawE.Active && !HAMMER)
                Render.Circle.DrawCircle(Player.Position, REQ.Range, drawE.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender) && E.IsReady() && HAMMER)
                E.Cast(gapcloser.Sender);
            if (E.CanCast(gapcloser.Sender) && !HAMMER)
                R.Cast();
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.CanCast(sender) && E.IsReady() && HAMMER)
                E.Cast(sender);
            if (E.CanCast(sender) && !HAMMER)
                R.Cast();
        }
        static void AutoRE()
        {
            if(RQTime - Utils.GameTimeTickCount + 250 >= 0 && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                var Target = TargetSelector.GetTarget(REQ.Range, Q.DamageType, true);
                if(Target != null && !HAMMER)
                E.Cast(getParalelVec(Target.ServerPosition));
            }
        }
        static void AutoR()
        {
            var Target = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && R.IsReady() && Target != null)
            {
                if(HAMMER && Player.Distance(Target.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player) && !Q.IsReady() && !E.IsReady())
                R.Cast();
                else if(HAMMER && Player.Distance(Target.ServerPosition) > E.Range + 50f && !Q.IsReady())
                R.Cast();
                else if(!HAMMER && !Q.IsReady() && (!W.IsReady() || Player.HasBuff2("jaycehypercharge")) && !E.IsReady() && AIO_Func.ECTarget(Player,1000f,40) <= 1 && Player.HealthPercent > 40
                && AIO_Func.isKillable(Target,(Q.GetDamage2(Target,1) + W.GetDamage2(Target) + E.GetDamage2(Target) + (float)Player.GetAutoAttackDamage2(Target, true))*1.5f))
                {
                    R.Cast();
                }
            }
        }
        
        static void ManualQE()
        {
            //var Target = TargetSelector.GetTarget(REQ.Range, Q.DamageType, true);
            if(QEM)//&& Target != null
            {
                if(HAMMER)
                R.Cast();
                else if(RQ.IsReady() && RE.IsReady())
                {
                    RQ.Cast(Game.CursorPos);
                    Utility.DelayAction.Add(
                            (int)(RQ.Delay), () => RE.Cast(getParalelVec(Game.CursorPos)));
                    //RE.Cast(getParalelVec(Game.CursorPos));
                }
            }
        }
        
        static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            return;
            if (args.Slot == SpellSlot.W && HAMMER && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                if(Items.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only) && Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
                Items.UseItem((int)ItemId.Ravenous_Hydra_Melee_Only);
                if(Items.HasItem((int)ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
                Items.UseItem((int)ItemId.Tiamat_Melee_Only);
            }
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead) // 
                return;
                
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseE || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseE)
            {//Player.Spellbook.GetSpell(SpellSlot.Q).Name
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(REQ.Range)) && RE.IsReady() && !HAMMER)
                {
                    //var Etarget = TargetSelector.GetTarget(REQ.Range, Q.DamageType);
                    //Utility.DelayAction.Add(
                    //        (int)(RQ.Delay), () => RE.Cast(getParalelVec(args.End)));
                    RQTime = Utils.GameTimeTickCount;
                    Utility.DelayAction.Add(
                            (int)(RQ.Delay), () => AIO_Func.sendDebugMsg("제이스 개같은 넘 !!"));
                    //if(Etarget != null)

                }
            }
        }
        
        static Vector2 getParalelVec(Vector3 pos)
        {
            int away = AIO_Menu.Champion.Misc.getSliderValue("E Distance").Value;
            if (AIO_Menu.Champion.Misc.getBoolValue("Parlel E"))
            {
                Random rnd = new Random();
                int neg = rnd.Next(0, 1);
                away = (neg == 1) ? away : -away;
                var v2 = Vector3.Normalize(pos - Player.ServerPosition) * away;
                var bom = new Vector2(v2.Y, -v2.X);
                return Player.ServerPosition.To2D() + bom;
            }
            else
            {
                var v2 = Vector3.Normalize(pos - Player.ServerPosition) * away;
                var bom = new Vector2(v2.X, v2.Y);
                return Player.ServerPosition.To2D() + bom;
            }
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (RQ.CanCast(target) && AIO_Func.isKillable(target, RQ) && !HAMMER)
                AIO_Func.LCast(RQ,target);
            }
        }

        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            
            if(HAMMER)
            {
                if (Q.IsReady())
                    damage += Q.GetDamage2(enemy,1);
                if (W.IsReady())
                    damage += W.GetDamage2(enemy);
                if (E.IsReady())
                    damage += E.GetDamage2(enemy);
                if (R.IsReady() && !HAMMER)
                {
                    if (Q.IsReady() && !E.IsReady())
                        damage += Q.GetDamage2(enemy);
                    if (W.IsReady())
                        damage += (float)Player.GetAutoAttackDamage2(enemy, true)*2;
                    if (Q.IsReady() && E.IsReady())
                        damage += Q.GetDamage2(enemy)*1.4f;
                }
            }
            else
            {
                if (Q.IsReady() && !E.IsReady())
                    damage += Q.GetDamage2(enemy);
                if (W.IsReady())
                    damage += (float)Player.GetAutoAttackDamage2(enemy, true)*2;
                if (Q.IsReady() && E.IsReady())
                    damage += Q.GetDamage2(enemy)*1.4f;
                if (R.IsReady() && Player.Distance(enemy.Position) <= Q.Range)
                {
                    if (Q.IsReady())
                        damage += Q.GetDamage2(enemy,1);
                    if (W.IsReady())
                        damage += W.GetDamage2(enemy);
                    if (E.IsReady())
                        damage += E.GetDamage2(enemy);
                }
            }

                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
