using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Alistar
{
    class Program
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        //private static int LastLaugh;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E;
        private static Menu menu;
        private static string version = "1.5";

        private static float playerMaxHeal = Player.MaxHealth;
        private static int playerMaxHealINT = Convert.ToInt32(playerMaxHeal);

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Alistar")
                return;

            Q = new Spell(SpellSlot.Q, 365);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 555);


            
            menu = new Menu(Player.ChampionName + " ♥", Player.ChampionName, true);
            Menu orbwalkerMenu = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu ts = menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(ts);
            /*Menu spellMenu = menu.AddSubMenu(new Menu("Spells", "Spells"));

            spellMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            spellMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));*/

            Menu drawMenu = menu.AddSubMenu(new Menu("Draw", "Draw"));
            drawMenu.AddItem(new MenuItem("eDraw", "Draw E range").SetValue(true));

            Menu healMenu = menu.AddSubMenu(new Menu("Heal Options", "Heal Options"));
            healMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            healMenu.AddItem(new MenuItem("allyHeal", "Heal Ally").SetValue(true));
            healMenu.AddItem(new MenuItem("Minimal HP to Heal", "Minimal HP to Heal").SetValue(new Slider(200, 1, playerMaxHealINT)));

            //spellMenu.AddItem(new MenuItem("LaughButton", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            Drawing.OnDraw += Drawing_OnDraw;

            menu.AddToMainMenu();
            

            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat("<font color=\"#ff9e00\">[NECEK CARRY]</font> <font color=\"#00ff00\">Combo Alistar! Have fun! By necek123. (V{0})</font> [L#]", version);
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
                //combo();
            }

            /*if (menu.Item("LaughButton").GetValue<KeyBind>().Active)
            {
                if (Environment.TickCount > LastLaugh + 4200)
                {
                    LastLaugh = Environment.TickCount;
                }
            }*/
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (E.IsReady())
            {
                Utility.DrawCircle(Player.Position, E.Range, Color.Aqua);
            }
            else
            {
                Utility.DrawCircle(Player.Position, E.Range, Color.DarkRed);
            }
        }

        /// <summary>
        /// Consume logic
        /// </summary>

        private static void HealE()
        {
            if (!menu.Item("useE").GetValue<bool>())
                return;

            if (E.IsReady())
            {
                
                Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (Player.Health <= menu.Item("Minimal HP to Heal").GetValue<Slider>().Value)
                {
                    E.Cast();
                    Game.PrintChat(Player.Health.ToString() + " <= " + menu.Item("Minimal HP to Heal").GetValue<Slider>().Value);
                }

                if (menu.Item("allyHeal").GetValue<bool>())
                {
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
                    {
                        if (Player.HasBuff("Recall") || Utility.InFountain(Player)) return;
                        if ((hero.Health / hero.MaxHealth) * 100 <= menu.Item("Minimal HP to Heal").GetValue<Slider>().Value &&
                            E.IsReady() &&
                            hero.Distance(Player.ServerPosition) <= E.Range)
                            E.Cast(hero);
                    }
                }
                
            }
        }

    }
}
