using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

// Based on https://connect.unity.com/p/part-3-unity-ecs-operating-on-entities
// This is basically where everything starts and is set up
public class BootstrapperImageTo3D
{
    static EntityArchetype pixelCubeArchetype;
    static Texture2D image;

    // Shared Components
    static MeshInstanceRenderer pixelCubeLook;
    static ImageColours imageColoursComponent;

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
        if (SceneManager.GetActiveScene().name.Contains("PureImageTo3D"))
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            pixelCubeLook = GetLookFromPrototype("EntityInstanceRenderer");
            image = GetImageFromPrototype("ImageContainerPrototype");
            imageColoursComponent = new ImageColours();
            imageColoursComponent.colors = image.GetPixels();
            imageColoursComponent.width = image.width;
            imageColoursComponent.height = image.height;
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
        var initTag = ComponentType.Create<InitializePositionTag>();
        //var cubeFloater = ComponentType.Create<CubeFloaterComponent>();
        //var cubeRotator = ComponentType.Create<CubeRotatorComponent>();

        pixelCubeArchetype = entityManager.CreateArchetype(position, transformMatrix, rotation, initTag);
    }

    private static void CreateEntities(EntityManager entityManager)
    {
        // if you spawn more entities, it's more performant to do it with NativeArray
        // if you want to spawn just one entity, do:
        // var entity = em.CreateEntity(EntityArchetype);
        var imageSize = imageColoursComponent.width * imageColoursComponent.height;
        var entities = new NativeArray<Entity>(imageSize, Allocator.Temp);

        // Spawns entities and attaches all components from the floatyCubes archetype
        entityManager.CreateEntity(pixelCubeArchetype, entities);

        // Setting up start values for the components
        for (int i = 0; i < imageSize; i++)
        {
            entityManager.SetComponentData(entities[i], new Position() { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(entities[i], new Rotation() { Value = quaternion.identity });

            // This shared component decides the rendered look of the entity
            entityManager.AddSharedComponentData(entities[i], pixelCubeLook);
            entityManager.AddSharedComponentData(entities[i], imageColoursComponent);
        }

        // All NativeArray's need to be disposed of manually. This will not destroy any entities, just state that we will not be using the array anymore. 
        entities.Dispose();
    }

    // One common approach to work between the scene view and Pure ECS is to create prototype GameObject's in the scene which we process here before removing them. 
    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var prototype = GameObject.Find(protoName);
        var result = prototype.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(prototype);
        return result;
    }

    private static Texture2D GetImageFromPrototype(string protoName)
    {
        var prototype = GameObject.Find(protoName);
        var result = prototype.GetComponent<ImageTextureComponent>().image;
        Object.Destroy(prototype);
        return result;
    }
}
