using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Skarner// By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 350f, TargetSelector.DamageType.Physical){Delay = 0.25f};
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 350f, TargetSelector.DamageType.Magical);
            
            R.SetTargetted(0.5f, float.MaxValue);
            E.SetSkillshot(0.25f, 70f, 1500f, false, SkillshotType.SkillshotLine); //

            AIO_Menu.Champion.Flee.addUseW();
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("R Cast", new KeyBind('T', KeyBindType.Press));


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
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
                W.Shield();
                AIO_Func.SC(E,0f);
            if(AIO_Menu.Champion.Combo.getKeyBindValue("R Cast").Active)
                AIO_Func.SC(R);
            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(Q);
            }
            
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee && AIO_Menu.Champion.Flee.UseW)
            {
                var Wtarget = TargetSelector.GetTarget(1000f, Q.DamageType, true); 
                if(Wtarget != null)
                W.Cast();
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();

        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender) && E.IsReady())
                E.Cast(gapcloser.Sender);
        }

        static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            return;
            if (args.Slot == SpellSlot.Q && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                if(Items.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only) && Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
                Items.UseItem((int)ItemId.Ravenous_Hydra_Melee_Only);
                if(Items.HasItem((int)ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
                Items.UseItem((int)ItemId.Tiamat_Melee_Only);
            }
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                Q.Cast();
            }
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                E.LCast(target,0f);
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
                
            if(enemy.InAARange())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
