using UniLinq;
using UnityEngine;
using File = System.IO.File;

namespace KSPLitParticle
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class AssetLoader : MonoBehaviour
    {
        static public GameObject particlesPrefab;

        // Load selected assets from assetbundles
        public void Awake()
        {
            DontDestroyOnLoad(this);

            // Find all LITPARTICLE confignodes
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("LITPARTICLE");
            
            // Parse all config nodes and load assetbundles if they exist
            foreach (UrlDir.UrlConfig config in configs)
            {
                Debug.Log("[LITPARTICLES] DEBUG TEST: " + config.config.GetValue("path"));
                foreach (string value in config.config.GetValues())
                {
                    Debug.Log("[LITPARTICLES] DEBUG TEST LOOP: " + value);
                }
                
                string path = config.config.GetValue("path");
                
                if (string.IsNullOrEmpty(path))
                {
                    Debug.Log($"[LITPARTICLES] AssetBundle path is null");
                    return;
                }
                
                using (WWW www = new WWW($"file://{KSPUtil.ApplicationRootPath}GameData/{path}"))
                {
                    if (!string.IsNullOrEmpty(www.error) || !File.Exists($"{KSPUtil.ApplicationRootPath}GameData/{path}"))
                    {
                        Debug.Log($"[LITPARTICLES] AssetBundle not found at path: GameData/{path}");   
                        return;
                    }
                    
                    Debug.Log($"[LITPARTICLES] AssetBundle at path GameData/{path} has begun loading!");
                    AssetBundle bundle = www.assetBundle;
                    
                    foreach (var asset in bundle.LoadAllAssets())
                    {
                        if (asset is GameObject)
                        {
                            config.config.GetValues("prefab").ToList().ForEach(prefab =>
                            {
                                if (((GameObject)asset).name == prefab)
                                {
                                    ((GameObject)asset).transform.SetParent(GameDatabase.Instance.transform);
                                    particlesPrefab = (GameObject)asset;
                                    ((GameObject)asset).SetActive(false);
                                }
                            });
                        }
                    }

                    bundle.Unload(false);
                    //www.Dispose();

                    this.enabled = false;
                }
                
            }
        }
    }
}