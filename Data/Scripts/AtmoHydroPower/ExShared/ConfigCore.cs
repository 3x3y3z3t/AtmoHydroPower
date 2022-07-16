/*  ExShared/ConfigCore.cs
 *  Version: v3.0 (2022.07.01)
 * 
 * Core framework for User Configuration Manager.
 * This is the Core Config Manager used to read and write User Config to a file in World Storage.
 * File name is configurable.
 * 
 *  Contributor
 *      Arime-chan
 */

using Sandbox.ModAPI;
using System;
using System.IO;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace ExShared
{
    public enum LoadResult
    {
        Okay = 0,
        FileNotFound,
        ParseFailed
    }

    public abstract class Config
    {
        public string Filename { get; protected set; }
        public string ConfigVersion { get; set; }
        public int LogLevel { get; set; }

        protected Config(string _filename, Logger _logger)
        {
            Filename = _filename;
            m_Logger = _logger;

            LoadDefaults();

            if (!LoadConfigFile())
            {
                SaveConfigFile();

            }

        }

        public bool LoadConfigFile()
        {
            m_Logger.WriteLine("Loading config..");

            string strData = PeekConfigFile();
            if (string.IsNullOrEmpty(strData))
                return false;

            MyIni iniData = new MyIni();
            MyIniParseResult parseResult;
            if (!iniData.TryParse(strData, out parseResult))
            {
                m_Logger.WriteLine("  Data loaded successfully, but could not be parsed: " + parseResult.ToString());
                return false;
            }

            ParseConfigData(iniData);

            if (m_IsConfigVersionMismatch)
            {
                m_Logger.WriteLine("  Saving backup data..");
                SaveBackupConfigFile(strData);
                SaveConfigFile();
            }

            m_Logger.WriteLine("  Done.");
            return true;
        }

        public bool SaveConfigFile()
        {
            m_Logger.WriteLine("Saving config..");

            MyIni iniData = new MyIni();
            PackIniData(iniData);
            string data = iniData.ToString();
            try
            {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(Filename, typeof(Config));
                writer.WriteLine(data);
                writer.Close();
            }
            catch (Exception _e)
            {
                m_Logger.WriteLine("  > Exception < Error during saving Config: " + _e.Message);
                return false;
            }

            m_Logger.WriteLine("Saving done");
            return true;
        }
        
        protected virtual void LoadDefaults()
        { }

        protected virtual void PackIniData(MyIni _iniData)
        {
            _iniData.Set(c_SectionCommon, c_NameConfigVersion, ConfigVersion);
            _iniData.Set(c_SectionCommon, c_NameLogLevel, LogLevel);

            _iniData.Set(c_SectionCommon, c_NameLogLevel, c_CommentLogLevel);
        }

        protected virtual void ParseConfigData(MyIni _iniData)
        {
            ConfigVersion = _iniData.Get(c_SectionCommon, c_NameConfigVersion).ToString();
            LogLevel = _iniData.Get(c_SectionCommon, c_NameLogLevel).ToInt32();

        }

        private bool SaveBackupConfigFile(string _data)
        {
            string filename = Filename.Insert(Filename.Length - 4, "_old");
            try
            {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(Config));
                writer.WriteLine(_data);
                writer.Close();
            }
            catch (Exception _e)
            {
                m_Logger.WriteLine("  > Exception < Error during saving backup file (you are out of luck): " + _e.Message);
                return false;
            }

            return true;
        }

        private string PeekConfigFile()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(Filename, typeof(Config)))
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Filename, typeof(Config));
                    string data = reader.ReadToEnd();
                    reader.Close();

                    return data;
                }
                else
                {
                    m_Logger.WriteLine("  Config file not found");
                    return null;
                }
            }
            catch (Exception _e)
            {
                m_Logger.WriteLine("  > Exception < Error during loading Config: " + _e.Message);
                return null;
            }
        }


        protected bool m_IsConfigVersionMismatch = false;

        protected Logger m_Logger = null;


        protected const string c_SectionCommon = "Common";

        protected const string c_NameConfigVersion = "Config Version (Do not touch this plz)";
        protected const string c_NameLogLevel = "Log Level";

        protected const string c_CommentLogLevel = "Log Level scales from -1 (disable logging) to 5 (log everything)";

    }
}
