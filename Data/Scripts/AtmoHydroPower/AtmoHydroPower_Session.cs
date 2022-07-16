// ;
using VRage.Game.Components;

namespace AtmoHydroPower
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class AtmoHydroPower_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            Logger.Init();
            Logger.LogLevel = 3; // Lower LogLevel value for less log, raise it for more log;

            Logger.Log("Warning: You are using advanced version of this mod, which means no failsafe.");
            Logger.Log("If you don't set up mods properly, your world may crash.");

        }

        protected override void UnloadData()
        {
            Logger.Close();
        }

    }
}
