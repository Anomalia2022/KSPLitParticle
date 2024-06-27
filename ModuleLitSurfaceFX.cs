/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSPLitParticle
{
    public class ModuleLitSurfaceFX : PartModule
    {
        public const string MOUDLENAME = "ModuleLitSurfaceFX";

        [KSPField] public string ModuleID = MOUDLENAME;

        [KSPField] public string prefabName;

        [KSPField] public float maxRate;

        ModuleSurfaceFX surfaceFX;

        GameObject replacePrefab;

        List<ParticleSystem> emitters = new List<ParticleSystem>();



        public void Start()
        {
            surfaceFX = part.FindModelComponents<ModuleSurfaceFX>().First();
            replacePrefab = GameObject.Instantiate(AssetLoader.particlesPrefab);
            replacePrefab.SetActive(true);
            replacePrefab.GetComponentsInChildren<ParticleSystem>().ToList().ForEach(e =>
            {
                e.enableEmission = false;
                emitters.Add(e);
            });

            replacePrefab.GetComponentsInChildren<ParticleSystemRenderer>().ToList().ForEach(renderer =>
            {
                renderer.material.renderQueue = 4000;
            });
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || !GameSettings.SURFACE_FX || !surfaceFX)
            {
                return;
            }

            if (surfaceFX.ScaledFX <= surfaceFX.fxMax && surfaceFX.ScaledFX > 0.05f)
            {
                // 0 maxRate is min effect scale, maxRate maxed is full scale effect
                // fxMax maxed out should equal maxRate maxed
                // 0 = 0
                // 0.4 = 60
                // 0.8 = 120
                float rate = (surfaceFX.ScaledFX / surfaceFX.fxMax) * maxRate;

                replacePrefab.transform.position = surfaceFX.point;

                emitters.ForEach(e =>
                {
                    e.enableEmission = true;
                    e.emissionRate = rate;
                });

                // Keep these guys disabled for sure
                AssetBase.GetPrefab("SurfaceFX").active = false;
                AssetBase.GetPrefab("WaterFX").active = false;

            }
            else
            {
                // Shutdown effects
                emitters.ForEach(e =>
                {
                    e.enableEmission = false;
                    e.emissionRate = 0;
                });
            }

        }

    }
}
*/