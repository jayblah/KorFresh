using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    class ChampLoader
    {
        private static bool loaded = false;

        enum Developer
        {
            xcsoft,
            RL244,
            Fakker
        }

        /// <summary>
        /// 전용 챔피언의 클래스를 로드합니다.
        /// </summary>
        /// <param name="champName">로드할 클래스의 이름을 기입하십시오.</param>
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal static void Load(string champName)
        {
            var champions = Assembly.GetExecutingAssembly().GetTypes().Where(x =>
                    x.Namespace.Contains("champions") && x.IsClass);

            foreach (var champion in champions)
            {
                if (champion.Name == AIO_Func.ChampionName)
                {
                    GetCredit(CreditDictionary[champion.Name]);
                    champion.GetMethod("Load", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
                    loaded = true;
                }

                if (loaded)
                    break;
            }

            if (!loaded)
                AIO_Func.sendDebugMsg("(ChampLoader) Error");
        }

        private static Dictionary<string, Developer> CreditDictionary = new Dictionary<string, Developer>()
        {
            {"Aatrox", Developer.RL244},
            {"Ahri", Developer.RL244},
            {"Alistar", Developer.RL244},
            {"Amumu", Developer.xcsoft},
            {"Annie", Developer.xcsoft},
            {"Ashe", Developer.RL244},
            // Work In Progress
            // {"Azir", Developer.RL244},
            {"Blitzcrank", Developer.RL244},
            {"Brand", Developer.RL244},
            {"Braum", Developer.RL244},
            {"Caitlyn", Developer.RL244},
            {"Cassiopeia", Developer.RL244},
            {"Chogath", Developer.Fakker},
            {"Corki", Developer.RL244},
            {"Darius", Developer.RL244},
            {"Diana", Developer.RL244},
            {"DrMundo", Developer.RL244},
            // Work In Progress (Almost done)
            // {"Ekko", Developer.RL244},
            {"Evelynn", Developer.RL244},
            {"Ezreal", Developer.RL244},
            // Not listed originally. I don't know why not listed
            // {"Fiddlesticks", Developer.RL244},
            {"Fiora", Developer.RL244},
            {"Galio", Developer.RL244},
            {"Gangplank", Developer.RL244},
            {"Garen", Developer.xcsoft},
            {"Graves", Developer.Fakker},
            {"Janna", Developer.xcsoft},
            {"JarvanIV", Developer.RL244},
            {"Jax", Developer.RL244},
            {"Jayce", Developer.RL244},
            {"Jinx", Developer.xcsoft},
            {"Kalista", Developer.xcsoft},
            {"Karma", Developer.RL244}, // Work In Progress
            {"Karthus", Developer.xcsoft},
            {"Kassadin", Developer.RL244},
            {"Katarina", Developer.xcsoft},
            {"Kayle", Developer.RL244},
            {"Khazix", Developer.RL244},
            {"KogMaw", Developer.RL244},
            {"Leona", Developer.RL244},
            {"Lulu", Developer.xcsoft},
            {"Malzahar", Developer.RL244},
            {"MasterYi", Developer.xcsoft},
            {"MissFortune", Developer.RL244},
            {"MonkeyKing", Developer.xcsoft},
            {"Mordekaiser", Developer.RL244},
            {"Morgana", Developer.RL244},
            {"Nami", Developer.xcsoft},
            {"Nasus", Developer.RL244},
            {"Nautilus", Developer.xcsoft},
            {"Nocturne", Developer.RL244},
            {"Nunu", Developer.RL244},
            {"Olaf", Developer.RL244},
            {"Orianna", Developer.xcsoft},
            {"Pantheon", Developer.RL244},
            {"Quinn", Developer.RL244},
            {"Renekton", Developer.RL244}, // Work In Progress
            {"Rengar", Developer.RL244},
            // {"Riven", Developer.RL244},
            {"Ryze", Developer.xcsoft},
            {"Sejuani", Developer.RL244},
            {"Shen", Developer.xcsoft},
            {"Shyvana", Developer.RL244},
            {"Sion", Developer.xcsoft},
            {"Sivir", Developer.Fakker},
            {"Skarner", Developer.RL244},
            {"Sona", Developer.RL244},
            {"Soraka", Developer.RL244},
            {"Swain", Developer.RL244},
            {"Syndra", Developer.RL244}, // Work In Progress
            {"Talon", Developer.RL244},
            {"Taric", Developer.RL244},
            {"Teemo", Developer.RL244},
            // WIP 쓰래쉬.. 챔프가없엉..아놔.. 100개넘게 챔프있는 아이디에 없는챔이라니..
            // {"Thresh", Developer.RL244},
            {"Tristana", Developer.RL244},
            {"Trundle", Developer.RL244},
            {"Twitch", Developer.xcsoft},
            {"Udyr", Developer.RL244},
            {"Urgot", Developer.RL244},
            {"Varus", Developer.RL244},
            {"Vayne", Developer.RL244},
            {"Veigar", Developer.RL244},
            {"Vi", Developer.xcsoft},
            {"Viktor", Developer.RL244},
            {"Vladimir", Developer.xcsoft},
            {"Volibear", Developer.RL244},
            {"Warwick", Developer.RL244},
            {"Xerath", Developer.RL244},
            {"XinZhao", Developer.RL244},
            {"Yasuo", Developer.RL244},
            {"Yorick", Developer.RL244},
            {"Zed", Developer.xcsoft}, //(Incomplete)
            {"Zilean", Developer.RL244},
            {"Zyra", Developer.RL244}
        };

        private static void GetCredit(Developer Developer)
        {
            string creditMsg = AIO_Func.ChampionName + " Made By '" + Developer.ToString() + "'.";

            AIO_Func.sendDebugMsg(creditMsg);
            Notifications.AddNotification(creditMsg, 4000);
        }

        /// <summary>
        /// 플레이어의 챔피언의 이름과 같은 이름의 클래스가 있는지 확인합니다.
        /// </summary>
        /// <param name="checkNamespace">체크할 네임스페이스를 기입하십시오.</param>
        /// <returns>플레이어의 챔피언의 이름과 같은 이름의 클래스가 있으면 true, 없으면 false를 반환합니다.</returns>
        internal static bool champSupportedCheck()
        {
            string championName = AIO_Func.ChampionName;

            if (CreditDictionary.ContainsKey(championName))
            {
                string successMsg = championName + " is supported.";

                AIO_Func.sendDebugMsg(successMsg);
                Notifications.AddNotification(successMsg, 4000);
                return true;
            }
            else
            {
                string failedMsg = championName + " is not supported.";

                AIO_Func.sendDebugMsg(failedMsg);
                Notifications.AddNotification(failedMsg, 4000);

                AIO_Menu.addItem(failedMsg, null);
                return false;
            }
        }
    }
}
