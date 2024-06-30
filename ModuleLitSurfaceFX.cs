/*using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarshipExpansionProject.Utils;

namespace KSPLitParticle
{


    public class ModuleLitSurfaceFX : VesselModule
    {

        public const string MOUDLENAME = "ModuleLitSurfaceFX";

        [KSPField] public string ModuleID = MOUDLENAME;

        [KSPField] public string prefabName;

        [KSPField] public float maxRate = 75f;

        //List<GameObject> particles = new List<GameObject>();
        List<ModuleEngines> moduleEngines;
        List<ParticleSystem> systems = new List<ParticleSystem>();
        GameObject particle;
        
        float index = -1;

        public void Start()
        {
            moduleEngines = vessel.FindPartModulesImplementing<ModuleEngines>();

            index = moduleEngines.Count;
            if (index == 0)
                return;

            // Store and disable all basic KSP Particle Effects
            GameObject SurfaceFX = AssetBase.GetPrefab("SurfaceFX");
            GameObject WaterFX = AssetBase.GetPrefab("WaterFX");
            foreach (Transform transform in SurfaceFX.transform)
            {
                transform.gameObject.SetActive(false);
            }
            foreach(Transform transform in WaterFX.transform) 
            { 
                transform.gameObject.SetActive(false);
            }

            // Future Gather our better lit FX for WaterFX




            // Instantiate and set up the particle prefab
            particle = GameObject.Instantiate(AssetLoader.particlesPrefab);
            particle.SetActive(true);
            foreach (Transform transform in particle.transform)
            {
                // Zero out the emitters localPositions so they don't get out of wack.
                transform.localPosition = Vector3.zero;
            }
            particle.transform.position = SurfaceFX.transform.position;
            particle.transform.localPosition = new Vector3(0, 0, 0);

            particle.GetComponentsInChildren<ParticleSystem>().ToList().ForEach(system =>
            {
                systems.Add(system);
                system.emissionRate = 0;
            });


            normalLine = new Vector3Renderer(vessel.rootPart, "Normal Line", "Normal Line", Color.yellow);
            forwardLine = new Vector3Renderer(vessel.rootPart, "Forward Line", "Forward Line", Color.blue);
            rightLine = new Vector3Renderer(vessel.rootPart, "Right Line", "Right Line", Color.red);
            upLine = new Vector3Renderer(vessel.rootPart, "Up Line", "Up Line", Color.green);

        }



        Vector3Renderer normalLine;
        Vector3Renderer forwardLine;
        Vector3Renderer rightLine;
        Vector3Renderer upLine;

        Vector3 prevPos;

        public void FixedUpdate()
        {

            if (!particle || !systems.Any())
                return;

            List<ModuleSurfaceFX> surfaceFX = new List<ModuleSurfaceFX>();
            List<Vector3> pointPos = new List<Vector3>();
            List<Vector3> normalVector = new List<Vector3>();

            float activeCount = 0f;
            float fxScaleTotal = 0f;
            float fxScaleMaxTotal = 0f;

            moduleEngines.Where(e => e.getIgnitionState == true).ToList().ForEach(engine =>
            {
                fxScaleTotal += engine.part.GetComponentsInChildren<ModuleSurfaceFX>().First().fxScale;
                fxScaleMaxTotal += engine.part.GetComponentsInChildren<ModuleSurfaceFX>().First().fxMax;
                surfaceFX.Add(engine.part.GetComponentsInChildren<ModuleSurfaceFX>().First());

                activeCount++;
            });

            float fxScale = fxScaleTotal / activeCount;
            float fxMax = fxScaleMaxTotal / activeCount;

            float rate = (fxScale / fxMax) * maxRate;


            // If there are none active lets disable the effects to be sure otherwise run the effects equal to the rate
            if(surfaceFX.Where(effect => effect.hit == ModuleSurfaceFX.SurfaceType.Terrain).ToList().Count >= 1)
            {
                systems.ForEach(system =>
                {
                    system.emissionRate = rate;
                });
            } else
            {
                systems.ForEach(system =>
                {
                    system.emissionRate = 0f;
                });
            }

            surfaceFX.Where(effect => effect.raycastHit == true).ToList().ForEach(effect =>
            {
                pointPos.Add(effect.point);
                normalVector.Add(effect.normal);
            });

            if (!pointPos.Any() || !normalVector.Any())
                return;

            Vector3 position = pointPos.First();
            Vector3 normal = normalVector.First();

            bool skip = true;
            pointPos.ForEach(point =>
            {
                // Skip the first element, too lazy to use a for loop bruh
                if(!skip)
                {
                    position += point;
                }
                skip = false;
            });
            skip = false;
            normalVector.ForEach(direction =>
            {
                if(!skip)
                {
                    normal += direction;
                }
                skip = false;
            });

            position = position / pointPos.Count();
            normal = normal / normalVector.Count();

            if(prevPos == null)
            {
                prevPos = position;
            }


            // Handle 0 speed
            float speed = (float)(vessel.horizontalSrfSpeed * TimeWarp.deltaTime);

            Vector3 lerped = Vector3.Lerp(prevPos, position, speed);
            prevPos = lerped;

            particle.transform.position = lerped;
            particle.transform.up = normal;
            
            normalLine.DrawLine(particle.transform, position, 2f, true);
            rightLine.DrawLine(particle.transform, particle.transform.right, 2f, true);
            forwardLine.DrawLine(particle.transform, particle.transform.forward, 2f, true);
            upLine.DrawLine(particle.transform, particle.transform.up, 2f, true);


            // Rotation of the particle based of the average normal.


        }

    }

}*/