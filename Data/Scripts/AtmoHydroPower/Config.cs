// ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtmoHydroPower
{
    public class Constants
    {
        public const float MAX_POWER_OUTPUT = 1.0f;
        public const float POWER_OUTPUT_MOD = 1.0f;

        public const int SPINUP_TIME_TICKS = (int)(60.0f * 1.0f);
        public const int ROLLBACK_TIME_TICKS = (int)(60.0f * 5.0f);
        
    }

    public class Config
    {
        public static bool s_IsReplaceModPresent = false;
        public static bool s_IsStandaloneModPresent = false;
    }
}
