using System;
using System.Collections.Generic;
using KSP.IO;
using UniLinq;
using UnityEngine;
using Enumerable = System.Linq.Enumerable;
using File = System.IO.File;
using Object = System.Object;

namespace KSPLitParticle
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class AssetLoader : MonoBehaviour
    {

        // Load selected assets from assetbundles
        public void Awake()
        {
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

                    // Parent of all particle prefabs aka the ParticleDatabase
                    GameObject database = new GameObject("ParticleDatabase");
                    
                    DontDestroyOnLoad(database);
                    foreach (var asset in bundle.LoadAllAssets())
                    {
                        if (asset is GameObject)
                        {
                            config.config.GetValues("prefab").ToList().ForEach(prefab =>
                            {
                                if (((GameObject)asset).name == prefab)
                                {
                                    ((GameObject)asset).SetActive(false);
                                    DontDestroyOnLoad(asset);
                                    ((GameObject)asset).transform.SetParent(database.transform);
                                }
                            });
                        }
                    }
                    
                    bundle.Unload(false);
                    www.Dispose();
                }
                
            }
                

            /*using (WWW www = new WWW("file://" + KSPUtil.ApplicationRootPath + $"GameData/{path}"))
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.Log($"[KSPLitParticles] {path} assetbundle not found!");
                    return;
                }

                Debug.Log($"[KSPLitParticles] In the WWW!");
                AssetBundle bundle = www.assetBundle;

                bundle.LoadAllAssets<Material>();
                bundle.LoadAllAssets<GameObject>();
                foreach (GameObject asset in bundle.LoadAllAssets<GameObject>())
                {
                    Debug.Log($"[KSPLitParticles] GameObject found by name: {asset.name}");
                }

                foreach (string asset in bundle.GetAllAssetNames())
                {
                    Debug.Log($"[KSPLitParticles] AssetName found with name: {asset}");
                }

                bundle.Unload(false);
                www.Dispose();
            }*/
        }
    }
}