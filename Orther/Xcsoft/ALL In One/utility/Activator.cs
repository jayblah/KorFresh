using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.utility
{
    class Activator
    {
        //아이템 목록 && 설명
        //http://lol.inven.co.kr/dataninfo/item/list.php
        //http://www.lolking.net/items/
        //https://mirror.enha.kr/wiki/%EB%A6%AC%EA%B7%B8%20%EC%98%A4%EB%B8%8C%20%EB%A0%88%EC%A0%84%EB%93%9C/%EA%B3%B5%EA%B2%A9%20%EC%95%84%EC%9D%B4%ED%85%9C
        //http://leagueoflegends.wikia.com/wiki/Category:Items

        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        internal static Menu Menu { get { return AIO_Menu.MainMenu_Manual.SubMenu("Activator"); } }

        internal static void Load()
        {
            AIO_Menu.addSubMenu("Activator", "AIO: Activator");

            Menu.AddSubMenu(new Menu("Auto-Potion", "AutoPotion"));
            Menu.AddSubMenu(new Menu("BeforeAttack", "BeforeAttack"));
            Menu.AddSubMenu(new Menu("AfterAttack", "AfterAttack"));
            Menu.AddSubMenu(new Menu("OnAttack", "OnAttack"));
            Menu.AddSubMenu(new Menu("Killsteal", "Killsteal"));
            Menu.AddSubMenu(new Menu("Misc", "Misc"));

            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.Use Health Potion", "Use Health Potion")).SetValue(true);
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.ifHealthPercent", "if Health Percent <")).SetValue(new Slider(55, 0, 100));
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.Use Mana Potion", "Use Mana Potion")).SetValue(true);
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.ifManaPercent", "if Mana Percent <")).SetValue(new Slider(55, 0, 100));

            Menu.SubMenu("OnAttack").AddItem(new MenuItem("OnAttack.RS", "Use Red Smite")).SetValue(true);
            Menu.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal.BS", "Blue Smite")).SetValue(true);

            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Cb", "On Combo")).SetValue(true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Hr", "On Harass")).SetValue(true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Lc", "On LaneClear")).SetValue(false);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Jc", "On JungleClear")).SetValue(true);

            additems();
            addPotions();
            AddOrderData();

            Game.OnUpdate += OnUpdate.Game_OnUpdate;
            Orbwalking.BeforeAttack += BeforeAttack.Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack.Orbwalking_AfterAttack;
            Orbwalking.OnAttack += OnAttack.Orbwalking_OnAttack;
            Spellbook.OnCastSpell += AfterAttack.Spellbook_OnCastSpell;
        }

        static void additems()
        {
            BeforeAttack.additem("Youmuu", (int)ItemId.Youmuus_Ghostblade, Orbwalking.GetRealAutoAttackRange(Player));

            AfterAttack.additem("Tiamat", (int)ItemId.Tiamat_Melee_Only, 400f);
            AfterAttack.additem("Hydra", (int)ItemId.Ravenous_Hydra_Melee_Only, 400f);
            AfterAttack.additem("Bilgewater", (int)ItemId.Bilgewater_Cutlass, 450f, true);
            AfterAttack.additem("BoTRK", (int)ItemId.Blade_of_the_Ruined_King, 450f, true);
        }

        static void AddOrderData()
        {
            AfterAttack.AddOrderData("MasterYi", SpellSlot.W, CastingOrder.ItemFirst);
            AfterAttack.AddOrderData("Talon", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Fiora", SpellSlot.E, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Jax", SpellSlot.W, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Rengar", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Renekton", SpellSlot.W, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Khazix", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Yorick", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("XinZhao", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Vi", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Nasus", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Mordekaiser", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("MonkeyKing", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Leona", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Kassadin", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Garen", SpellSlot.Q, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Darius", SpellSlot.W, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Blitzcrank", SpellSlot.E, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Sejuani", SpellSlot.W, CastingOrder.SpellFirst);
            AfterAttack.AddOrderData("Sejuani", SpellSlot.Q, CastingOrder.SpellFirst);
        }

        internal class item
        {
            internal string Name { get; set; }
            internal int Id { get; set; }
            internal float Range { get; set; }
            internal bool isTargeted { get; set; }
        }

        #region PotionManager
        static void addPotions()
        {
            potions = new List<Potion>
            {
                new Potion
                {
                    Name = "ItemCrystalFlask",
                    MinCharges = 1,
                    ItemId = (ItemId) 2041,
                    Priority = 1,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "RegenerationPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2003,
                    Priority = 2,
                    TypeList = new List<PotionType> {PotionType.Health}
                },
                new Potion
                {
                    Name = "ItemMiniRegenPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2010,
                    Priority = 4,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "FlaskOfCrystalWater",
                    MinCharges = 0,
                    ItemId = (ItemId) 2004,
                    Priority = 3,
                    TypeList = new List<PotionType> {PotionType.Mana}
                }
            };
        }

        //PotionManager part of Marksman
        static List<Potion> potions;

        enum PotionType
        {
            Health, Mana
        };

        class Potion
        {
            internal string Name { get; set; }
            internal int MinCharges { get; set; }
            internal ItemId ItemId { get; set; }
            internal int Priority { get; set; }
            internal List<PotionType> TypeList { get; set; }
        }

        static InventorySlot GetPotionSlot(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from item in Player.InventoryItems
                    where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                    select item).FirstOrDefault();
        }

        static bool IsBuffActive(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from buff in Player.Buffs
                    where buff.Name == potion.Name && buff.IsActive
                    select potion).Any();
        }
        #endregion

        internal class OnUpdate
        {
            internal static void Game_OnUpdate(EventArgs args)
            {
                if (Player.IsDead)
                    return;

                if (!Player.IsRecalling() && !Player.InFountain())
                {
                    if (Menu.Item("AutoPotion.Use Health Potion").GetValue<bool>())
                    {
                        if (Player.HealthPercent <= Menu.Item("AutoPotion.ifHealthPercent").GetValue<Slider>().Value)
                        {
                            var healthSlot = GetPotionSlot(PotionType.Health);

                            if (!IsBuffActive(PotionType.Health) && healthSlot != null)
                                Player.Spellbook.CastSpell(healthSlot.SpellSlot);
                        }
                    }

                    if (Menu.Item("AutoPotion.Use Mana Potion").GetValue<bool>())
                    {
                        if (Player.ManaPercent <= Menu.Item("AutoPotion.ifManaPercent").GetValue<Slider>().Value)
                        {
                            var manaSlot = GetPotionSlot(PotionType.Mana);

                            if (!IsBuffActive(PotionType.Mana) && manaSlot != null)
                                Player.Spellbook.CastSpell(manaSlot.SpellSlot);
                        }
                    }
                }

                #region RS
                if (Menu.Item("OnAttack.RS").GetValue<bool>())
                    OnAttack.setRSmiteSlot(); //Red Smite
                #endregion

                #region BS
                if (Menu.Item("Killsteal.BS").GetValue<bool>() && Killsteal.smiteSlot != SpellSlot.Unknown)
                {
                    Killsteal.setBSmiteSlot();

                    var ts = ObjectManager.Get<Obj_AI_Hero>().Where(f => !f.IsAlly && !f.IsDead && Player.Distance(f, false) <= Killsteal.smrange);
                    if (ts == null)
                        return;

                    float dmg = Killsteal.BSDamage();
                    foreach (var t in ts)
                    {
                        if (AIO_Func.isKillable(t, dmg))
                        {
                            if (Killsteal.smiteSlot.IsReady() && Killsteal.BS.Slot == Killsteal.smiteSlot)
                                Player.Spellbook.CastSpell(Killsteal.smiteSlot, t);
                            else
                                return;
                        }
                    }
                }
                #endregion
            }
        }

        internal class OnAttack
        {
            internal static Spell RS;
            internal static SpellSlot smiteSlot = SpellSlot.Unknown;
            internal static float smrange = 575f; //500f + player.width + target width. 대충 575f.. 정글몹에겐 700f 정도
            internal static void setRSmiteSlot()
            {
                foreach (var spell in Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase))) // Red Smite
                {
                    smiteSlot = spell.Slot;
                    RS = new Spell(smiteSlot, smrange);
                    return;
                }
            }

            internal static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
            {
                var Target = target as Obj_AI_Hero;

                if (!unit.IsMe || Target == null)
                    return;

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("OnAttack.RS").GetValue<bool>() && OnAttack.smiteSlot != SpellSlot.Unknown)
                {
                    if (smiteSlot.IsReady() && RS.Slot == smiteSlot)
                        Player.Spellbook.CastSpell(smiteSlot, Target);
                    else
                        return;
                }
            }
        }

        internal class Killsteal
        {
            internal static Spell BS;
            internal static SpellSlot smiteSlot = SpellSlot.Unknown;
            internal static float smrange = 575f; //500f + player.width + target width. 대충 575f.. 정글몹에겐 700f 정도

            internal static float BSDamage()
            {
                int lvl = Player.Level;
                int damage = (20 + 8 * lvl);
                return damage;
            }

            internal static void setBSmiteSlot()
            {
                foreach (var spell in Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase))) // Red Smite
                {
                    smiteSlot = spell.Slot;
                    BS = new Spell(smiteSlot, smrange);
                    return;
                }
            }
        }

        internal class BeforeAttack
        {
            internal static List<item> itemsList = new List<item>();

            internal static void additem(string itemName, int itemid, float itemRange, bool itemisTargeted = false)
            {
                itemsList.Add(new item { Name = itemName, Id = itemid, Range = itemRange, isTargeted = itemisTargeted });

                Menu.SubMenu("BeforeAttack").AddItem(new MenuItem("BeforeAttack.Use " + itemid.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
            {
                if (!args.Unit.IsMe || args.Target == null || args.Unit.IsDead || args.Target.IsDead || args.Target.Type != GameObjectType.obj_AI_Hero)
                    return;

                foreach (var item in BeforeAttack.itemsList.Where(x => Items.CanUseItem((int)x.Id) && args.Target.IsValidTarget(x.Range) && Menu.Item("BeforeAttack.Use " + x.Id.ToString()).GetValue<bool>()))
                {
                    if (Menu.Item("Misc.Cb").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    {
                        if (item.isTargeted)
                            Items.UseItem(item.Id, (Obj_AI_Base)args.Target);
                        else
                            Items.UseItem(item.Id);
                    }
                }
            }
        }

        internal enum CastingOrder
        {
            SpellFirst,
            ItemFirst
        }

        internal class AfterAttack
        {
            internal class OrderData
            {
                internal string ChampionName { get; set; }
                internal SpellSlot SpellSlot { get; set; }
                internal CastingOrder CastingOrder { get; set; }
            }

            internal static List<item> itemsList = new List<item>();
            internal static bool ALLCancelItemsAreCasted { get { return !AfterAttack.itemsList.Any(x => Items.CanUseItem((int)x.Id) && !x.isTargeted && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>()); } }

            static List<OrderData> OrderDataList = new List<OrderData>();

            internal static void AddOrderData(string championName, SpellSlot spellSlot, CastingOrder castingOrder)
            {
                OrderDataList.Add(new OrderData { ChampionName = championName, SpellSlot = spellSlot, CastingOrder = castingOrder });
            }

            internal static void additem(string itemName, int itemid, float itemRange, bool itemisTargeted = false)
            {
                itemsList.Add(new item { Name = itemName, Id = itemid, Range = itemRange, isTargeted = itemisTargeted });

                Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.Use " + itemid.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
            {
                if (!unit.IsMe || target == null || target.IsDead || unit.IsDead || (target.Type != GameObjectType.obj_AI_Minion && target.Type != GameObjectType.obj_AI_Hero))
                    return;

                var itemone = AfterAttack.itemsList.FirstOrDefault(x => Items.CanUseItem((int)x.Id) && target.IsValidTarget(x.Range) && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>());

                if (itemone != null)
                {
                    var Minions = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Enemy);
                    var Mobs = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Neutral);

                    if ((Menu.Item("Misc.Cb").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) || (Menu.Item("Misc.Hr").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) || (Menu.Item("Misc.Jc").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Mobs.Count > 0) || (Menu.Item("Misc.Lc").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Minions.Count > 0))
                    {
                        var orderdata = OrderDataList.Find(x => x.ChampionName == AIO_Func.ChampionName);

                        if (orderdata != null)
                        {
                            switch (orderdata.CastingOrder)
                            {
                                case CastingOrder.SpellFirst:
                                    if (!Player.Spellbook.GetSpell(orderdata.SpellSlot).IsReady() ||
                                    (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && (!Menu.Item("Combo.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true).GetValue<bool>() || Menu.Item("Combo.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true) == null) ||
                                    Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && (!Menu.Item("Harass.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true).GetValue<bool>() || Menu.Item("Harass.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true) == null) ||
                                    Minions.Count() > 0 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && (!Menu.Item("Laneclear.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true).GetValue<bool>() || Menu.Item("Laneclear.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true) == null) ||
                                    Mobs.Count() > 0 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && (!Menu.Item("Jungleclear.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true).GetValue<bool>() || Menu.Item("Jungleclear.Use " + Player.Spellbook.GetSpell(orderdata.SpellSlot).Slot.ToString(), true) == null)))
                                    {
                                        if (itemone.isTargeted)
                                            Items.UseItem(itemone.Id, (Obj_AI_Base)target);
                                        else
                                            Items.UseItem(itemone.Id);
                                    }
                                    break;
                                case CastingOrder.ItemFirst:
                                    if (itemone.isTargeted)
                                        Items.UseItem(itemone.Id, (Obj_AI_Base)target);
                                    else
                                        Items.UseItem(itemone.Id);
                                    break;
                            }
                        }
                        else
                        {
                            if (itemone.isTargeted)
                                Items.UseItem(itemone.Id, (Obj_AI_Base)target);
                            else
                                Items.UseItem(itemone.Id);
                        }
                    }
                }
            }

            internal static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
            {
                var orderdata = OrderDataList.Find(x => x.ChampionName == AIO_Func.ChampionName);

                if (orderdata == null || orderdata.CastingOrder != CastingOrder.ItemFirst)
                    return;

                if (args.Slot == orderdata.SpellSlot && !ALLCancelItemsAreCasted)
                    args.Process = false;
            }
        }

        public static float getItemDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
                damage += (float)Player.GetItemDamage2(enemy, Damage.DamageItems.Tiamat);

            if (Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
                damage += (float)Player.GetItemDamage2(enemy, Damage.DamageItems.Hydra);

            if (Items.CanUseItem((int)ItemId.Bilgewater_Cutlass))
                damage += (float)Player.GetItemDamage2(enemy, Damage.DamageItems.Bilgewater);

            if (Items.CanUseItem((int)ItemId.Blade_of_the_Ruined_King))
                damage += (float)Player.GetItemDamage2(enemy, Damage.DamageItems.Botrk);

            return damage;
        }
    }
}
