// ;
using System;
using System.Collections.Generic;

namespace AtmoHydroPower
{
    public class Constants
    {
        public const int SPINUP_TIME_TICKS = (int)(60.0f * SPINUP_TIME);
        public const int ROLLBACK_TIME_TICKS = (int)(60.0f * ROLLBACK_TIME);

        /* Power Density is taken from vanilla Hydrogen Engine.
         *      Large block: 500L/5MW
         *      Small block: 50L/500kW
         * Basically 0.01MW/1L
         */
        public const float POWER_DENSITY_LARGE = 0.01f;
        public const float POWER_DENSITY_SMALL = 0.01f;

        public const float POWER_INPUT_MOD = 0.5f;
        public const float POWER_INPUT_KICKSTART = 10.0f;

        public const float POWER_OUTPUT_IDLE = 1.05f;
        public const float POWER_OUTPUT_MAX = 2.5f;


        public const float THRUST_POWER_MOD_WHEN_LOW_POWER = 0.5f;

#if false
        public const float POWER_IN_SMALLGRID_SMALLBLOCK = 1.0f;
        public const float POWER_IN_SMALLGRID_LARGEBLOCK = 1.0f;
        public const float POWER_IN_SMALLGRID_AFTERBURNER = 1.0f;

        public const float POWER_IN_LARGEGRID_SMALLBLOCK = 1.0f;
        public const float POWER_IN_LARGEGRID_LARGEBLOCK = 1.0f;
        public const float POWER_IN_LARGEGRID_AFTERBURNER = 1.0f;

        public static readonly Dictionary<string, float> s_PowerInputs = new Dictionary<string, float>()
        {
            /* Vanilla Replaced */
            { "LargeBlockLargeAtmosphericThrust", POWER_IN_LARGEGRID_LARGEBLOCK },
            { "LargeBlockSmallAtmosphericThrust", POWER_IN_LARGEGRID_SMALLBLOCK },
            { "SmallBlockLargeAtmosphericThrust", POWER_IN_SMALLGRID_LARGEBLOCK },
            { "SmallBlockSmallAtmosphericThrust", POWER_IN_SMALLGRID_SMALLBLOCK },
            { "LargeBlockLargeAtmosphericThrustSciFi", POWER_IN_LARGEGRID_LARGEBLOCK },
            { "LargeBlockSmallAtmosphericThrustSciFi", POWER_IN_LARGEGRID_SMALLBLOCK },
            { "SmallBlockLargeAtmosphericThrustSciFi", POWER_IN_SMALLGRID_LARGEBLOCK },
            { "SmallBlockSmallAtmosphericThrustSciFi", POWER_IN_SMALLGRID_SMALLBLOCK },

            /* Standalone */
            { "AtmoHydro_LargeBlockLargeAtmosphericThrust", POWER_IN_LARGEGRID_LARGEBLOCK },
            { "AtmoHydro_LargeBlockSmallAtmosphericThrust", POWER_IN_LARGEGRID_SMALLBLOCK },
            { "AtmoHydro_SmallBlockLargeAtmosphericThrust", POWER_IN_SMALLGRID_LARGEBLOCK },
            { "AtmoHydro_SmallBlockSmallAtmosphericThrust", POWER_IN_SMALLGRID_SMALLBLOCK },
            { "AtmoHydro_LargeBlockLargeAtmosphericThrustSciFi", POWER_IN_LARGEGRID_LARGEBLOCK },
            { "AtmoHydro_LargeBlockSmallAtmosphericThrustSciFi", POWER_IN_LARGEGRID_SMALLBLOCK },
            { "AtmoHydro_SmallBlockLargeAtmosphericThrustSciFi", POWER_IN_SMALLGRID_LARGEBLOCK },
            { "AtmoHydro_SmallBlockSmallAtmosphericThrustSciFi", POWER_IN_SMALLGRID_SMALLBLOCK },

            /* Afterburners */
            { "AtmosphericThrusterLarge_SciFiForced", POWER_IN_LARGEGRID_AFTERBURNER },
            { "AtmosphericThrusterSmall_SciFiForced", POWER_IN_SMALLGRID_AFTERBURNER },
            { "AtmosphericThrusterSmall_SciFiForced123", POWER_IN_SMALLGRID_AFTERBURNER }
        };
#endif

        public static readonly List<string> s_VanillaSubtypeIds = new List<string>()
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

        public static readonly List<string> s_ModSubtypeIds = new List<string>()
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





        private const float SPINUP_TIME = 1.0f;
        private const float ROLLBACK_TIME = 5.0f;

    }

    public class Config
    {
        public static bool s_IsReplaceModPresent = false;
        public static bool s_IsStandaloneModPresent = false;
    }

    public static class Utils
    {
        public static string FormatTimeFromGameTicks(int _ticks)
        {
            if (_ticks < 60)
                return "1s";

            TimeSpan ts = TimeSpan.FromSeconds(_ticks / 60.0 + 1.0);

            if (ts.Days > 0)
                return ts.Days + "d " + ts.Hours + "h " + ts.Minutes + "m " + ts.Seconds + "s";

            if (ts.Hours > 0)
                return ts.Hours + "h " + ts.Minutes + "m " + ts.Seconds + "s";

            if (ts.Minutes > 0)
                return ts.Minutes + "m " + ts.Seconds + "s";

            if (ts.Seconds > 0)
                return ts.Seconds + "s";

            return "";
        }

        public static string FormatPowerFromMegaWatt(float _power)
        {
            if (_power >= 1e9f)
                return _power.ToString("e2") + " TW"; // scientific notation;

            if (_power >= 1e6f)
                return string.Format("{0:0.##} TW", _power / 1e6f);

            if (_power >= 1e3f)
                return string.Format("{0:0.##} GW", _power / 1e3f);

            if (_power >= 1e0f)
                return string.Format("{0:0.##} MW", _power);

            if (_power >= 1e-3f)
                return string.Format("{0:0.##} kW", _power * 1e3f);

            return string.Format("{0:0.##} W", _power * 1e6f);
        }

        

    }
}
