// ;
using ExShared;

namespace AtmoHydroPower
{
    public class Logger
    {
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

        public static void Log(string _message)
        {
            if (s_Logger == null)
                Init();

            s_Logger.WriteLine(_message);
        }

    }
}
