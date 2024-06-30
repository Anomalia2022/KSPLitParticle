using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using File = System.IO.File;

namespace KSPLitParticle
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class AssetLoader : MonoBehaviour
    {
        static public Dictionary<string, GameObject> particlesPrefab = new Dictionary<string, GameObject>();

        // Load selected assets from assetbundles
        public void Awake()
        {
            // Load the assets
            ReloadAsset();
        }

        public void FixedUpdate()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.P))
            {
                // Destroy all old prefabs
                particlesPrefab.Values.ToList().ForEach(p => { 
                    Destroy(p);
                });
                
                // Reinstantiate the list
                particlesPrefab = new Dictionary<string, GameObject>();

                // Reload the Assets
                ReloadAsset();
            }
        }

        public void ReloadAsset()
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

                    // Fix the file call after looking for the file dumbass
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
                                    GameObject loadedAsset = (GameObject)asset;

                                    loadedAsset.transform.SetParent(GameDatabase.Instance.transform);

                                    loadedAsset.SetActive(false);

                                    particlesPrefab.Add(loadedAsset.name, loadedAsset);

                                }
                            });
                        }
                    }

                    bundle.Unload(false);
                }

            }

            Debug.Log("[KSPLitParticles] All Loaded Assets:");
            particlesPrefab.Keys.ToList().ForEach(key =>
            {
                Debug.Log($"Key value {key} found associated with prefab {particlesPrefab[key].name}");
            });
        }
    }
}