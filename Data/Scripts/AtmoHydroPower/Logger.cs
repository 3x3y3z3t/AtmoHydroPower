// ;
using ExShared;

namespace AtmoHydroPower
{
    public class Logger
    {
        public static int LogLevel
        {
            get { if (s_Logger != null) return s_Logger.LogLevel; return -1; }
            set { if (s_Logger != null) s_Logger.LogLevel = value; }
        }

        private static ExShared.Logger s_Logger = null;

        public static bool Init()
        {
            if (s_Logger != null)
                return false;

            s_Logger = new ExShared.Logger("debug", "AtmoHydroPower");
            return true;
        }

        public static bool Close()
        {
            if (s_Logger == null)
                return false;

            s_Logger.Close();
            return true;
        }

        public static void Log(string _message, int _level = 0)
        {
            if (s_Logger == null)
                Init();

            s_Logger.WriteLine(_message, _level);
        }

    }
}
