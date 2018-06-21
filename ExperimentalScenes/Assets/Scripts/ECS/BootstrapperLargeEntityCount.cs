using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

// Based on https://connect.unity.com/p/part-3-unity-ecs-operating-on-entities
// This is basically where everything starts and is set up
public class BootstrapperLargeEntityCount
{
    static int startEntityCount = 10000;

    // Archetypes are used to batch together entities with the same ComponentType's
    public static EntityArchetype floatyCubeArchetype;

    // MeshInstanceRenderer's are used to determine the "look" of a entity if we want to render it. 
    // This is a good candidate for GPU instancing in this case which requires the material to have instancing enabled. 
    public static MeshInstanceRenderer cubeLookBottom;
    public static MeshInstanceRenderer cubeLookTop;

    // Allows us to avoid MonoBehaviour usage to set things up
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeBeforeScene()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>(); 

        // We want archetypes before we create entities
        CreateArchetypes(entityManager); 
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void IntitializeAfterScene()
    {
        if (SceneManager.GetActiveScene().name.Contains("PureLargeEntityCount"))
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            cubeLookBottom = GetLookFromPrototype("EntityInstanceRenderer");
            cubeLookTop = GetLookFromPrototype("EntityInstanceRenderer2");
            CreateEntities(entityManager);
        }
    }

    private static void CreateArchetypes(EntityManager entityManager)
    {
        // ComponentType.Create<> is slightly more efficient than using typeof()
        // em.CreateArchetype(typeof(foo), typeof(bar), typeof(cake), typeof(cookie));
        // Note: A TransformMatrix is MANDATORY to actually render things properly
        var position = ComponentType.Create<Position>();
        var rotation = ComponentType.Create<Rotation>();
        var transformMatrix = ComponentType.Create<TransformMatrix>();
        var floater = ComponentType.Create<FloaterComponent>();
        var rotator = ComponentType.Create<RotatorComponent>();

        floatyCubeArchetype = entityManager.CreateArchetype(position, transformMatrix, rotation, floater, rotator);
    }

    private static void CreateEntities(EntityManager entityManager)
    {
        CreateAndSpawnEntities(
            entityManager, 
            startEntityCount / 2, 
            cubeLookBottom, 
            new float3(0, 0, 0), 
            new float3(0, 1, 0)
            );

        CreateAndSpawnEntities(
            entityManager,
            startEntityCount / 2,
            cubeLookTop,
            new float3(0, 100, 0),
            new float3(0, -1, 0)
            );

        // After this, the entities are spawned in the world and ready for processing. 
    }

    private static void CreateAndSpawnEntities(EntityManager entityManager, int count, MeshInstanceRenderer look, float3 startPos, float3 floatDirection)
    {
        // if you spawn more entities, it's more performant to do it with NativeArray
        // if you want to spawn just one entity, do:
        // var entity = em.CreateEntity(EntityArchetype);
        var entities = new NativeArray<Entity>(count, Allocator.Temp);

        // Spawns entities and attaches all components from the floatyCubes archetype
        entityManager.CreateEntity(floatyCubeArchetype, entities);

        // Setting up start values for the components
        for (int i = 0; i < count; i++)
        {
            var floaterComponent = new FloaterComponent();
            floaterComponent.floatSpeed = Random.Range(1.0f, 5.0f);
            floaterComponent.floatDirection = new float3(
                floatDirection.x == 0 ? Random.Range(-1.0f, 1.0f) : floatDirection.x,
                floatDirection.y == 0 ? Random.Range(-1.0f, 1.0f) : floatDirection.y,
                floatDirection.z == 0 ? Random.Range(-1.0f, 1.0f) : floatDirection.z
                );

            var rotatorComponent = new RotatorComponent();
            rotatorComponent.rotationSpeed = floaterComponent.floatSpeed;
            rotatorComponent.direction = new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

            entityManager.SetComponentData(entities[i], new Position() { Value = startPos });
            entityManager.SetComponentData(entities[i], new Rotation() { Value = new quaternion() });
            entityManager.SetComponentData(entities[i], rotatorComponent);
            entityManager.SetComponentData(entities[i], floaterComponent);

            // This shared component decides the rendered look of the entity
            entityManager.AddSharedComponentData(entities[i], look);
        }

        // All NativeArray's need to be disposed of manually. This will not destroy any entities, just state that we will not be using the array anymore. 
        entities.Dispose();
    }

    // One common approach to work between the scene view and Pure ECS is to create prototype GameObject's in the scene which we process here before removing them. 
    // Note: Prefabs with GameObjectEntity can also be instantiated by the EntityManager which is fairly convenient. 
    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var prototype = GameObject.Find(protoName);
        var result = prototype.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(prototype);
        return result;
    }
}
