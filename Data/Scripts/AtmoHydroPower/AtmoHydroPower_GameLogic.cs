// ;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace AtmoHydroPower
{
    public enum ThrusterSpinState
    {
        Off = 0,
        Idle = 1,
        SpinningUp,
        RollingBack
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false, new string[] {
        /* Vanilla Replaced */
        "LargeBlockLargeAtmosphericThrust", "LargeBlockSmallAtmosphericThrust",
        "SmallBlockLargeAtmosphericThrust", "SmallBlockSmallAtmosphericThrust",
        "LargeBlockLargeAtmosphericThrustSciFi", "LargeBlockSmallAtmosphericThrustSciFi",
        "SmallBlockLargeAtmosphericThrustSciFi", "SmallBlockSmallAtmosphericThrustSciFi",

        /* Standalone */
        "AtmoHydro_LargeBlockLargeAtmosphericThrust", "AtmoHydro_LargeBlockSmallAtmosphericThrust",
        "AtmoHydro_SmallBlockLargeAtmosphericThrust", "AtmoHydro_SmallBlockSmallAtmosphericThrust",
        "AtmoHydro_LargeBlockLargeAtmosphericThrustSciFi", "AtmoHydro_LargeBlockSmallAtmosphericThrustSciFi",
        "AtmoHydro_SmallBlockLargeAtmosphericThrustSciFi", "AtmoHydro_SmallBlockSmallAtmosphericThrustSciFi",

        /* Afterburners */
        "AtmosphericThrusterLarge_SciFiForced", "AtmosphericThrusterSmall_SciFiForced",
        "AtmosphericThrusterSmall_SciFiForced123"
    })]
    class AtmoHydroPower_GameLogic : MyGameLogicComponent
    {
        private static readonly List<string> s_VanillaSubtypeIds = new List<string>()
        {
            "LargeBlockLargeAtmosphericThrust",
            "LargeBlockSmallAtmosphericThrust",
            "SmallBlockLargeAtmosphericThrust",
            "SmallBlockSmallAtmosphericThrust",
            "LargeBlockLargeAtmosphericThrustSciFi",
            "LargeBlockSmallAtmosphericThrustSciFi",
            "SmallBlockLargeAtmosphericThrustSciFi",
            "SmallBlockSmallAtmosphericThrustSciFi",
        };

        private static readonly List<string> s_ModSubtypeIds = new List<string>()
        {
            "AtmoHydro_LargeBlockLargeAtmosphericThrust",
            "AtmoHydro_LargeBlockSmallAtmosphericThrust",
            "AtmoHydro_SmallBlockLargeAtmosphericThrust",
            "AtmoHydro_SmallBlockSmallAtmosphericThrust",
            "AtmoHydro_LargeBlockLargeAtmosphericThrustSciFi",
            "AtmoHydro_LargeBlockSmallAtmosphericThrustSciFi",
            "AtmoHydro_SmallBlockLargeAtmosphericThrustSciFi",
            "AtmoHydro_SmallBlockSmallAtmosphericThrustSciFi",

            "AtmosphericThrusterLarge_SciFiForced",
            "AtmosphericThrusterSmall_SciFiForced",
            "AtmosphericThrusterSmall_SciFiForced123"
        };

        private int m_RemainingSpinTicks = 0;
        private ThrusterSpinState m_SpinState = ThrusterSpinState.Off;



        private IMyThrust m_Block = null; 

        private MyResourceSourceComponent m_PowerSource = null;
        private MyResourceSinkComponent m_PowerSink = null;
        
        public override void Init(MyObjectBuilder_EntityBase _objectBuilder)
        {
            if (Config.s_IsReplaceModPresent == Config.s_IsStandaloneModPresent)
                return;

            Logger.Log("Initializing GameLogic for Entity " + Entity.EntityId + "..");
            if (!(Entity is IMyThrust))
            {
                Logger.Log("  Entity " + Entity.EntityId + " is not thruster");
                return;
            }

            if (Config.s_IsReplaceModPresent)
            { }
            else if (Config.s_IsStandaloneModPresent)
            {
                IMyCubeBlock block = Entity as IMyCubeBlock;
                if (s_VanillaSubtypeIds.Contains(block.BlockDefinition.SubtypeId))
                {
                    Logger.Log("  Vanilla block " + block.EntityId + " (" + block.BlockDefinition.SubtypeId + ") will be ignored");
                    NeedsUpdate = MyEntityUpdateEnum.NONE;
                    return;
                }
            }

            m_Block = (IMyThrust)Entity;
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            Logger.Log("  Block " + m_Block.EntityId + " is " + m_Block.BlockDefinition.SubtypeId);
            
            float maxPowerCost = 0.0f;
            if (m_Block is MyThrust)
            {
                maxPowerCost = ((MyThrust)m_Block).BlockDefinition.MaxPowerConsumption;
                Logger.Log("  Max Power Cost = " + maxPowerCost);
            }
            else
            {
                Logger.Log("  Block is not MyThrust (this should not happens)");
            }
            
            m_PowerSource = new MyResourceSourceComponent();
            m_PowerSource.Init(MyStringHash.Get("Reactors"), new MyResourceSourceInfo()
            {
                DefinedOutput = maxPowerCost * Constants.POWER_OUTPUT_MOD,
                IsInfiniteCapacity = true,
                //ProductionToCapacityMultiplier = 1.0f,
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId
            });
            m_Block.Components.Add(typeof(MyResourceSourceComponent), m_PowerSource);
            Logger.Log("  Defined Output = " + m_PowerSource.DefinedOutputByType(MyResourceDistributorComponent.ElectricityId));
            
            // TODO: power sink;

            m_Block.IsWorkingChanged += M_Block_IsWorkingChanged;
            m_Block.AppendingCustomInfo += M_Block_AppendingCustomInfo;

            Logger.Log("  GameLogic attached.");
        }

        private void M_Block_IsWorkingChanged(IMyCubeBlock _obj)
        {
            Logger.Log("Block " + _obj.EntityId + "'s working state changed..");
            IMyThrust block = _obj as IMyThrust;
            if (block == null)
                return; // this should not happens;
            if (m_PowerSource == null)
            {
                Logger.Log("  m_PowerSource is null (this should not happens)");
                return;
            }
            
            if (block.IsWorking)
            {
                Logger.Log("  Block is enabled");

                if (m_SpinState == ThrusterSpinState.Off)
                {
                    m_SpinState = ThrusterSpinState.SpinningUp;
                    m_RemainingSpinTicks = Constants.SPINUP_TIME_TICKS;
                    Logger.Log("  Spinning up from cold");
                }
                else if (m_SpinState == ThrusterSpinState.RollingBack)
                {
                    m_SpinState = ThrusterSpinState.SpinningUp;
                    float rollbackPercent = m_RemainingSpinTicks / Constants.ROLLBACK_TIME_TICKS;
                    m_RemainingSpinTicks = (int)(Constants.SPINUP_TIME_TICKS * (1.0f - rollbackPercent));
                    Logger.Log(string.Format("  Spinning up from {0:0.##}%", rollbackPercent * 100.0f));
                }
                else
                {
                    Logger.Log("  Invalid state: " + m_SpinState);
                }
                
                m_PowerSource.Enabled = true;
            }
            else
            {
                Logger.Log("  Block is disabled");

                if (m_SpinState == ThrusterSpinState.Idle)
                {
                    m_SpinState = ThrusterSpinState.RollingBack;
                    m_RemainingSpinTicks = Constants.ROLLBACK_TIME_TICKS;
                    Logger.Log("  Rolling back from idle");
                }
                else if (m_SpinState == ThrusterSpinState.SpinningUp)
                {
                    m_SpinState = ThrusterSpinState.RollingBack;
                    float spinupPercentInvert = 1.0f - (m_RemainingSpinTicks / Constants.SPINUP_TIME_TICKS);
                    m_RemainingSpinTicks = (int)(Constants.ROLLBACK_TIME_TICKS * spinupPercentInvert);
                    Logger.Log(string.Format("  Rolling back from {0:0.##}%", spinupPercentInvert * 100.0f));
                }
                else
                {
                    Logger.Log("  Invalid state: " + m_SpinState);
                }
                
                m_PowerSource.Enabled = false;
                m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
                m_PowerSource.SetMaxOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
            }
        }

        private void M_Block_AppendingCustomInfo(IMyTerminalBlock arg1, System.Text.StringBuilder arg2)
        {









        }

        public override void UpdateOnceBeforeFrame()
        {
            if (m_Block.IsWorking)
            {
                m_SpinState = ThrusterSpinState.Idle;
            }
            else
            {
                m_SpinState = ThrusterSpinState.Off;
                m_PowerSource.Enabled = false;
                m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
                m_PowerSource.SetMaxOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
            }
        }

        public override void MarkForClose()
        {
            if (m_Block == null)
                return;
            
            m_Block.IsWorkingChanged -= M_Block_IsWorkingChanged;
            m_Block.AppendingCustomInfo -= M_Block_AppendingCustomInfo;

            Logger.Log("Block " + m_Block.EntityId + " has been marked for close");
        }

        public override void UpdateBeforeSimulation()
        {
            Logger.Log("UpdateBeforeSimulation..", 5);

            if (m_RemainingSpinTicks > 0)
                --m_RemainingSpinTicks;

            if (m_RemainingSpinTicks <= 0)
            {
                if (m_SpinState == ThrusterSpinState.SpinningUp)
                {
                    m_SpinState = ThrusterSpinState.Idle;
                }
                else if (m_SpinState == ThrusterSpinState.RollingBack)
                {
                    m_SpinState = ThrusterSpinState.Off;
                }
            }

            if (m_SpinState == ThrusterSpinState.Off)
            {
                m_Block.ThrustMultiplier = 0.01f; // this is useless anyway;
                Logger.Log("  Thruster " + m_Block.EntityId + " is Off", 5);
            }
            else if (m_SpinState == ThrusterSpinState.Idle)
            {
                m_Block.ThrustMultiplier = 1.0f;
                Logger.Log("  Thruster " + m_Block.EntityId + " is Idle", 5);
            }
            else if (m_SpinState == ThrusterSpinState.SpinningUp)
            {
                float spinupPercentInvert = m_RemainingSpinTicks / Constants.SPINUP_TIME_TICKS;
                if (spinupPercentInvert < 0.25f)
                {
                    m_Block.ThrustMultiplier = 1.0f - (spinupPercentInvert / 0.25f);
                }
                else
                {
                    m_Block.ThrustMultiplier = 0.01f;
                }

                Logger.Log(string.Format("  Thruster {0:0} is Spinning up {1:0.##}%", m_Block.EntityId, (1.0f - spinupPercentInvert) * 100.0f), 5);
            }
            else if (m_SpinState == ThrusterSpinState.RollingBack)
            {
                float rollbackPercent = m_RemainingSpinTicks / Constants.ROLLBACK_TIME_TICKS;

                // these will not work in current state of the game;
                //if (rollbackPercent > 0.75f)
                //{
                //    m_Block.ThrustMultiplier = 1.0f - ((1.0f - rollbackPercent) / 0.25f);
                //}
                //else
                //{
                //    m_Block.ThrustMultiplier = 0.01f;
                //}

                Logger.Log(string.Format("  Thruster {0:0} is Rolling back {1:0.##}%", m_Block.EntityId, rollbackPercent * 100.0f), 5);
            }





        }

        public override void UpdateBeforeSimulation10()
        {
            Logger.Log("UpdateBeforeSimulation10..", 4);

            if (!m_Block.IsWorking)
            {
                Logger.Log("  Block " + m_Block.EntityId + " is not working", 5);
                return;
            }

            if (m_SpinState == ThrusterSpinState.Idle)
                CalculatePowerOutput();
            

        }

        private void CalculatePowerOutput()
        {
            Logger.Log("  Thruster " + m_Block.EntityId, 4);

            const float coeff = 2.0f / 2.1f;

            float thrusPercent = m_Block.CurrentThrust / m_Block.MaxThrust;
            float powerOut = (thrusPercent + 1.1f) * m_PowerSource.DefinedOutputByType(MyResourceDistributorComponent.ElectricityId) * coeff;
            float powerPercent = powerOut / m_PowerSource.DefinedOutputByType(MyResourceDistributorComponent.ElectricityId);

            m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, powerOut);
            m_PowerSource.SetMaxOutputByType(MyResourceDistributorComponent.ElectricityId, powerOut);

            Logger.Log(string.Format("    Thrust {0:0.##}/{1:0.##} ({2:0.##}%)", m_Block.CurrentThrust, m_Block.MaxThrust, thrusPercent * 100.0f), 4);
            Logger.Log(string.Format("    Power {0:0.##}/{1:0.##} ({2:0.##}%)", 
                m_PowerSource.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId), 
                m_PowerSource.DefinedOutputByType(MyResourceDistributorComponent.ElectricityId), powerPercent * 100.0f)
                , 4);



        }
    }
}
