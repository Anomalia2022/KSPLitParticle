using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KSPLitParticle
{
    public class ModuleDeluge : PartModule
    {
        public const string MOUDLENAME = "ModuleDeluge";

        [KSPField] public string ModuleID = MOUDLENAME;

        [KSPField] public string transformName;

        [KSPField] public string prefabName;

        List<Transform> replaceTransforms;

        List<ParticleSystem> emitters = new List<ParticleSystem>();

        GameObject replacePrefab;

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
                if (replaceTransforms == null) { enableDelugeModule = false; return; }

                replaceTransforms.ForEach(transform =>
                {
                    replacePrefab = GameObject.Instantiate(AssetLoader.particlesPrefab);

                    replacePrefab.SetActive(true);

                    Debug.Log($"[{MOUDLENAME}] Adding {transform.name} ParticleSystem to Emitters List!");
                    ParticleSystem module = replacePrefab.GetComponentsInChildren<ParticleSystem>().First();
                    module.enableEmission = false;
                    emitters.Add(module);

                    // Set the renderQueue to Overlay
                    Debug.Log("Attempting to set renderQueue");
                    replacePrefab.GetComponentsInChildren<ParticleSystemRenderer>().First().material.renderQueue = 4000;

                    replacePrefab.transform.position = transform.transform.position;
                    replacePrefab.transform.rotation = transform.rotation;
                    replacePrefab.transform.parent = transform.parent;
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

            // Plugin must reinitialize all due to scene reload!
            /*if (HighLogic.LoadedSceneIsFlight && HighLogic.LoadedScene == GameScenes.FLIGHT)
            {

                // Change to recursive list to take into effect multiple transform names
                replaceTransforms = part.FindModelTransforms(transformName).ToList();
                if (replaceTransforms == null) { enableDelugeModule = false; return; }

                replaceTransforms.ForEach(transform =>
                {
                    replacePrefab = GameObject.Instantiate(AssetLoader.particlesPrefab);

                    replacePrefab.SetActive(true);

                    Debug.Log($"[{MOUDLENAME}] Adding {transform.name} ParticleSystem to Emitters List!");
                    ParticleSystem module = replacePrefab.GetComponentsInChildren<ParticleSystem>().First();
                    module.enableEmission = false;
                    emitters.Add(module);

                    // Set the renderQueue to Overlay
                    Debug.Log("Attempting to set renderQueue");
                    replacePrefab.GetComponentsInChildren<ParticleSystemRenderer>().First().material.renderQueue = 4000;

                    replacePrefab.transform.position = transform.transform.position;
                    replacePrefab.transform.rotation = transform.rotation;
                    replacePrefab.transform.parent = transform.parent;
                });


            }
            else
            {
                Fields["delugeActive"].guiActive = false;
                Fields["delugeActive"].guiActiveEditor = false;
            }

            if (!enableDelugeModule)
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
            }*/

            delugeActive = IsDelugeActive;

            //delugeInitialized = true;
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
                emitters.ForEach((emitter) =>
                {
                    emitter.enableEmission = true;
                });
            } else if(!delugeActive && emitters.Any())
            {
                emitters.ForEach(emitter =>
                {
                    emitter.enableEmission = false;
                });
            }

            if(delugeActive != IsDelugeActive)
            {
                EnableDeluge();
            }
        }

        public void OnDestroy()
        {
            Destroy(replacePrefab);
        }
    }
}