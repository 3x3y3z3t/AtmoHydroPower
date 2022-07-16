/*  AtmoHydroPower_GameLogic_Setup.cs
 *  Version: v1.0 (2022.07.17)
 * 
 * GameLogic component for AtmoHydroPower mod.
 * 
 * This file contains the setup part for GameLogic class.
 * I put this part into a separated file so modders (especially certain someone) 
 *  don't have to worry about the main code part (that they will not touch anyway)
 *  and can easily see where they need to edit so AHP works with their mod.
 *  Yes, everything you need to edit is in this file.
 *  
 * How to edit (see README.md for setup guide)
 *      1. Put your thruster blocks' SubtypeID in the string array, same as the two example I placed.
 *      2. Remove the example if you mind.
 * 
 *  Contributor
 *      Arime-chan
 */

using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;

namespace AtmoHydroPower
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false, new string[] {
        /* Place your thrusters' SubtypeIDs below this line. */

        "LargeBlockExampleThruster", // this is an example;
        "SmallBlockExampleThruster", // this is an example;



        /* Place your thrusters' SubtypeIDs above this line. */
    })]
    partial class AtmoHydroPower_GameLogic : MyGameLogicComponent
    { }
}
