using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace KSPLitParticle
{
    public class ModuleDeluge : PartModule
    {
        public const string MOUDLENAME = "ModuleDeluge";

        [KSPField] public string ModuleID = MOUDLENAME;

        [KSPField] public string transformName;

        [KSPField] public string prefabName;

        [KSPField] public float hitRange = 9;

        [KSPField] public int renderQueue = 3500;

        [KSPField] public float maxSolarFlux = 1400; // Roughly the max seen on stock kerbin 20km up

        [KSPField] public float maxEnginePerformance; // How much kN of thrust is required for a deluge plume to reach full volume, if a weaker engine or less engines is fired the plume generated should also reflect this

        List<Transform> replaceTransforms;

        [KSPField] public Vector3 rotationOffset;

        List<ParticleSystem> emitters = new List<ParticleSystem>();

        List<GameObject> replacedPrefabs = new List<GameObject>();

        private bool enableDelugeModule = true;

        public bool delugeInitialized = false;

        [KSPField(guiActive = false, isPersistant = true)]
        private bool IsDelugeActive;

        [KSPField(guiActive = true, guiActiveUnfocused = true, guiName = "Toggle Deluge", isPersistant = false, unfocusedRange = 250f)]
        [UI_Toggle(affectSymCounterparts = UI_Scene.All, disabledText = "Off", enabledText = "On", scene = UI_Scene.All)]
        public bool delugeActive;

        [KSPAction(guiName = "Toggle Delulge")]
        public void ToggleDelugeAction(KSPActionParam param)
        {
            delugeActive = !delugeActive;
            EnableDeluge();
        }

        [KSPAction(guiName = "Enable Deluge")]
        public void EnableDelugeAction(KSPActionParam param)
        {
            delugeActive = true;
            EnableDeluge();
        }

        [KSPAction(guiName = "Disable Deluge")]
        public void DisableDelugeAction(KSPActionParam param)
        {
            delugeActive = false;
            EnableDeluge();
        }


        public void Start()
        {

            
            if(HighLogic.LoadedSceneIsFlight && HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                // Change to recursive list to take into effect multiple transform names
                replaceTransforms = part.FindModelTransforms(transformName).ToList();
                if (replaceTransforms == null) { enableDelugeModule = false; }

                replaceTransforms.ForEach(transform =>
                {
                    if(!AssetLoader.particlesPrefab.ContainsKey(prefabName))
                    {
                        enableDelugeModule = false; return;
                    }

                    GameObject replacePrefab = GameObject.Instantiate(AssetLoader.particlesPrefab[prefabName]);

                    replacePrefab.SetActive(true);

                    replacePrefab.GetComponentsInChildren<ParticleSystem>().ToList().ForEach(e =>
                    {
                        e.enableEmission = false;
                        emitters.Add(e);
                    });

                    Debug.Log($"[{MOUDLENAME}] Adding {transform.name} ParticleSystem to Emitters List!");

                    // Set the renderQueue to Overlay
                    Debug.Log("Attempting to set renderQueue");
                    replacePrefab.GetComponentsInChildren<ParticleSystemRenderer>().ToList().ForEach(renderer =>
                    {
                        renderer.material.renderQueue = renderQueue;
                    });

                    replacePrefab.transform.position = transform.transform.position;
                    replacePrefab.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotationOffset);
                    replacePrefab.transform.parent = transform.parent;
                    replacedPrefabs.Add(replacePrefab);
                });

            } 
            else
            {
                Fields["delugeActive"].guiActive = false;
                Fields["delugeActive"].guiActiveEditor = false;
            }

            if(!enableDelugeModule)
            {
                Fields["delugeActive"].guiActive = false;
                Fields["delugeActive"].guiActiveEditor = false;

                Actions["ToggleDelugeAction"].active = false;
                Actions["ToggleDelugeAction"].activeEditor = false;

                Actions["EnableDelugeAction"].active = false;
                Actions["EnableDelugeAction"].activeEditor = false;

                Actions["DisableDelugeAction"].active = false;
                Actions["DisableDelugeAction"].activeEditor = false;

                delugeActive = false;
            }

            if(Fields.TryGetFieldUIControl("delugeActive", out UI_Toggle delugeActiveField))
            {
                delugeActiveField.onFieldChanged += OnDelugeActivateField;
            }

            delugeInitialized = true;
        }

        public override void OnLoad(ConfigNode node)
        {
            delugeActive = IsDelugeActive;
        }

        private void OnDelugeActivateField(BaseField field, object sender)
        {
            EnableDeluge();
        }

        public void EnableDeluge()
        {
            if (!enableDelugeModule)
                return;

            if(delugeActive && emitters.Any())
            {
                IsDelugeActive = true;
            }
            else
            {
                IsDelugeActive = false;
            }

        }

        public void FixedUpdate()
        {
            if (!enableDelugeModule || !delugeInitialized)
                return;

            if(delugeActive && emitters.Any())
            {
                emitters.ForEach(e => {
                    e.enableEmission = true;
                });

            }
            else if(!delugeActive && emitters.Any())
            {
                emitters.ForEach(e => {
                    e.enableEmission = false;
                });
            }


            List<ModuleSurfaceFX> engineSurfaceFX = new List<ModuleSurfaceFX>();
            Vector3 delugeLocation = part.transform.position;

            // Find ModuleSurfaceFX of all actively running engines on all nearby engines and check if they make contact with the plate
            FlightGlobals.VesselsLoaded.ForEach(v =>
            {
                // All engines that are ingited and above 0 throttle whether it be the active vessel, kOS, or Independent Throttle
                v.FindPartModulesImplementing<ModuleEngines>().Where(e => e.getIgnitionState == true && e.currentThrottle > 0).ToList().ForEach(activeEngine =>
                {
                    if(activeEngine.part.GetComponentsInChildren<ModuleSurfaceFX>().Where(fx => Vector3.Distance(delugeLocation, fx.point) <= hitRange && fx.hit != ModuleSurfaceFX.SurfaceType.None) != null)
                    {
                        engineSurfaceFX.Add(activeEngine.part.GetComponentsInChildren<ModuleSurfaceFX>().Where(fx => Vector3.Distance(delugeLocation, fx.point) <= hitRange && fx.hit != ModuleSurfaceFX.SurfaceType.None).First());
                    }
                    // If the active engine's ModuleSurfaceFX hit point is within X meters and is hitting the surface not blasting the pad above/left behind point when out of range it will add to the list of engineSurfaceFX
                });

            });

            // ----- DETECT ENGINE POWER LATER -----
            // Found engines actively hitting deluge system
            if(engineSurfaceFX.Any())
            {
                emitters.ForEach(e =>
                {
                    e.enableEmission = true;
                });
            }


            if (delugeActive != IsDelugeActive)
            {
                EnableDeluge();
            }
        }

        public void OnDestroy()
        {
            // Destroy any prefab to ensure a safe closure on scene destruction
            replacedPrefabs.ForEach(prefab =>
            {
                Destroy(prefab);
            });

            // Reinstantiate the empty list
            replacedPrefabs = new List<GameObject>();
        }
    }
}