using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;


namespace FreshRyze
{
    internal class MainMenu
    {
        private static Menu _MainMenu;
        private static Orbwalking.Orbwalker _OrbWalker;
        private static Obj_AI_Hero Player;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public MainMenu()
        {
            _MainMenu = new Menu("Ryze", "Ryze", true);

            Q = new Spell(SpellSlot.Q, 900);
            Q.SetSkillshot(0.25f, 50f, 1700, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            Menu orbwalkerMenu = new Menu("OrbWalker", "OrbWalker");
            _OrbWalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _MainMenu.AddSubMenu(orbwalkerMenu);

            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _MainMenu.AddSubMenu(targetSelectorMenu);

            var Combo = new Menu("Combo", "Combo");
            {
                Combo.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
                Combo.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
                Combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
                Combo.AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            }
            _MainMenu.AddSubMenu(Combo);

            var Harass = new Menu("Harass", "Harass");
            {
                Harass.AddItem(new MenuItem("HUseQ", "Use Q").SetValue(true));
                Harass.AddItem(new MenuItem("HUseW", "Use W").SetValue(true));
                Harass.AddItem(new MenuItem("HUseE", "Use E").SetValue(true));                
                Harass.AddItem(new MenuItem("HManaRate", "Mana %").SetValue(new Slider(20)));
                Harass.AddItem(new MenuItem("AutoHarass", "Auto Harass").SetValue(true));
            }
            _MainMenu.AddSubMenu(Harass);

            var LaneClear = new Menu("LaneClear", "LaneClear");
            {
                LaneClear.AddItem(new MenuItem("LUseQ", "Use Q").SetValue(true));
                LaneClear.AddItem(new MenuItem("LUseW", "Use W").SetValue(true));
                LaneClear.AddItem(new MenuItem("LUseE", "Use E").SetValue(true));
                LaneClear.AddItem(new MenuItem("LManaRate", "Mana %").SetValue(new Slider(20)));
            }
            _MainMenu.AddSubMenu(LaneClear);

            var JungleClear = new Menu("JungleClear", "JungleClear");
            {
                JungleClear.AddItem(new MenuItem("Use Q", "Use Q").SetValue(true));
                JungleClear.AddItem(new MenuItem("Use W", "Use W").SetValue(true));
                JungleClear.AddItem(new MenuItem("Use E", "Use E").SetValue(true));
                JungleClear.AddItem(new MenuItem("ManaRate", "Mana %").SetValue(new Slider(20)));
            }
            _MainMenu.AddSubMenu(JungleClear);

            var Misc = new Menu("Misc", "Misc");
            {
                Misc.AddItem(new MenuItem("AutoLasthit", "Auto LastHit with Spell").SetValue(true));
                Misc.AddItem(new MenuItem("WGap", "Auto W On GapClosers").SetValue(true));
            }
            _MainMenu.AddSubMenu(Misc);

            var Draw = new Menu("Draw", "Draw");
            {                
                Draw.AddItem(new MenuItem("QRange", "Q Range").SetValue(true));
                Draw.AddItem(new MenuItem("WRange", "W Range").SetValue(true));
                Draw.AddItem(new MenuItem("ERange", "E Range").SetValue(true));
            }
            _MainMenu.AddSubMenu(Draw);

            _MainMenu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {            
            if (_MainMenu.Item("QRange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,2);
            }
            if (_MainMenu.Item("WRange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White, 2);
            }
            if (_MainMenu.Item("ERange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White, 2);
            }            
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var QTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var WTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var ETarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);            
            if (_OrbWalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {                
                if (QTarget != null && Q.IsReady())
                {
                    Q.Cast(QTarget);
                }
                if (WTarget != null && W.IsReady())
                {
                    W.Cast(WTarget);
                }
                if (ETarget != null && E.IsReady())
                {
                    E.Cast(ETarget);
                }
                if (ETarget != null && R.IsReady())
                {
                    R.Cast();
                }
            }            
            
            if ((_MainMenu.Item("AutoHarass").GetValue<bool>() | _OrbWalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed ) && ObjectManager.Player.ManaPercent > _MainMenu.Item("HManaRate").GetValue<Slider>().Value)
            {
                if (_MainMenu.Item("HUseQ").GetValue<bool>() && QTarget != null) { Q.Cast(QTarget); }
                if (_MainMenu.Item("HUseW").GetValue<bool>() && WTarget != null) { W.Cast(WTarget); }
                if (_MainMenu.Item("HUseE").GetValue<bool>() && ETarget != null) { E.Cast(ETarget); }
            }

            if(_OrbWalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var MinionTarget = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
                if (MinionTarget.Count <= 0) { return; }                

                foreach(var minion in MinionTarget)
                {
                    if(ObjectManager.Player.ManaPercent > _MainMenu.Item("LManaRate").GetValue<Slider>().Value)
                    {
                        if (Q.IsReady())
                        {
                            Q.CastIfHitchanceEquals(minion, HitChance.VeryHigh, true);
                        }
                        if (W.IsReady())
                        {
                            W.Cast(minion, true);
                        }
                        if (E.IsReady())
                        {
                            E.Cast(minion, true);
                        }
                    }
                }                
            }
            var RyzePassive = ObjectManager.Player.Buffs.Find(DrawFX => DrawFX.Name == "ryzepassivestack" && DrawFX.IsValidBuff()).Count;
            Drawing.DrawText(200, 200, System.Drawing.Color.Aqua, RyzePassive.ToString());
            if (_MainMenu.Item("AutoLasthit").GetValue<bool>() && (RyzePassive != null || RyzePassive < 4))
            {
                var MinionTarget = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
                if (MinionTarget.Count <= 0) { return; }

                if (Q.IsReady())
                {
                    var qTarget = MinionTarget.Where(x => x.IsValidTarget(Q.Range) && Q.GetPrediction(x).Hitchance >= HitChance.Medium && Q.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                    if (qTarget != null && Q.IsReady()) { Q.Cast(qTarget); }
                }
                if (W.IsReady())
                {
                    var wTarget = MinionTarget.Where(x => x.IsValidTarget(W.Range) && W.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                    if (wTarget != null && W.IsReady()) { W.Cast(wTarget); }
                }
                if (E.IsReady())
                {
                    var eTarget = MinionTarget.Where(x => x.IsValidTarget(E.Range) && E.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                    if (eTarget != null && E.IsReady()) { E.Cast(eTarget); }
                }
            }
            /* lasthit
            if(_MainMenu.Item("LUseQ").GetValue<bool>() && ObjectManager.Player.ManaPercent > _MainMenu.Item("LManaRate").GetValue<Slider>().Value)
                {
                    var qTarget = MinionTarget.Where(x => x.IsValidTarget(Q.Range) && Q.GetPrediction(x).Hitchance >= HitChance.Medium && Q.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                    if (qTarget != null && Q.IsReady()) { Q.Cast(qTarget); }
                }
                if (_MainMenu.Item("LUseW").GetValue<bool>() && ObjectManager.Player.ManaPercent > _MainMenu.Item("LManaRate").GetValue<Slider>().Value)
                {
                    var wTarget = MinionTarget.Where(x => x.IsValidTarget(W.Range) && W.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                    if (wTarget != null && W.IsReady()) { W.Cast(wTarget); }
                }
                if (_MainMenu.Item("LUseE").GetValue<bool>() && ObjectManager.Player.ManaPercent > _MainMenu.Item("LManaRate").GetValue<Slider>().Value)
                {
                    var eTarget = MinionTarget.Where(x => x.IsValidTarget(E.Range) && E.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                    if (eTarget != null && E.IsReady()) { E.Cast(eTarget); }
                }
            */
        }
    }
}