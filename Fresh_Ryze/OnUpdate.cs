using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FreshRyze
{
    public static class OnUpdate
    {        
        public static void OnGameUpdate(EventArgs args)
        {
            
            var QTarget = TargetSelector.GetTarget(Program.Q.Range, TargetSelector.DamageType.Magical);
            var WTarget = TargetSelector.GetTarget(Program.W.Range, TargetSelector.DamageType.Magical);
            var ETarget = TargetSelector.GetTarget(Program.E.Range, TargetSelector.DamageType.Magical);
            var RyzeStack = 0;

            var RyzePassive = ObjectManager.Player.Buffs.Find(DrawFX => DrawFX.Name == "ryzepassivestack" && DrawFX.IsValidBuff()); // 라이즈 스택
            if(RyzePassive != null){RyzeStack = RyzePassive.Count;} else{RyzeStack = 0;}
            var recall = ObjectManager.Player.Buffs.Find(x => x.Name == "recall" && x.IsValidBuff());


            if (MainMenu._OrbWalker.ActiveMode == LeagueSharp.Common.Orbwalking.OrbwalkingMode.Combo)   // Combo Mode
            {                
                if (MainMenu._MainMenu.Item("UseR").GetValue<bool>() && Program.R.IsReady() && (Program.Q.IsReady() || Program.E.IsReady()) && RyzeStack > 2 && WTarget != null)
                {
                    Program.R.Cast();
                }
                if (MainMenu._MainMenu.Item("UseQ").GetValue<bool>() && Program.Q.IsReady() && QTarget != null)
                {
                    Program.Q.Cast(QTarget);
                }
                if (MainMenu._MainMenu.Item("UseW").GetValue<bool>() && Program.W.IsReady() && WTarget != null)
                {
                    Program.W.Cast(WTarget);
                }
                if (MainMenu._MainMenu.Item("UseE").GetValue<bool>() && Program.E.IsReady() && ETarget != null)
                {
                    Program.E.Cast(ETarget);
                }
            }

            if((MainMenu._OrbWalker.ActiveMode == LeagueSharp.Common.Orbwalking.OrbwalkingMode.Mixed || MainMenu._MainMenu.Item("AutoHarass").GetValue<bool>()) 
                && MainMenu._MainMenu.Item("HManaRate").GetValue<Slider>().Value < ObjectManager.Player.ManaPercent)    // Harass
            {
                if (MainMenu._MainMenu.Item("HUseQ").GetValue<bool>() && Program.Q.IsReady() && QTarget != null)
                {
                    Program.Q.Cast(QTarget);
                }
                if (MainMenu._MainMenu.Item("HUseW").GetValue<bool>() && Program.W.IsReady() && WTarget != null)
                {
                    Program.W.Cast(WTarget);
                }
                if (MainMenu._MainMenu.Item("HUseE").GetValue<bool>() && Program.E.IsReady() && ETarget != null)
                {
                    Program.E.Cast(ETarget);
                }
            }

            if(MainMenu._OrbWalker.ActiveMode == LeagueSharp.Common.Orbwalking.OrbwalkingMode.LaneClear)    // LaneClear
            {
                var MinionTarget = MinionManager.GetMinions(Program.Q.Range, MinionTypes.All, MinionTeam.Enemy);
                var JungleTarget = MinionManager.GetMinions(Program.Q.Range, MinionTypes.All, MinionTeam.Neutral);
                
                if(MinionTarget != null && MainMenu._MainMenu.Item("LManaRate").GetValue<Slider>().Value < Program.Player.ManaPercent)    // 미니언 있을시
                {
                    foreach(var minion in MinionTarget)
                    {
                        if(MainMenu._MainMenu.Item("LUseQ").GetValue<bool>() && Program.Q.IsReady())
                        {
                            Program.Q.CastIfHitchanceEquals(minion, HitChance.VeryHigh, true);
                        }
                        if (MainMenu._MainMenu.Item("LUseW").GetValue<bool>() && Program.W.IsReady())
                        {
                            Program.W.Cast(minion, true);
                        }
                        if (MainMenu._MainMenu.Item("LUseE").GetValue<bool>() && Program.E.IsReady())
                        {
                            Program.E.Cast(minion, true);
                        }
                    }
                }

                if(JungleTarget != null && MainMenu._MainMenu.Item("JManaRate").GetValue<Slider>().Value < Program.Player.ManaPercent)    // 정글몹 있을시
                {
                    foreach(var jungle in JungleTarget)
                    {
                        if (MainMenu._MainMenu.Item("JUseQ").GetValue<bool>() && Program.Q.IsReady())
                        {
                            Program.Q.CastIfHitchanceEquals(jungle, HitChance.VeryHigh, true);
                        }
                        if (MainMenu._MainMenu.Item("JUseW").GetValue<bool>() && Program.W.IsReady())
                        {
                            Program.W.Cast(jungle, true);
                        }
                        if (MainMenu._MainMenu.Item("JUseE").GetValue<bool>() && Program.W.IsReady())
                        {
                            Program.E.Cast(jungle, true);
                        }
                    }
                }
            }

            if (MainMenu._MainMenu.Item("AutoLasthit").GetValue<bool>() && recall == null && RyzePassive.Count < 4 && (Program.Q.IsReady() || Program.E.IsReady()))
            {
                var MinionTarget = MinionManager.GetMinions(Program.Q.Range, MinionTypes.All, MinionTeam.Enemy);
                var minion_QTarget = MinionTarget.Where(x => x.IsValidTarget(Program.Q.Range) && Program.Q.GetPrediction(x).Hitchance >= HitChance.Medium && Program.Q.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                var minion_ETarget = MinionTarget.Where(x => x.IsValidTarget(Program.E.Range) && Program.E.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                if(Program.Q.IsReady() && minion_QTarget != null)
                {
                    Program.Q.Cast(minion_QTarget);
                }
                if (Program.E.IsReady() && minion_ETarget != null)
                {
                    Program.E.Cast(minion_ETarget);
                }
            }
        }
    }
}
