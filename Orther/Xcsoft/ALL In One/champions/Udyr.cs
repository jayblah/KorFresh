using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Udyr// By RL244 UdyrTurtleStance udyrturtleactivation UdyrPhoenixStance (이거 갯수 셀수있음)  udyrphoenixactivation UdyrBearStance udyrbearactivation UdyrTigerStance udyrtigerpunch udyrmonkeyagilitybuff udyrbearstuncheck(Target)
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            
            AIO_Menu.Champion.Flee.addUseE();
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();


            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addUseR();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addUseR();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addUseR();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
                var EEtarget = TargetSelector.GetTarget(600f, Q.DamageType, true); 

                W.Shield();
                if(EEtarget != null)
                {
                    if(!EEtarget.HasBuff("udyrbearstuncheck"))
                    AIO_Func.SC(E);
                    else
                    {
                        AIO_Func.SC(Q);
                        AIO_Func.SC(R);
                    }
                }

            
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee && AIO_Menu.Champion.Flee.UseE)
            {
                var Etarget = TargetSelector.GetTarget(1000f, Q.DamageType, true); 
                if(Etarget != null)
                E.Cast();
            }


        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

        }

        static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            return;
            if ((args.Slot == SpellSlot.Q || args.Slot == SpellSlot.R) && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                if(Items.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only) && Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
                Items.UseItem((int)ItemId.Ravenous_Hydra_Melee_Only);
                if(Items.HasItem((int)ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
                Items.UseItem((int)ItemId.Tiamat_Melee_Only);
            }
        }

        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy) * 5;
                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
