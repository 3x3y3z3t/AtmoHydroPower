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

        public const float POWER_INPUT_MOD_IDLE = 0.5f;
        public const float POWER_INPUT_MOD_KICKSTART = 10.0f;

        public const float POWER_OUTPUT_IDLE = 1.05f;
        public const float POWER_OUTPUT_MAX = 2.5f;


        public const float THRUST_POWER_MOD_WHEN_LOW_POWER = 0.5f;




        private const float SPINUP_TIME = 1.0f;
        private const float ROLLBACK_TIME = 5.0f;

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
