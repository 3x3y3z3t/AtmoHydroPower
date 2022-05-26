// ;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace AtmoHydroPower
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false,
        new string[]
        {
            "AtmoHydro_LargeBlockLargeAtmosphericThrust", "AtmoHydro_LargeBlockSmallAtmosphericThrust",
            "AtmoHydro_SmallBlockLargeAtmosphericThrust", "AtmoHydro_SmallBlockSmallAtmosphericThrust",
            "AtmoHydro_LargeBlockLargeAtmosphericThrustSciFi", "AtmoHydro_LargeBlockSmallAtmosphericThrustSciFi",
            "AtmoHydro_SmallBlockLargeAtmosphericThrustSciFi", "AtmoHydro_SmallBlockSmallAtmosphericThrustSciFi",
            "AtmosphericThrusterLarge_SciFiForced", "AtmosphericThrusterSmall_SciFiForced",
            "AtmosphericThrusterSmall_SciFiForced123"
        })]
    class AtmoHydroPower_GameLogic : MyGameLogicComponent
    {
        private IMyThrust m_Block = null; // storing the entity as a block reference to avoid re-casting it every time it's needed, this is the lowest type a block entity can be;

        private MyResourceSourceComponent m_PowerSource = null;
        private MyResourceSinkComponent m_PowerSink = null;

        public override void Init(MyObjectBuilder_EntityBase _objectBuilder)
        {
            Logger.Log("Initializing..");
            m_Block = (IMyThrust)Entity;
            if (m_Block == null)
            {
                // entity is not thruster;
                Logger.Log("Entity is " + Entity.GetObjectBuilder().SubtypeId);
                return;
            }


            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;

            Logger.Log("GameLogic attached");
            Logger.Log("block is " + m_Block.BlockDefinition.SubtypeId);

            m_PowerSink = Entity.Components.Get<MyResourceSinkComponent>();
            if (m_PowerSink == null)
            {
                Logger.Log("sink is null");
            }
            else
            {
                float input = m_PowerSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
                Logger.Log("Max Input (electric) = " + input);
            }

            float maxPowerCost = 0.0f;
            MyThrust block = m_Block as MyThrust;
            if (block == null)
            {
                Logger.Log("  Block is not MyThrust (this should not happens)");
            }
            else
            {
                maxPowerCost = block.BlockDefinition.MaxPowerConsumption;
                Logger.Log("  Max Power Cost = " + maxPowerCost);
            }
            
            m_PowerSource = new MyResourceSourceComponent();
            m_PowerSource.Init(MyStringHash.Get("Reactors"), new MyResourceSourceInfo()
            {
                DefinedOutput = maxPowerCost * Constants.POWER_OUTPUT_MOD,
                IsInfiniteCapacity = true,
                ProductionToCapacityMultiplier = 1.0f,
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId
            });

            m_Block.Components.Add(typeof(MyResourceSourceComponent), m_PowerSource);


            Logger.Log("Defined Output = " + m_PowerSource.DefinedOutputByType(MyResourceDistributorComponent.ElectricityId));


            m_Block.IsWorkingChanged += M_Block_IsWorkingChanged;
        }

        private void M_Block_IsWorkingChanged(IMyCubeBlock _obj)
        {
            IMyThrust block = _obj as IMyThrust;
            if (block == null)
                return; // this should not happens;
            if (m_PowerSource == null)
            {
                Logger.Log("m_Block.IsWorkingChanged > m_PowerSource is null (this should not happens)");
                return;
            }
            
            if (block.IsWorking)
            {
                Logger.Log("Block is enabled");
                m_PowerSource.Enabled = true;
                CalculatePowerOutput();
            }
            else
            {
                Logger.Log("Block is disabled");
                m_PowerSource.Enabled = false;
                Logger.Log("Current Output = " + m_PowerSource.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId));
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            m_PowerSink = Entity.Components.Get<MyResourceSinkComponent>();
            if (m_PowerSink == null)
            {
                Logger.Log("BeforeFrame > sink is null");
            }
            else
            {
                float input = m_PowerSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
                Logger.Log("BeforeFrame > Max Input (electric) = " + input);
            }


            UpdateBeforeSimulation10(); // HACK! this is used to set the power output to prevent power spike on the first frame;

        }

        public override void MarkForClose()
        {
            // called when entity is about to be removed for whatever reason (block destroyed, entity deleted, ship despawn because of sync range, etc)

            m_Block.IsWorkingChanged -= M_Block_IsWorkingChanged;



        }

        public override void UpdateBeforeSimulation()
        {

        }

        public override void UpdateBeforeSimulation10()
        {
            if (!m_Block.IsWorking)
            {
                Logger.Log("Block is not working");
                return;
            }

            CalculatePowerOutput();

            //MyThrust block = m_Block as MyThrust;
            //if (block == null)
            //{
            //    Logger.Log("block is not MyThrust???");
            //    return;
            //}

            //block.CustomInfo += "Power Output = " + m_PowerSource.CurrentOutput;


            //m_Block.ThrustMultiplier = 0.0f;
        }

        private void CalculatePowerOutput()
        {
            float outputPercent = m_Block.CurrentThrust / m_Block.MaxThrust;
            m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, 1.5f);
            //m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, outputPercent * Constants.MAX_POWER_OUTPUT);
            
            


            //Logger.Log("Current Output = " + m_PowerSource.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId));
        }
    }
}
