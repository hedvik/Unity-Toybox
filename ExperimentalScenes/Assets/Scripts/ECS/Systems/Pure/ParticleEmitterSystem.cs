using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

// There are a lot of references to templates going on here so working with a standard ComponentSystem is a bit easier since Jobs cannot take references like the mesh.
// This is regardless the primary part of the particle system that could need optimisations.
// TODO: Entity pooling would make it possible to jobify this system though! 
public class ParticleEmitterSystem : ComponentSystem
{
    private struct EmitterGroup
    {
        public ComponentDataArray<ParticleEmitter> emitters;
        [ReadOnly] public ComponentDataArray<Position> positions;
        [ReadOnly] public int Length;
    }

    [Inject] EmitterGroup emitterGroup;

    protected override void OnUpdate()
    {
        var dt = Time.deltaTime;
        for (int i = 0; i < emitterGroup.Length; i++)
        {
            var emitter = emitterGroup.emitters[i];
            var position = emitterGroup.positions[i];
            emitter.emissionTimer += dt;
            if (emitter.emissionTimer >= emitter.emissionRate)
            {
                emitter.emissionTimer = 0;
                var particleEntities = new NativeArray<Entity>((int)emitterGroup.emitters[i].particlesPerEmission, Allocator.Temp);
                // Using EntityManager invalidates all injections so we need to cleanup afterwards
                EntityManager.CreateEntity(BootstrapperParticles.particleArchetype, particleEntities);
                for (int j = 0; j < emitter.particlesPerEmission; j++)
                {
                    // Generating emission vector
                    var direction = new float3(
                        Random.Range(-emitter.rangeX, emitter.rangeX),
                        Random.Range(-emitter.rangeY, emitter.rangeY),
                        Random.Range(-emitter.rangeZ, emitter.rangeZ)
                        );

                    EmitParticle(position.Value, (emitter.emissionDirection + direction) * emitter.initialSpeed, j, particleEntities);
                }
                // Cleanup
                particleEntities.Dispose();
                UpdateInjectedComponentGroups();
            }
            emitterGroup.emitters[i] = emitter;
        }
    }

    private void EmitParticle(float3 position, float3 velocity, int index, NativeArray<Entity> entities)
    {
        var rotatorComponent = new RotatorComponent();
        rotatorComponent.rotationSpeed = Random.Range(1.0f, 5.0f);
        rotatorComponent.direction = new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

        var newParticle = BootstrapperParticles.particleTemplate;
        newParticle.velocity = velocity;

        EntityManager.SetComponentData(entities[index], new Position() { Value = position });
        EntityManager.SetComponentData(entities[index], new Rotation() { Value = quaternion.identity });
        EntityManager.SetComponentData(entities[index], rotatorComponent);
        EntityManager.SetComponentData(entities[index], newParticle);
        EntityManager.AddSharedComponentData(entities[index], BootstrapperParticles.particleLook);
    }
}

// Above approach is slightly faster
//public class ParticleEmitterSystem : ComponentSystem
//{
//    private struct EmitterGroup
//    {
//        public ComponentDataArray<ParticleEmitter> emitters;
//        [ReadOnly] public ComponentDataArray<Position> positions;
//        [ReadOnly] public int Length;
//    }

//    [Inject] EmitterGroup emitterGroup;

//    protected override void OnUpdate()
//    {
//        var dt = Time.deltaTime;
//        for (int i = 0; i < emitterGroup.Length; i++)
//        {
//            var emitter = emitterGroup.emitters[i];
//            emitter.emissionTimer += dt;
//            if (emitter.emissionTimer >= emitter.emissionRate)
//            {
//                emitter.emissionTimer = 0;
//                for (int j = 0; j < emitterGroup.emitters[i].particlesPerEmission; j++)
//                {
//                    // Generating emission vector
//                    var direction = new float3(
//                        Random.Range(-emitterGroup.emitters[i].rangeX, emitterGroup.emitters[i].rangeX),
//                        Random.Range(-emitterGroup.emitters[i].rangeY, emitterGroup.emitters[i].rangeY),
//                        Random.Range(-emitterGroup.emitters[i].rangeZ, emitterGroup.emitters[i].rangeZ)
//                        );

//                    EmitParticle(emitterGroup.positions[i].Value, (emitterGroup.emitters[i].emissionDirection + direction) * emitterGroup.emitters[i].initialSpeed);
//                }
//            }
//            emitterGroup.emitters[i] = emitter;
//        }
//    }

//    private void EmitParticle(float3 position, float3 velocity)
//    {
//        PostUpdateCommands.CreateEntity(BootstrapperParticles.particleArchetype);

//        var rotatorComponent = new RotatorComponent();
//        rotatorComponent.rotationSpeed = Random.Range(1.0f, 5.0f);
//        rotatorComponent.direction = new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

//        var newParticle = BootstrapperParticles.particleTemplate;
//        newParticle.velocity = velocity;

//        PostUpdateCommands.SetComponent(new Position() { Value = position });
//        PostUpdateCommands.SetComponent(new Rotation() { Value = new quaternion() });
//        PostUpdateCommands.SetComponent(rotatorComponent);
//        PostUpdateCommands.SetComponent(newParticle);
//        PostUpdateCommands.AddSharedComponent(BootstrapperParticles.particleLook);
//    }
//}