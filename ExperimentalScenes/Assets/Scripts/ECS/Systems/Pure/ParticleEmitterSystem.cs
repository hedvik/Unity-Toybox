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

// Jobified example, hard to say which one is faster as both fluctuate in terms of frame latency per test :p
// Being able to add MeshInstanceRenderer directly during creation would remove the need for MeshInstanceRendererAdderSystem. 
//[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
//public class ParticleEmitterSystem : JobComponentSystem
//{
//    private struct EmitterGroup
//    {
//        public ComponentDataArray<ParticleEmitter> emitters;
//        public readonly int Length;
//    }

//    //[BurstCompile]
//    private struct EmissionJob : IJobParallelFor
//    {
//        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;
//        [ReadOnly] public EntityArchetype particleArchetype;
//        [ReadOnly] public NativeArray<RotatorComponent> randomRotators;
//        [ReadOnly] public NativeArray<float3> randomDirections;
//        [ReadOnly] public ParticleEmitter emitter;
//        [ReadOnly] public Position emitterPosition;
//        [ReadOnly] public int offset;

//        public void Execute(int i)
//        {
//            commandBuffer.CreateEntity(particleArchetype);
//            var rotatorComponent = randomRotators[i * offset];

//            var newParticle = BootstrapperParticles.particleTemplate;
//            newParticle.velocity = (emitter.emissionDirection + randomDirections[i * offset]) * emitter.initialSpeed;

//            commandBuffer.SetComponent(new Position() { Value = emitterPosition.Value });
//            commandBuffer.SetComponent(new Rotation() { Value = quaternion.identity });
//            commandBuffer.SetComponent(rotatorComponent);
//            commandBuffer.SetComponent(newParticle);
//            commandBuffer.AddSharedComponent(new NeedsMeshInstanceRendererTag());
//        }
//    }

//    [Inject] EmitterGroup emitterGroup;
//    [Inject] EndFrameBarrier endFrameBarrier;

//    private NativeArray<float3> randomEmissionDirections;
//    private NativeArray<RotatorComponent> randomRotatorComponents;

//    protected override void OnStartRunning()
//    {
//        // For now we only support one emitter for the sake of simplicity
//        var emitter = BootstrapperParticles.particleEmitterTemplate;
//        randomEmissionDirections = new NativeArray<float3>(emitter.maxParticles, Allocator.Persistent);
//        randomRotatorComponents = new NativeArray<RotatorComponent>(emitter.maxParticles, Allocator.Persistent);

//        for (int i = 0; i < emitter.maxParticles; i++)
//        {
//            randomEmissionDirections[i] = new float3
//            (
//                Random.Range(-emitter.rangeX, emitter.rangeX),
//                Random.Range(-emitter.rangeY, emitter.rangeY),
//                Random.Range(-emitter.rangeZ, emitter.rangeZ)
//            );

//            var rotator = new RotatorComponent();
//            rotator.rotationSpeed = Random.Range(1.0f, 5.0f);
//            rotator.direction = new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
//            randomRotatorComponents[i] = rotator;
//        }
//    }

//    protected override void OnStopRunning()
//    {
//        randomEmissionDirections.Dispose();
//        randomRotatorComponents.Dispose();
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var jobHandle = inputDeps;
//        var dt = Time.deltaTime;
//        for (int i = 0; i < emitterGroup.Length; i++)
//        {
//            var emitter = emitterGroup.emitters[i];
//            var position = emitterGroup.emitters[i].emitterPosition;
//            emitter.emissionTimer += dt;
//            if (emitter.emissionTimer >= emitter.emissionRate)
//            {
//                emitter.emissionTimer = 0;
//                var emissionJob = new EmissionJob()
//                {
//                    commandBuffer = endFrameBarrier.CreateCommandBuffer(),
//                    particleArchetype = BootstrapperParticles.particleArchetype,
//                    randomDirections = randomEmissionDirections,
//                    randomRotators = randomRotatorComponents,
//                    emitter = emitter,
//                    emitterPosition = new Position() { Value = position },
//                    offset = Random.Range(0, emitter.maxParticles / (int)emitter.particlesPerEmission)
//                };
//                jobHandle = emissionJob.Schedule((int)emitter.particlesPerEmission, 1, inputDeps);
//            }
//            emitterGroup.emitters[i] = emitter;
//        }

//        return jobHandle;
//    }
//}

//The fastest main thread approach so far
public class ParticleEmitterSystem : ComponentSystem
{
    private struct EmitterGroup
    {
        public ComponentDataArray<ParticleEmitter> emitters;
        [ReadOnly] public ComponentDataArray<Position> positions;
        public readonly int Length;
    }

    [Inject] private EmitterGroup emitterGroup;

    private float3[] randomEmissionDirections;
    private RotatorComponent[] randomRotatorComponents;

    protected override void OnStartRunning()
    {
        // For now we only support one emitter for the sake of simplicity
        var emitter = BootstrapperParticles.particleEmitterTemplate;
        randomEmissionDirections = new float3[emitter.maxParticles];
        randomRotatorComponents = new RotatorComponent[emitter.maxParticles];

        for (int i = 0; i < emitter.maxParticles; i++)
        {
            randomEmissionDirections[i] = new float3
            (
                Random.Range(-emitter.rangeX, emitter.rangeX),
                Random.Range(-emitter.rangeY, emitter.rangeY),
                Random.Range(-emitter.rangeZ, emitter.rangeZ)
            );

            randomRotatorComponents[i].rotationSpeed = Random.Range(1.0f, 5.0f);
            randomRotatorComponents[i].direction = new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        }
    }

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
                var particleEntities = new NativeArray<Entity>((int)emitter.particlesPerEmission, Allocator.Temp);
                // Using EntityManager invalidates all injections so we need to cleanup afterwards
                EntityManager.CreateEntity(BootstrapperParticles.particleArchetype, particleEntities);
                for (int j = 0; j < emitter.particlesPerEmission; j++)
                {
                    // "Generating" emission vector
                    var direction = randomEmissionDirections[Random.Range(0, randomEmissionDirections.Length)];
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
        var rotatorComponent = randomRotatorComponents[Random.Range(0, randomRotatorComponents.Length)];

        var newParticle = BootstrapperParticles.particleTemplate;
        newParticle.velocity = velocity;

        EntityManager.SetComponentData(entities[index], new Position() { Value = position });
        EntityManager.SetComponentData(entities[index], new Rotation() { Value = quaternion.identity });
        EntityManager.SetComponentData(entities[index], rotatorComponent);
        EntityManager.SetComponentData(entities[index], newParticle);
        EntityManager.AddSharedComponentData(entities[index], BootstrapperParticles.particleLook);
    }
}