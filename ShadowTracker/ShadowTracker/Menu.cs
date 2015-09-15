using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShadowTracker
{
    class MainMenu
    {
        public static Menu _MainMenu;

        public static void Initialize()
        {
            _MainMenu = new Menu("ShadowTracker", "ShadowTracker", true);
            var Draw = new Menu("Draw", "Draw");
            {
                Draw.AddItem(new MenuItem("Spell", "Spell").SetValue(true));
                Draw.AddItem(new MenuItem("Skill", "Skill").SetValue(true));
                Draw.AddItem(new MenuItem("ItsMe", "My Locate").SetValue(new StringList(new[] {"Always", "Combo/Harass", "None"},0)));
            }
            _MainMenu.AddSubMenu(Draw);            

            _MainMenu.AddToMainMenu();
        }
    }
}
