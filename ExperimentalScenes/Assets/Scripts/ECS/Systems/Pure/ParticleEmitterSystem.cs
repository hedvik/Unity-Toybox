using System;
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

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
public class ParticleEmitterSystem : JobComponentSystem
{
    private struct EmitterGroup
    {
        [ReadOnly] public int Length;
        //[ReadOnly] public ComponentDataArray<Position> positions;
        public ComponentDataArray<ParticleEmitter> emitters;
    }

    private struct ParticleGroup
    {
        [ReadOnly] public int Length;
        [ReadOnly] public ComponentDataArray<DisabledComponentTag> disabledTags;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Particle> particles;
        [ReadOnly] public EntityArray entities;
    }

    //[BurstCompile] 
    // BurstCompile does not like to deal with add/remove/set and entityarray
    private struct EmissionJob : IJobParallelFor
    {
        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;
        [ReadOnly] public EntityArray inactiveEntities;
        [ReadOnly] public float3 emitterPosition;
        public ComponentDataArray<Particle> particles;
        public ComponentDataArray<Position> positions;

        public void Execute(int i)
        {
            var particle = particles[i];
            var position = positions[i];

            particle.force = new float3(0, 0, 0);
            particle.acceleration = new float3(0, 0, 0);
            particle.velocity = particle.initialVelocity;
            particle.lifeTimer = particle.initialLifeTime;

            position.Value = emitterPosition;

            particles[i] = particle;
            positions[i] = position;
            commandBuffer.RemoveComponent<DisabledComponentTag>(inactiveEntities[i]);
        }
    }

    [Inject] private EmitterGroup emitterGroup;
    [Inject] private ParticleGroup inactiveParticles;
    [Inject] private EndFrameBarrier endFrameBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dt = Time.deltaTime;
        var jobHandle = inputDeps;
        for (int i = 0; i < emitterGroup.Length; i++)
        {
            var emitter = emitterGroup.emitters[i];
            emitter.emissionTimer += dt;
            if (emitter.emissionTimer >= emitter.emissionRate)
            {
                emitter.emissionTimer -= emitter.emissionRate;
                var emissionJob = new EmissionJob
                {
                    commandBuffer = endFrameBarrier.CreateCommandBuffer(),
                    inactiveEntities = inactiveParticles.entities,
                    emitterPosition = emitter.emitterPosition,
                    particles = inactiveParticles.particles,
                    positions = inactiveParticles.positions
                };
                var amountOfJobs = (inactiveParticles.entities.Length < emitter.particlesPerEmission) ? inactiveParticles.entities.Length : (int)emitter.particlesPerEmission;
                jobHandle = emissionJob.Schedule(amountOfJobs, 1, inputDeps);
            }
            emitterGroup.emitters[i] = emitter;
        }
        return jobHandle;
    }
}

// Failed attempt at jobifying. 
// Throws exception whenever I try to set a component despite it being on the entity due to the archetype.
// This is due to a peculiarity apparently:https://github.com/Unity-Technologies/EntityComponentSystemSamples/search?q=commandBuffer&unscoped_q=commandBuffer 
//public class ParticleEmitterSystem : JobComponentSystem
//{
//    private struct EmitterGroup
//    {
//        public int Length;
//        public ComponentDataArray<ParticleEmitter> emitters;
//        public ComponentDataArray<Position> positions;
//    }

//    //[BurstCompile]
//    //This cannot currently be burst compiled because CommandBuffer.SetComponent() accesses a static field.
//    private struct EmissionJob : IJob
//    {
//        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;
//        [ReadOnly] public EntityArchetype particleArchetype;
//        [ReadOnly] public Particle particleTemplate;
//        [ReadOnly] public ParticleEmitter emitter;
//        [ReadOnly] public Position position;
//        [ReadOnly] public float3 emissionDirection;
//        [ReadOnly] public float3 rotatorDirection;
//        [ReadOnly] public float rotatorSpeed;

//        public void Execute()
//        {
//            for (int i = 0; i < emitter.particlesPerEmission; i++)
//            {
//                commandBuffer.CreateEntity(particleArchetype);

//                var rotatorComponent = new RotatorComponent();
//                rotatorComponent.rotationSpeed = rotatorSpeed;
//                rotatorComponent.direction = rotatorDirection;

//                var newParticle = particleTemplate;
//                newParticle.velocity = (emitter.emissionDirection + emissionDirection) * emitter.initialSpeed;

//                commandBuffer.SetComponent(new Position() { Value = position.Value });
//                commandBuffer.SetComponent(new Rotation() { Value = new quaternion() });
//                commandBuffer.SetComponent(rotatorComponent);
//                commandBuffer.SetComponent(newParticle);
//                commandBuffer.AddSharedComponent(new NeedsMeshInstanceRendererTag());
//            }
//        }
//    }

//    [Inject] private EmitterGroup emitterGroup;
//    [Inject] private EndFrameBarrier endFrameBarrier;

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var dt = Time.deltaTime;
//        for (int i = 0; i < emitterGroup.Length; i++)
//        {
//            var particleEmitter = emitterGroup.emitters[i];
//            var emitterPosition = emitterGroup.positions[i];
//            particleEmitter.emissionTimer += dt;
//            if (particleEmitter.emissionTimer >= particleEmitter.emissionRate)
//            {
//                particleEmitter.emissionTimer = 0;
//                var randomEmissionDirection = new float3
//                (
//                    Random.Range(-particleEmitter.rangeX, particleEmitter.rangeX),
//                    Random.Range(-particleEmitter.rangeY, particleEmitter.rangeY),
//                    Random.Range(-particleEmitter.rangeZ, particleEmitter.rangeZ)
//                );
//                var emissionJob = new EmissionJob()
//                {
//                    commandBuffer = endFrameBarrier.CreateCommandBuffer(),
//                    particleArchetype = BootstrapperParticles.particleArchetype,
//                    particleTemplate = BootstrapperParticles.particleTemplate,
//                    emitter = particleEmitter,
//                    position = emitterPosition,
//                    emissionDirection = randomEmissionDirection,
//                    rotatorSpeed = Random.Range(1.0f, 5.0f),
//                    rotatorDirection = new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f))
//                };
//                var handle = emissionJob.Schedule(inputDeps);
//                handle.Complete();
//            }
//        }
//        return inputDeps;
//    }
//}


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