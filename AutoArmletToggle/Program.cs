// This is Auto Armlet Toggle for Dota2 Ensage coded by aranor104 using Zynox's "aegis snatcher" as a base since i'm still learning.



using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

namespace Snatcher
{
    class Program
    {
        const int WM_KEYUP = 0x0101;

        private static int _sleeptick = 0;
        private static int _sleeptick2 = 0;
        private static Font _text;
        private static bool _enabled = true;

        static void Main(string[] args)
        {
            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = 13,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != WM_KEYUP || args.WParam != 'P' || Game.IsChatOpen)
                return;
            _enabled = !_enabled;
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            _text.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame )
                return;

            var player = ObjectMgr.LocalPlayer;
            if( player == null || player.Team == Team.Observer)
                return;

            if (_enabled)
                _text.DrawText(null, "Auto Armlet Toggle: Enabled, \"P\" for toggle.", 5, 64, Color.White);
            else
                _text.DrawText(null, "Auto Armlet Toggle: Disabled, \"P\" for toggle.", 5, 64, Color.White);
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            _text.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            _text.OnLostDevice();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var tick = Environment.TickCount;
            var myhero = ObjectMgr.LocalHero;
            var armlet = myhero.FindItem("item_armlet");

            if (myhero == null || tick < _sleeptick || !_enabled || myhero.ClassID != ClassID.CDOTA_Item_Armlet || tick < _sleeptick2)
                return;


            if ((health - myhero.Health) > 200)// If myhero's health decreased by 200 then turn armlet on
            {
                armlet.UseAbility(hero);
                _sleeptick2 = tick + 15000; // Check after 30 sec if myhero still losing health
                return;
            }
            var health = myhero.Health;

            else if(myhero.Health == health)
            {
                armlet.UseAbility(hero);
                return;
            }
        }

        private static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
