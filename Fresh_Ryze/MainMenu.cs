using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;


namespace FreshRyze
{
    public static class MainMenu
    {
        public static Menu _MainMenu;
        public static Orbwalking.Orbwalker _OrbWalker;

        public static void Initialize()
        {
            _MainMenu = new Menu("Ryze", "Ryze", true);

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
                Combo.AddItem(new MenuItem("ComboSelect", "Q is NoCollision").SetValue(true));                
            }
            _MainMenu.AddSubMenu(Combo);

            var Harass = new Menu("Harass", "Harass");
            {
                Harass.AddItem(new MenuItem("HUseQ", "Use Q").SetValue(true));
                Harass.AddItem(new MenuItem("HUseW", "Use W").SetValue(true));
                Harass.AddItem(new MenuItem("HUseE", "Use E").SetValue(true));                
                Harass.AddItem(new MenuItem("HManaRate", "Mana %").SetValue(new Slider(20)));
                Harass.AddItem(new MenuItem("AutoHarass", "Auto Harass").SetValue(false));
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
                JungleClear.AddItem(new MenuItem("JUse Q", "Use Q").SetValue(true));
                JungleClear.AddItem(new MenuItem("JUse W", "Use W").SetValue(true));
                JungleClear.AddItem(new MenuItem("JUse E", "Use E").SetValue(true));
                JungleClear.AddItem(new MenuItem("JManaRate", "Mana %").SetValue(new Slider(20)));
            }
            _MainMenu.AddSubMenu(JungleClear);

            var Misc = new Menu("Misc", "Misc");
            {
                Misc.AddItem(new MenuItem("AutoLasthit", "Auto LastHit with Spell Q,E").SetValue(false));
                Misc.AddItem(new MenuItem("WGap", "Auto W On GapClosers").SetValue(true));
            }
            _MainMenu.AddSubMenu(Misc);

            var Draw = new Menu("Draw", "Draw");
            {                
                Draw.AddItem(new MenuItem("QRange", "Q Range").SetValue(true));
                Draw.AddItem(new MenuItem("WRange", "W Range").SetValue(true));
                Draw.AddItem(new MenuItem("ERange", "E Range").SetValue(true));
                Draw.AddItem(new MenuItem("DisplayStack", "Display Stack").SetValue(true));
                Draw.AddItem(new MenuItem("DisplayTime", "Display Time").SetValue(true));
            }
            _MainMenu.AddSubMenu(Draw);

            _MainMenu.AddToMainMenu();                        
        }
    }
}
