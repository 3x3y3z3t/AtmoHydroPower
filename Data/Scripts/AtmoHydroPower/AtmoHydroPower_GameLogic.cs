// ;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
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
        float m_PowerInput = 0.0f;
        float m_PowerOutput = 0.0f;


        private ThrusterSpinState m_SpinState = ThrusterSpinState.Off;
        private int m_RemainingSpinTicks = 0;
        private int m_SkipTicks = 0;

        private IMyThrust m_Block = null;
        private IMyCubeGrid m_Grid = null;
        private MyResourceDistributorComponent m_ResDist = null;

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

            IMyCubeBlock block = (IMyCubeBlock)Entity;
            string blockSubtypeId = block.BlockDefinition.SubtypeId;
            if (Config.s_IsReplaceModPresent)
            { }
            else if (Config.s_IsStandaloneModPresent)
            {
                if (Constants.s_VanillaSubtypeIds.Contains(blockSubtypeId))
                {
                    Logger.Log("  Vanilla block " + block.EntityId + " (" + blockSubtypeId + ") will be ignored");
                    NeedsUpdate = MyEntityUpdateEnum.NONE;
                    return;
                }
            }

            m_Block = (IMyThrust)Entity;
            m_Grid = m_Block.CubeGrid;
            if (m_Grid == null)
            {
                Logger.Log("  Block " + m_Block.EntityId + " doesn't have CubeGrid (this should not happens)");
                return;
            }
            MyCubeSize cubeSize = ((MyCubeBlock)m_Block).BlockDefinition.CubeSize;
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            Logger.Log("  SubtypeId: " + blockSubtypeId);
            Logger.Log("  CubeSize: " + cubeSize);
            Logger.Log("  CubeGrid.EntityId: " + m_Grid.EntityId);

#if false
            if (Constants.s_PowerInputs.ContainsKey(blockSubtypeId))
            {
                m_PowerInput = Constants.s_PowerInputs[blockSubtypeId];
                m_PowerOutput = m_PowerInput * Constants.POWER_OUTPUT_MOD;
                Logger.Log("  PowerRating: In:" + m_PowerInput + ", Out:" + m_PowerOutput);
            }
            else
            {
                Logger.Log("  Couldn't find power input rating for block " + block.EntityId + " (" + blockSubtypeId + ")");
            }
#else
            if (m_Block is MyThrust)
            {
                float fuelCost = ((MyThrust)m_Block).BlockDefinition.MaxPowerConsumption;
                if (cubeSize == MyCubeSize.Large)
                    m_PowerOutput = fuelCost * Constants.POWER_DENSITY_LARGE;
                else
                    m_PowerOutput = fuelCost * Constants.POWER_DENSITY_SMALL;
                m_PowerInput = m_PowerOutput * Constants.POWER_INPUT_MOD;
                Logger.Log("  Fuel Cost = " + fuelCost);
                Logger.Log("  PowerRating: In:" + m_PowerInput + ", Out:" + m_PowerOutput);
            }
            else
            {
                Logger.Log("  Block is not MyThrust (this should not happens)");
            }
#endif

            m_PowerSource = new MyResourceSourceComponent();
            m_PowerSource.Init(MyStringHash.Get("Reactors"), new MyResourceSourceInfo()
            {
                DefinedOutput = m_PowerOutput,
                IsInfiniteCapacity = true,
                //ProductionToCapacityMultiplier = 1.0f,
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId
            });
            m_Block.Components.Add(typeof(MyResourceSourceComponent), m_PowerSource);

            m_PowerSink = new MyResourceSinkComponent();
            m_PowerSink.Init(MyStringHash.Get("Thrust"), new MyResourceSinkInfo()
            {
                RequiredInputFunc = RequiredElectricInputFunc,
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId
            });
            //m_Block.Components.Add(typeof(MyResourceSinkComponent), m_PowerSink);

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
            if (m_PowerSink == null)
            {
                Logger.Log("  m_PowerSink is null (this should not happens)");
                return;
            }
            
            if (block.IsWorking)
            {
                Logger.Log("  Block is enabled", 4);

                if (m_SpinState == ThrusterSpinState.Off)
                {
                    m_SpinState = ThrusterSpinState.SpinningUp;
                    m_RemainingSpinTicks = Constants.SPINUP_TIME_TICKS;
                    Logger.Log("  Spinning up from cold", 4);
                }
                else if (m_SpinState == ThrusterSpinState.RollingBack)
                {
                    m_SpinState = ThrusterSpinState.SpinningUp;
                    float rollbackPercent = (float)m_RemainingSpinTicks / Constants.ROLLBACK_TIME_TICKS;
                    m_RemainingSpinTicks = (int)(Constants.SPINUP_TIME_TICKS * (1.0f - rollbackPercent));
                    Logger.Log(string.Format("  Spinning up from {0:0.##}%", rollbackPercent * 100.0f), 4);
                }
                else
                {
                    Logger.Log("  Invalid state: " + m_SpinState, 4);
                }
                
                m_PowerSource.Enabled = true;

                    CalculatePowerInput();
                    CalculatePowerOutput();
            }
            else
            {
                Logger.Log("  Block is disabled", 4);

                if (m_SpinState == ThrusterSpinState.Idle)
                {
                    m_SpinState = ThrusterSpinState.RollingBack;
                    m_RemainingSpinTicks = Constants.ROLLBACK_TIME_TICKS;
                    Logger.Log("  Rolling back from idle", 4);
                }
                else if (m_SpinState == ThrusterSpinState.SpinningUp)
                {
                    m_SpinState = ThrusterSpinState.RollingBack;
                    float spinupPercentInvert = 1.0f - ((float)m_RemainingSpinTicks / Constants.SPINUP_TIME_TICKS);
                    m_RemainingSpinTicks = (int)(Constants.ROLLBACK_TIME_TICKS * spinupPercentInvert);
                    Logger.Log(string.Format("  Rolling back from {0:0.##}%", spinupPercentInvert * 100.0f), 4);
                }
                else
                {
                    Logger.Log("  Invalid state: " + m_SpinState, 4);
                }
                
                m_PowerSource.Enabled = false;
                m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
                m_PowerSource.SetMaxOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);

                m_PowerSink.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
                m_PowerSink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
            }
        }

        private void M_Block_AppendingCustomInfo(IMyTerminalBlock _block, System.Text.StringBuilder _sb)
        {
            _sb.Append("Working Status: " + m_SpinState);
            if (m_SpinState == ThrusterSpinState.SpinningUp)
            {
                float spinupPercent = 1.0f - (float)m_RemainingSpinTicks / Constants.SPINUP_TIME_TICKS;
                _sb.AppendFormat(" {0:0.##}%\n", spinupPercent * 100.0f);
            }
            else if (m_SpinState == ThrusterSpinState.RollingBack)
            {
                float rollbackPercent = (float)m_RemainingSpinTicks / Constants.ROLLBACK_TIME_TICKS;
                _sb.AppendFormat(" {0:0.##}%\n", rollbackPercent * 100.0f);
            }
            else
            {
                _sb.Append("\n");
            }

            _sb.Append("Power Status:\n");
            if (m_PowerSink != null)
            {
                float powerIn = m_PowerSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId);
                float powerPercent = powerIn / m_PowerInput;
                _sb.AppendFormat("  Input: {0} ({1:0.##}%)\n", Utils.FormatPowerFromMegaWatt(powerIn), powerPercent * 100.0f);
            }
            if (m_PowerSource != null)
            {
                float powerOut = m_PowerSource.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId);
                float powerPercent = powerOut / m_PowerOutput;
                _sb.AppendFormat("  Output: {0} ({1:0.##}%)\n", Utils.FormatPowerFromMegaWatt(powerOut), powerPercent * 100.0f);
            }

            _sb.Append("***Note*** Status is kinda broken. Click Terminal buttons to properly \"refresh\" detail status.\n");


        }

        private float RequiredElectricInputFunc()
        {
            return 0.0f;

            //if (m_Block == null)
            //    return 0.0f;

            //if (!m_Block.IsFunctional)
            //    return 0.0f;

            //return 1.0f;
        }

        public override void UpdateOnceBeforeFrame()
        {
            Logger.Log(m_Block.EntityId + " > UpdateOnceBeforeFrame()..", 3);

            if (m_Block.IsWorking)
            {
                Logger.Log("  Block is working");
                m_SpinState = ThrusterSpinState.Idle;
            }
            else
            {
                Logger.Log("  Block is not working");
                m_SpinState = ThrusterSpinState.Off;

                m_PowerSource.Enabled = false;
                m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
                m_PowerSource.SetMaxOutputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);

                m_PowerSink.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
                m_PowerSink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, 0.0f);
            }



            Logger.Log("  Done.", 3);
        }

        public override void MarkForClose()
        {
            if (m_Block == null)
                return;
            
            if (m_ResDist != null)
            {
                m_ResDist.RemoveSink(m_PowerSink);
            }

            m_Block.IsWorkingChanged -= M_Block_IsWorkingChanged;
            m_Block.AppendingCustomInfo -= M_Block_AppendingCustomInfo;

            Logger.Log("Block " + m_Block.EntityId + " has been marked for close");
        }

        public override void UpdateBeforeSimulation()
        {
            Logger.Log(m_Block.EntityId + " > UpdateBeforeSimulation..", 5);

            if (m_ResDist == null)
            {
                m_ResDist = (MyResourceDistributorComponent)m_Grid.ResourceDistributor;
                if (m_ResDist == null)
                {
                    Logger.Log("  Grid ResourceDistributor is still null");
                }
                else
                {
                    m_ResDist.AddSink(m_PowerSink);
                    Logger.Log("  Added electricity sink");
                }
            }

            MyResourceStateEnum state = MyResourceStateEnum.NoPower;
            if (m_ResDist != null)
            {
                state = m_ResDist.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true, m_Grid as MyCubeGrid);
                if (m_SkipTicks == 0)
                {
                    if (state == MyResourceStateEnum.OverloadAdaptible)
                        m_SkipTicks = 1;
                    else if (state == MyResourceStateEnum.OverloadBlackout)
                        m_SkipTicks = 5;
                }
            }

            if (m_SkipTicks > 0)
                --m_SkipTicks;

            if (m_RemainingSpinTicks > 0 && m_SkipTicks == 0)
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
                if (state == MyResourceStateEnum.Ok)
                    m_Block.ThrustMultiplier = 1.0f;
                else
                    m_Block.ThrustMultiplier = Constants.THRUST_POWER_MOD_WHEN_LOW_POWER;
                Logger.Log("  Thruster " + m_Block.EntityId + " is Idle", 5);
            }
            else if (m_SpinState == ThrusterSpinState.SpinningUp)
            {
                float spinupPercentInvert = (float)m_RemainingSpinTicks / Constants.SPINUP_TIME_TICKS;
                if (spinupPercentInvert < 0.25f)
                {
                    m_Block.ThrustMultiplier = 1.0f - (spinupPercentInvert / 0.25f);
                }
                else
                {
                    m_Block.ThrustMultiplier = 0.01f;
                }

                //Logger.Log("Tick = " + m_RemainingSpinTicks + " SkipTick = " + m_SkipTicks);
                Logger.Log(string.Format("  Thruster {0:0} is Spinning up {1:0.##}%", m_Block.EntityId, (1.0f - spinupPercentInvert) * 100.0f), 5);
            }
            else if (m_SpinState == ThrusterSpinState.RollingBack)
            {
                float rollbackPercent = (float)m_RemainingSpinTicks / Constants.ROLLBACK_TIME_TICKS;

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

            Logger.Log("  Thruster " + m_Block.EntityId, 4);
            CalculatePowerInput();
            if (m_SpinState == ThrusterSpinState.Idle)
            {
                CalculatePowerOutput();
            }

            if (m_ResDist != null)
            {
                var state = m_ResDist.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true, m_Grid as MyCubeGrid);
                var demand = m_ResDist.TotalRequiredInputByType(MyResourceDistributorComponent.ElectricityId, m_Grid);
                var produce = m_ResDist.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId, m_Grid);
                Logger.Log("    Grid Power State: " + demand + "/" + produce + " (" + state + ")", 4);
            }



            //m_Block.ShowInToolbarConfig = !m_Block.ShowInToolbarConfig;
            //m_Block.ShowInToolbarConfig = !m_Block.ShowInToolbarConfig;

        }

        private void CalculatePowerInput()
        {
            float powerIn = 0.0f;
            if (m_SpinState == ThrusterSpinState.SpinningUp)
            {
                powerIn = m_PowerInput * Constants.POWER_INPUT_KICKSTART;
            }
            else if (m_SpinState == ThrusterSpinState.Idle)
            {
                powerIn = m_PowerInput;
            }

            //powerIn *= 7.0f;

            m_PowerSink.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, powerIn);
            m_PowerSink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, powerIn);
            //m_PowerSink.Update();

            powerIn = m_PowerSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId);
            float powerPercent = powerIn / m_PowerInput;

            Logger.Log(string.Format("    Power In: {0:0.##}/{1:0.##} ({2:0.##}%)", powerIn * 1000.0f, m_PowerInput * 1000.0f, powerPercent * 100.0f), 4);

        }

        private void CalculatePowerOutput()
        {
            float thrustPercent = m_Block.CurrentThrust / m_Block.MaxThrust;
#if false
            const float coeff = 1.0f / 1.05f;
            float powerOut = (thrustPercent + (Constants.POWER_OUTPUT_MOD - 0.9f)) * m_PowerOutput * coeff;
#else
            float powerOut = m_PowerOutput * VRageMath.MathHelper.Lerp(Constants.POWER_OUTPUT_IDLE, Constants.POWER_OUTPUT_MAX, thrustPercent);
#endif

            //if (m_SpinState == ThrusterSpinState.Idle)
            //    powerOut *= 7.0f;

            m_PowerSource.SetMaxOutputByType(MyResourceDistributorComponent.ElectricityId, powerOut);
            m_PowerSource.SetOutputByType(MyResourceDistributorComponent.ElectricityId, powerOut);

            powerOut = m_PowerSource.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId);
            float powerPercent = powerOut / m_PowerOutput;

            Logger.Log(string.Format("    Power Out: {0:0.##}/{1:0.##} ({2:0.##}%)", powerOut * 1000.0f, m_PowerOutput * 1000.0f, powerPercent * 100.0f), 3);
            Logger.Log(string.Format("    Thrust {0:0.##}/{1:0.##} ({2:0.##}%)", m_Block.CurrentThrust, m_Block.MaxThrust, thrustPercent * 100.0f), 3);
        }
    }
}
