using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace B9_Aerospace_ProceduralWings
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class StaticWingGlobals : MonoBehaviour
    {
        public static List<WingTankConfiguration> wingTankConfigurations = new List<WingTankConfiguration>();

        public static Shader wingShader;

        private static string _bundlePath;
        public string BundlePath
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXPlayer:
                        return _bundlePath + Path.DirectorySeparatorChar + "pwings_macosx.bundle";
                    case RuntimePlatform.WindowsPlayer:
                        return _bundlePath + Path.DirectorySeparatorChar + "pwings_windows.bundle";
                    case RuntimePlatform.LinuxPlayer:
                        return _bundlePath + Path.DirectorySeparatorChar + "pwings_linux.bundle";
                    default:
                        return _bundlePath + Path.DirectorySeparatorChar + "pwings_windows.bundle";
                }
            }
        }

        private void Awake()
        {
            _bundlePath = KSPUtil.ApplicationRootPath + "GameData" +
                                                    Path.DirectorySeparatorChar +
                                                    "B9_Aerospace_ProceduralWings" + Path.DirectorySeparatorChar + "AssetBundles";
        }

        public void Start()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("ProceduralWingFuelSetups"))
            {
                ConfigNode[] fuelNodes = node.GetNodes("FuelSet");
                for (int i = 0; i < fuelNodes.Length; ++i)
                {
                    wingTankConfigurations.Add(new WingTankConfiguration(fuelNodes[i]));
                }
            }
            LOG.trace("start bundle load process");

            StartCoroutine(LoadBundleAssets());
        }

        public IEnumerator LoadBundleAssets()
        {
            LOG.trace("Aquiring bundle data");
            AssetBundle shaderBundle = AssetBundle.LoadFromFile(BundlePath);

            if (shaderBundle != null)
            {
                Shader[] objects = shaderBundle.LoadAllAssets<Shader>();
                for (int i = 0; i < objects.Length; ++i)
                {
                    if (objects[i].name == "KSP/Specular Layered")
                    {
                        wingShader = objects[i];
                        LOG.info($"Wing shader \"{wingShader.name}\" loaded. Shader supported? {wingShader.isSupported}");
                    }
                }

                yield return null;
                yield return null; // unknown how neccesary this is

                LOG.trace("unloading bundle");
                shaderBundle.Unload(false); // unload the raw asset bundle
            }
            else
            {
                LOG.error("Found no asset bundle to load");
            }
        }
 
        private static readonly KSPe.Util.Log.Logger LOG = KSPe.Util.Log.Logger.CreateForType<StaticWingGlobals>("B9 PWings", "StaticWingGlobals");
    }
}