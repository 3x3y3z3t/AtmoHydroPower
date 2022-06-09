// ;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;

namespace AtmoHydroPower
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class AtmoHydroPower_Session : MySessionComponentBase
    {
        private static MyDefinitionId s_PowerCellDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Component/PowerCell");

        private Dictionary<string, List<MyCubeBlockDefinition.Component>> m_ModdedComponentLists = null;

        public override void LoadData()
        {
            Logger.Init();
            Logger.LogLevel = 3;

            Logger.Log("Session Comp LoadData()..");
            if (MyAPIGateway.Session == null)
            {
                Logger.Log("  Session is null");
                return;
            }

            List<MyObjectBuilder_Checkpoint.ModItem> modList = MyAPIGateway.Session.Mods;
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
            else
            {
                m_ModdedComponentLists = new Dictionary<string, List<MyCubeBlockDefinition.Component>>();
                if (Config.s_IsReplaceModPresent)
                    ModifyVanillaBlockDefinition();

                ModifyModdedBlockDefinition();
            }




            Logger.Log("  LoadData done.");
        }
        
        protected override void UnloadData()
        {
            if (m_ModdedComponentLists != null)
                RestoreBlockDefinition();

            Logger.Close();
        }

        private void ModifyVanillaBlockDefinition()
        {
            Logger.Log("  Adding Power Cell Component to vanilla block definitions...");

            var powerCellCompDefId = MyDefinitionManager.Static.GetComponentDefinition(s_PowerCellDefinitionId);
            var powerCellItemDefId = MyDefinitionManager.Static.GetPhysicalItemDefinition(s_PowerCellDefinitionId);
            foreach (string subtypeId in Constants.s_VanillaSubtypeIds)
            {
                var definition = MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), subtypeId));
                if (definition == null)
                {
                    Logger.Log("    CubeBlock " + subtypeId + " not found");
                    continue;
                }

                MyCubeBlockDefinition.Component comp = new MyCubeBlockDefinition.Component()
                {
                    Definition = powerCellCompDefId,
                    Count = ComputeComponentCount(subtypeId),
                    DeconstructItem = powerCellItemDefId
                };

                List<MyCubeBlockDefinition.Component> components = new List<MyCubeBlockDefinition.Component>() { comp };
                components.AddRange(definition.Components);
                definition.Components = components.ToArray();
                m_ModdedComponentLists.Add(subtypeId, components);

                Logger.Log("    Added " + comp.Count + " Power Cell to CubeBlock " + subtypeId);
            }
        }

        private void ModifyModdedBlockDefinition()
        {
            Logger.Log("  Adding Power Cell Component to modded block definitions...");

            var powerCellCompDefId = MyDefinitionManager.Static.GetComponentDefinition(s_PowerCellDefinitionId);
            var powerCellItemDefId = MyDefinitionManager.Static.GetPhysicalItemDefinition(s_PowerCellDefinitionId);
            foreach (string subtypeId in Constants.s_ModSubtypeIds)
            {
                var definition = MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), subtypeId));
                if (definition == null)
                {
                    Logger.Log("    CubeBlock " + subtypeId + " not found");
                    continue;
                }

                MyCubeBlockDefinition.Component comp = new MyCubeBlockDefinition.Component()
                {
                    Definition = powerCellCompDefId,
                    Count = ComputeComponentCount(subtypeId),
                    DeconstructItem = powerCellItemDefId
                };

                List<MyCubeBlockDefinition.Component> components = new List<MyCubeBlockDefinition.Component>() { comp };
                components.AddRange(definition.Components);
                definition.Components = components.ToArray();
                m_ModdedComponentLists.Add(subtypeId, components);

                Logger.Log("    Added " + comp.Count + " Power Cell to CubeBlock " + subtypeId);
            }
        }

        private void RestoreBlockDefinition()
        {
            foreach (string subtypeId in m_ModdedComponentLists.Keys)
            {
                var definition = MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), subtypeId));
                if (definition == null)
                { 
                    Logger.Log("    CubeBlock " + subtypeId + " not found (this should not happens)");
                    continue;
                }

                var components = m_ModdedComponentLists[subtypeId];
                components.RemoveAt(0);
                definition.Components = components.ToArray();
            }

            m_ModdedComponentLists.Clear();
        }

        private int ComputeComponentCount(string _subtypeId)
        {
            if (_subtypeId.Contains("LargeBlockLarge"))
                return 10;
            if (_subtypeId.Contains("LargeBlockSmall"))
                return 3;
            if (_subtypeId.Contains("SmallBlockLarge"))
                return 3;
            if (_subtypeId.Contains("SmallBlockSmall"))
                return 1;

            if (_subtypeId.Contains("Large_SciFiForced"))
                return 5;
            if (_subtypeId.Contains("Small_SciFiForced"))
                return 2;

            return 0;
        }
        
    }
}
