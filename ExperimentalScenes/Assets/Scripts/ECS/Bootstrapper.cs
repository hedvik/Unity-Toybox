using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

// Based on https://connect.unity.com/p/part-3-unity-ecs-operating-on-entities
// This is basically where everything starts and is set up
public class Bootstrapper
{
    static int startEntityCount = 10000;

    // Archetypes are used to batch together entities with the same ComponentType
    public static EntityArchetype floatyCubeArchetype;
    public static MeshInstanceRenderer cubeLook;

    // Allows us to avoid MonoBehaviour usage to instantiate entities
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeBeforeScene()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>(); 

        // We need archetypes before we can create entities
        CreateArchetypes(entityManager); 
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void IntitializeAfterScene()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        cubeLook = GetLookFromPrototype("EntityInstanceRenderer");
        CreateEntities(entityManager);
    }

    private static void CreateArchetypes(EntityManager entityManager)
    {
        // ComponentType.Create<> is slightly more efficient than using typeof()
        // em.CreateArchetype(typeof(Position), typeof(Heading), typeof(Health), typeof(MoveSpeed));
        var position = ComponentType.Create<Position>();
        var heading = ComponentType.Create<Heading>();
        var moveSpeed = ComponentType.Create<MoveSpeed>();

        floatyCubeArchetype = entityManager.CreateArchetype(position, heading, moveSpeed);
    }

    private static void CreateEntities(EntityManager entityManager)
    {
        // if you spawn more entities, it's more performant to do it with NativeArray
        // if you want to spawn just one entity, do:
        // var entity = em.CreateEntity(EntityArchetype);
        var entities = new NativeArray<Entity>(startEntityCount, Allocator.Temp);

        // Spawns entities and attaches all components from the floatyCubes archetype
        entityManager.CreateEntity(floatyCubeArchetype, entities);

        // Setting up start values for the components
        for (int i = 0; i < startEntityCount; i++)
        {
            // Heading is a built-in Unity component that needs to be set as the default is (0,0,0) which we cannot look towards(apparently).
            entityManager.SetComponentData(entities[i], new Heading() { Value = new float3(0, 1, 0) });
            entityManager.SetComponentData(entities[i], new Position() { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(entities[i], new MoveSpeed() { speed = 10.0f });

            // This shared component decides the rendered look of the entity
            entityManager.AddSharedComponentData(entities[i], cubeLook);
        }

        // All NativeArray's need to be disposed of manually. This will not destroy any entities, just state that we will not be using the array anymore. 
        entities.Dispose(); 
        // As of now, this should mean that the entities are spawned in the world and ready to be injected into our systems. 
    }

    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var prototype = GameObject.Find(protoName);
        var result = prototype.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(prototype);
        return result;
    }
}
