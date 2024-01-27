using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CollisionHandler
{
    public class ParentsHolder<T>
    {
        public T ParentA { get; private set; }
        public T ParentB { get; private set; }
        
        public ParentsHolder(T parentA, T parentB)
        {
            ParentA = parentA;
            ParentB = parentB;
        }
    }
    
    public class CelestialCollisionInfo
    {
        public ParentsHolder<CelestialBodyScript> Parents { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float Mass { get; private set; }
        public Vector3 Point { get; private set; }
        public CelestialType ResultingType { get; private set; }

        
        public CelestialCollisionInfo(CelestialBodyScript obj1, CelestialBodyScript obj2)
        {
            Parents = new ParentsHolder<CelestialBodyScript>(obj1, obj2);
        }
        
        public void Config(Vector3 velocity, float mass, Vector3 point, CelestialType resultingType)
        {
            Velocity = velocity;
            Mass = mass;
            Point = point;
            ResultingType = resultingType;
        }
        
        /// <summary>
        /// DoCollision invalidates parents (as they are destroyed)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CelestialBodyScript DoCollision()
        {
            GameObject prefab;
            switch (ResultingType)
            {
                case CelestialType.Star:
                    prefab = GalaxyConfigurator.Instance.StarPrefab;
                    break;
                case CelestialType.Planet:
                    prefab = GalaxyConfigurator.Instance.PlanetPrefab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            
            CelestialBodyScript celestialBody = Object.Instantiate(prefab, Point, Quaternion.identity, GalaxyConfigurator.Instance.PlanetsParent).GetComponent<CelestialBodyScript>();
            Parents.ParentA.DestroySelf();
            Parents.ParentB.DestroySelf();

            celestialBody.Config(ResultingType, Mass, Velocity);

            celestialBody.Create();

            return celestialBody;
        }

    }

    public static CelestialCollisionInfo CalculateCollision<T>(T obj1, T obj2) where T : CelestialBodyScript
    {
        Vector3 position = (obj1.transform.position - obj2.transform.position) / 2 + obj2.transform.position;
        return CalculateCollision(obj1, obj2, position);
    }
    
    public static CelestialCollisionInfo CalculateCollision<T>(T obj1, T obj2, Vector3 position) where T : CelestialBodyScript
    {
        CelestialCollisionInfo collisionInfo = new(obj1, obj2);
        
        if (obj1.CelestialType == obj2.CelestialType)
        {
            Vector3 resultingVelocity = CalculateResultingCollisionVelocity(obj1, obj2);
            float resultingMass = obj1.Mass + obj2.Mass;
            
            collisionInfo.Config(resultingVelocity, resultingMass, position, obj1.CelestialType);
            
            return collisionInfo;
        }
        
        return null;
    }

    private static Vector3 CalculateResultingCollisionVelocity<T>(T obj1, T obj2) where T : CelestialBodyScript
    {
        Vector3 incoming1 = obj1.Velocity;
        Vector3 incoming2 = obj2.Velocity;

        float totalMass = obj1.Mass + obj2.Mass;

        return (incoming1 * obj1.Mass + incoming2 * obj2.Mass) / totalMass;
    }

    
    public static T CalculateHeaviestObject<T>(T obj1, T obj2) where T : CelestialBodyScript
    {
        return obj1.Mass >= obj2.Mass ? obj1 : obj2;
    }

}