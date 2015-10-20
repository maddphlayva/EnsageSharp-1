using System;
using System.Linq;
using System.Collections.Generic;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX.Direct3D9;
using System.Windows.Input;

namespace ApproxES
{
    internal class Program
    {
        private static bool activated;
        private static bool toggle = true;
        private static Font txt;
        private static Font not;
        private static Key KeyCombo = Key.Space;
        private static bool loaded;
        private static Hero me;
        private static Hero target;
        private static ParticleEffect rangeDisplay;
        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("> ApproxES loaded!");

            txt = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 12,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            not = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 20,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectMgr.LocalHero;
            if (!Game.IsInGame || me.ClassID != ClassID.CDOTA_Unit_Hero_Earthshaker)
            {
                if (rangeDisplay == null)
                {
                    return;
                }
                rangeDisplay = null;
                return;
            }

            var blink = me.FindItem("item_blink");
            var W = me.Spellbook.SpellW;
            var R = me.Spellbook.SpellR;
            var Q = me.Spellbook.SpellQ;
            var dagon = me.GetDagon();
            var shiva = me.FindItem("item_shivas_guard");
            var arcane = me.FindItem("item_arcane_boots");
            var wand = me.FindItem("item_magic_wand");
            var stick = me.FindItem("item_magic_stick");
            var cheese = me.FindItem("item_cheese");
            var veilofdiscord = me.FindItem("item_veil_of_discord");
            var range = 1400;
            var halfhealth = me.MaximumHealth/2;           

            var enemyHeroes = ObjectMgr.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune) &&
                   e.ClassID != ClassID.CDOTA_Unit_Hero_Beastmaster_Hawk &&
                   e.ClassID != ClassID.CDOTA_Unit_Hero_Beastmaster_Boar &&
                   e.ClassID != ClassID.CDOTA_Unit_Hero_Beastmaster_Beasts &&
                   e.ClassID != ClassID.CDOTA_Unit_Brewmaster_PrimalEarth &&
                   e.ClassID != ClassID.CDOTA_Unit_Brewmaster_PrimalFire &&
                   e.ClassID != ClassID.CDOTA_Unit_Brewmaster_PrimalStorm &&
                   e.ClassID != ClassID.CDOTA_Unit_Undying_Tombstone &&
                   e.ClassID != ClassID.CDOTA_Unit_Undying_Zombie &&
                   e.ClassID != ClassID.CDOTA_Ability_Juggernaut_HealingWard).ToList();          

            if (rangeDisplay == null)
            {
                rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                rangeDisplay.SetControlPoint(1, new Vector3(range, 0, 0));
            }
            else
            {
                rangeDisplay.Dispose();
                rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                rangeDisplay.SetControlPoint(1, new Vector3(range, 0, 0));
            }
            

            if (activated && toggle)
            {
                var target = me.ClosestToMouseTarget(2000);
                var ehalfhealth = target.MaximumHealth / 2; 
                var thalfhealth = target.MaximumHealth / 3; 
                if (target.IsAlive && !target.IsInvul())
                {
                    if (target != null && target.IsAlive && target.IsVisible && me.Distance2D(target) <= 1200 && !target.IsIllusion)
                    {
                        me.Attack(target);
                        if (R.CanBeCasted() &&
                            blink.CanBeCasted() &&
                            me.Position.Distance2D(target.Position) > 300 &&
                            Utils.SleepCheck("blink"))
                        {
                            blink.UseAbility(target.Position);
                            Utils.Sleep(250, "blink");
                            me.Attack(target); 
                        }
                        if (R.CanBeCasted() &&
                            me.Position.Distance2D(target.Position) < 575 &&
                            Utils.SleepCheck("R") && ((enemyHeroes.Count(x => x.Distance2D(me) <= 575) > 1) || (me.Health <= halfhealth) && (target.Health >= thalfhealth) || target.Health >= ehalfhealth))
                        {
                            {
                                blink.UseAbility(target.Position);
                                Utils.Sleep(250, "blink");
                                R.UseAbility();
                                me.Attack(target);
                            }
                        }                                               
                        if (W.CanBeCasted() &&
                            blink.CanBeCasted() &&
                            me.Position.Distance2D(target.Position) > 300 &&
                            Utils.SleepCheck("blink"))
                        {
                            blink.UseAbility(target.Position);
                            Utils.Sleep(250, "blink");
                            me.Attack(target);
                        }
                        if (W.CanBeCasted() &&
                            me.Position.Distance2D(target.Position) < 200 &&
                            Utils.SleepCheck("W") && !target.IsMagicImmune())
                        {
                            W.UseAbility();
                            Utils.Sleep(150, "W");
                            me.Attack(target);
                        }

                        if (// Arcane Boots Item
                            arcane != null &&
                            me.Mana <= W.ManaCost &&
                            arcane.CanBeCasted())
                        {
                            arcane.UseAbility();
                        } // Arcane Boots Item end

                        if (// Shiva Item
                            shiva != null &&
                            shiva.CanBeCasted() &&
                            me.CanCast() &&
                            !target.IsMagicImmune() &&
                            (shiva.CanBeCasted() &&
                            Utils.SleepCheck("shiva") &&
                            me.Distance2D(target) <= 600)
                            )
                        {
                            shiva.UseAbility();
                            Utils.Sleep(250 + Game.Ping, "shiva");
                        } // Shiva Item end

                        if (// Dagon
                            dagon != null &&
                            dagon.CanBeCasted() &&
                            me.CanCast() &&
                            !target.IsMagicImmune() &&
                            (dagon.CanBeCasted() &&
                            Utils.SleepCheck("dagon"))
                           )
                        {
                            dagon.UseAbility(target);
                            Utils.Sleep(150 + Game.Ping, "dagon");
                        } // Dagon Item end

                        if (
                            veilofdiscord != null &&
                            veilofdiscord.CanBeCasted() &&
                            me.CanCast() &&
                            !target.IsMagicImmune() &&
                            (veilofdiscord.CanBeCasted() &&
                            Utils.SleepCheck("veilofdiscord"))
                           )
                        {
                            veilofdiscord.UseAbility(target.Position);
                            Utils.Sleep(150 + Game.Ping, "veilofdiscord");
                        }

                        if (
                            // Stick
                            (stick != null && stick.CanBeCasted()) ||
                            (wand != null && wand.CanBeCasted()) ||
                            (cheese != null && cheese.CanBeCasted()) &&
                            Utils.SleepCheck("stick") &&
                            me.Distance2D(target) <= 700)
                        {
                            stick.UseAbility();
                            wand.UseAbility();
                            cheese.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "stick");
                        } // Stick Item end
                    }
                }
            }
        }




        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
                if (Game.IsKeyDown(KeyCombo))
                {
                    activated = true;
                }
                else
                {
                    activated = false;
                }



            }
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            txt.Dispose();
            not.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectMgr.LocalPlayer;
            var me = ObjectMgr.LocalHero;
            if (player == null || player.Team == Team.Observer || me.ClassID != ClassID.CDOTA_Unit_Hero_Earthshaker)
                return;

            if (activated)
            {
                txt.DrawText(null, "ApproxES: Comboing!", 4, 150, Color.Green);
            }

            if (!activated)
            {
                txt.DrawText(null, "ApproxES:  [" + KeyCombo + "] for toggle combo", 4, 150, Color.DarkRed);
            }


        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            txt.OnResetDevice();
            not.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            txt.OnLostDevice();
            not.OnLostDevice();
        }
    }
}
