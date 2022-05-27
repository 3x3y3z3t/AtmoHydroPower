// ;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;

namespace AtmoHydroPower
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class AtmoHydroPower_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            Logger.Init();
        }

        public override void BeforeStart()
        {
            Logger.Log("Session Comp BeforeStart()");

            List<MyObjectBuilder_Checkpoint.ModItem> modList = MyAPIGateway.Session?.Mods;
            if (modList == null)
            {
                Logger.Log("  Mod list is null");
                return;
            }

            foreach (MyObjectBuilder_Checkpoint.ModItem mod in modList)
            {
                if (mod.PublishedFileId == 2806398181)
                {
                    Config.s_IsReplaceModPresent = true;
                    Logger.Log("  Atmo Hydro thruster mod present");
                }
                else if (mod.PublishedFileId == 2807922557)
                {
                    Config.s_IsStandaloneModPresent = true;
                    Logger.Log("  Atmo Hydro thruster (standalone) mod present");
                }

                if (Config.s_IsReplaceModPresent && Config.s_IsStandaloneModPresent)
                    break;
            }

            if (Config.s_IsReplaceModPresent && Config.s_IsStandaloneModPresent)
            {
                Logger.Log("  Both Atmo Hydro thruster and its standalone mod is present, which may cause conflict");
                Logger.Log("    > This mod will be disabled");
            }
        }

        protected override void UnloadData()
        {
            Logger.Close();
        }
        
    }
}
