using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapperParticles
{
    public static EntityArchetype particleArchetype;
    public static EntityArchetype particleEmitterArchetype;

    public static MeshInstanceRenderer particleLook;
    public static Particle particleTemplate;
    public static ParticleEmitter particleEmitterTemplate;

    // Allows us to avoid MonoBehaviour usage to set things up
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeBeforeScene()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        CreateArchetypes(entityManager);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void IntitializeAfterScene()
    {
        if (SceneManager.GetActiveScene().name.Contains("PureParticles"))
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            GetSettingsFromPrototype("ParticleSettings");
            CreateEntities(entityManager);
        }
    }

    private static void CreateArchetypes(EntityManager entityManager)
    {
        var position = ComponentType.Create<Position>();
        var rotation = ComponentType.Create<Rotation>();
        var transformMatrix = ComponentType.Create<TransformMatrix>();
        var rotator = ComponentType.Create<RotatorComponent>();
        var particle = ComponentType.Create<Particle>();
        var particleEmitter = ComponentType.Create<ParticleEmitter>();

        particleArchetype = entityManager.CreateArchetype(position, transformMatrix, rotation, particle, rotator);
        particleEmitterArchetype = entityManager.CreateArchetype(position, transformMatrix, particleEmitter);
    }

    private static void CreateEntities(EntityManager entityManager)
    {
        var emitterEntity = entityManager.CreateEntity(particleEmitterArchetype);
        entityManager.SetComponentData(emitterEntity, new Position() { Value = particleEmitterTemplate.emitterPosition });
        entityManager.SetComponentData(emitterEntity, particleEmitterTemplate);
    }

    private static void GetSettingsFromPrototype(string protoName)
    {
        var prototype = GameObject.Find(protoName);

        particleLook = prototype.GetComponent<MeshInstanceRendererComponent>().Value;
        particleTemplate = prototype.GetComponent<ParticleComponent>().Value;
        particleTemplate.inverseMass = 1 / particleTemplate.mass;
        particleEmitterTemplate = prototype.GetComponent<ParticleEmitterComponent>().Value;

        Object.Destroy(prototype);
    }
}
