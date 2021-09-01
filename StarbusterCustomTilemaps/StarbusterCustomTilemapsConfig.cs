using System;
using UModFramework.API;

namespace StarbusterCustomTilemaps
{
    public class StarbusterCustomTilemapsConfig
    {
        private static readonly string configVersion = "1.1";

        //Add your config vars here.
        public static bool showDebug = false;

        internal static void Load()
        {
            StarbusterCustomTilemaps.Log("Loading settings.");
            try
            {
                using (UMFConfig cfg = new UMFConfig())
                {
                    string cfgVer = cfg.Read("ConfigVersion", new UMFConfigString());
                    if (cfgVer != string.Empty && cfgVer != configVersion)
                    {
                        cfg.DeleteConfig(false);
                        StarbusterCustomTilemaps.Log("The config file was outdated and has been deleted. A new config will be generated.");
                    }

                    //cfg.Write("SupportsHotLoading", new UMFConfigBool(false)); //Uncomment if your mod can't be loaded once the game has started.
                    cfg.Write("ModDependencies", new UMFConfigStringArray(new string[] { "" })); //A comma separated list of mod/library names that this mod requires to function. Format: SomeMod:1.50,SomeLibrary:0.60
                    cfg.Read("LoadPriority", new UMFConfigString("Normal"));
                    cfg.Write("MinVersion", new UMFConfigString("0.53.0"));
                    cfg.Write("MaxVersion", new UMFConfigString("0.54.99999.99999")); //This will prevent the mod from being loaded after the next major UMF release
                    cfg.Write("UpdateURL", new UMFConfigString(""));
                    cfg.Write("ConfigVersion", new UMFConfigString(configVersion));

                    StarbusterCustomTilemaps.Log("Finished UMF Settings.");

                    //Add your settings here
                    cfg.Read("doNothing", new UMFConfigBool(false));
                    cfg.Write("showDebug", new UMFConfigBool(false)); // Forcing this off by default in the actual config file because I don't want to forget when recording.
                    showDebug = cfg.Read("showDebug", new UMFConfigBool(showDebug));

                    StarbusterCustomTilemaps.Log("Finished loading settings.");
                }
            }
            catch (Exception e)
            {
                StarbusterCustomTilemaps.Log("Error loading mod settings: " + e.Message + "(" + e.InnerException?.Message + ")");
            }
        }
    }
}