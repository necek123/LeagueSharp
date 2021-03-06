﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Alistar
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        //private static int LastLaugh;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E;
        private static SpellSlot ignite;
        private static Menu menu;
        private static string version = "1.2";

        private static float playerMaxHeal = Player.MaxHealth;
        private static int playerMaxHealINT = Convert.ToInt32(playerMaxHeal);

        private static float playerMaxMana = Player.MaxMana;
        private static int playerMaxManaINT = Convert.ToInt32(playerMaxMana);

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Alistar")
                return;

            Q = new Spell(SpellSlot.Q, 350);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 535);
            
            menu = new Menu(Player.ChampionName + " ♥", Player.ChampionName, true);
            Menu orbwalkerMenu = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu ts = menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(ts);
            Menu spellMenu = menu.AddSubMenu(new Menu("Spells", "Spells"));

            spellMenu.AddItem(new MenuItem("useIgnite", "Use ignite").SetValue(true));
            /*spellMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            spellMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));*/

            Menu drawMenu = menu.AddSubMenu(new Menu("Draw", "Draw"));
            drawMenu.AddItem(new MenuItem("qDraw", "Draw Q range")).SetValue(new Circle(false, Color.FromArgb(150, Color.White)));
            drawMenu.AddItem(new MenuItem("wDraw", "Draw W range")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            drawMenu.AddItem(new MenuItem("eDraw", "Draw E range")).SetValue(new Circle(false, Color.FromArgb(150, Color.White)));
            drawMenu.AddItem(new MenuItem("debugDraw", "Debug Heal HP").SetValue(false));

            Menu healMenu = menu.AddSubMenu(new Menu("Heal Options", "Heal Options"));
            healMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            healMenu.AddItem(new MenuItem("allyHeal", "Heal Ally").SetValue(true));
            healMenu.AddItem(new MenuItem("Minimal HP to Heal", "Minimal HP to Heal").SetValue(new Slider(150, 1, playerMaxHealINT)));
            healMenu.AddItem(new MenuItem("Minimal Mana to Heal", "Minimal Mana to Heal").SetValue(new Slider(150, 1, playerMaxManaINT)));

            Menu miscMenu = menu.AddSubMenu(new Menu("Misc", "Misc"));
            miscMenu.AddItem(new MenuItem("Say GL HF", "Say GL HF").SetValue<bool>(true));

            //spellMenu.AddItem(new MenuItem("LaughButton", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            Drawing.OnDraw += Drawing_OnDraw;

            menu.AddToMainMenu();
            

            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat("<font color=\"#ff9e00\">[NECEK CARRY]</font> <font color=\"#00ff00\">Combo Alistar! Have fun! By necek123. (V{0})</font> [L#]", version);
           
            if (menu.Item("Say GL HF").GetValue<bool>())
            {
                Random rnd = new Random();
                int secondFromStart = rnd.Next(15, 28);
                Utility.DelayAction.Add(secondFromStart * 1000, () => Game.Say("/all Good Luck, Have fun guys! :D"));
            }
        
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead)
                return;

            //===================================================
            playerMaxHealINT = Convert.ToInt32(playerMaxHeal);
            HealE();
            //===================================================

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                combo();
            }

            /*if (menu.Item("LaughButton").GetValue<KeyBind>().Active)
            {
                if (Environment.TickCount > LastLaugh + 4200)
                {
                    LastLaugh = Environment.TickCount; ss
                }
            }*/
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var qcircle = menu.Item("qDraw").GetValue<Circle>();
            var wcircle = menu.Item("wDraw").GetValue<Circle>();
            var ecircle = menu.Item("eDraw").GetValue<Circle>();

            if (menu.Item("debugDraw").GetValue<bool>())
            {
                Drawing.DrawText(Drawing.WorldToScreen(Player.Position)[0], Drawing.WorldToScreen(Player.Position)[1], Color.Orange, Player.Health.ToString() + " < " + menu.Item("Minimal HP to Heal").GetValue<Slider>().Value);
            }

            if(ecircle.Active && E.Level > 0)
            {
                Utility.DrawCircle(Player.Position, E.Range, ecircle.Color);
            }
            
            if(wcircle.Active && W.Level > 0)
            {
                Utility.DrawCircle(Player.Position, W.Range, wcircle.Color);
            }

            if(qcircle.Active && Q.Level > 0)
            {
                Utility.DrawCircle(Player.Position, Q.Range, qcircle.Color);
            }

        }

        //================================================================================

        private static void combo()
        {
            var targetEnemy = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if(targetEnemy == null || !targetEnemy.IsValid)
                return;

            SpellDataInst manaQ = Player.Spellbook.GetSpell(SpellSlot.Q);
            SpellDataInst manaW = Player.Spellbook.GetSpell(SpellSlot.W);

            if (Q.IsReady() && manaQ.ManaCost <= Player.Mana)
            {
                if (Player.Distance((AttackableUnit)targetEnemy) < Q.Range)
                {
                    Q.Cast();
                }
                else if (Q.IsReady() && W.IsReady() && manaQ.ManaCost + manaW.ManaCost <= Player.Mana)
                {
                    W.CastOnUnit(targetEnemy);
                    if (Player.Distance((AttackableUnit)targetEnemy) < Q.Range)
                    {
                        Q.Cast();
                    }
                }
            }

            if(W.IsReady() && W.IsKillable(targetEnemy, 1) && Player.Distance((AttackableUnit)targetEnemy) < W.Range)
            {
                W.CastOnUnit(targetEnemy);
            }

            if(Player.Distance((AttackableUnit)targetEnemy) <= 600 && IgniteDamage(targetEnemy) >= targetEnemy.Health && menu.Item("useIgnite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(ignite, targetEnemy);
            }

        }


        private static void HealE()
        {
            if (!menu.Item("useE").GetValue<bool>())
                return;

            if (E.IsReady())
            {
                
                Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (Player.Health <= menu.Item("Minimal HP to Heal").GetValue<Slider>().Value && Player.Mana >= menu.Item("Minimal Mana to Heal").GetValue<Slider>().Value)
                {
                    E.Cast();
                    
                }

                if (menu.Item("allyHeal").GetValue<bool>())
                {
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
                    {
                        if (Player.HasBuff("Recall") || Utility.InFountain(Player)) return;
                        if ((hero.Health / hero.MaxHealth) * 100 <= menu.Item("Minimal HP to Heal").GetValue<Slider>().Value &&
                            E.IsReady() &&
                            hero.Distance(Player.ServerPosition) <= E.Range && Player.Mana >= menu.Item("Minimal Mana to Heal").GetValue<Slider>().Value)
                            E.Cast(hero);
                    }
                }
                
            }
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if(ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }
    }
}
