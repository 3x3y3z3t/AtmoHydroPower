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
        }

        protected override void UnloadData()
        {
            Logger.Close();
        }
        
    }
}
