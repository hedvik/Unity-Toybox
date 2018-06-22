using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class ParticleLifetimeSystem : JobComponentSystem
{
    struct ParticleGroup
    {
        public int Length;
        public ComponentDataArray<Particle> particles;
        public EntityArray entities;
    }

    [BurstCompile]
    struct LifetimeJob : IJobParallelFor
    {
        public ComponentDataArray<Particle> particles;

        [ReadOnly] public float dt;
        [ReadOnly] public EntityArray entities;
        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(int i)
        {
            var particle = particles[i];
            particle.lifeTime -= dt;
            particles[i] = particle;

            if (particle.lifeTime < 0)
            {
                commandBuffer.DestroyEntity(entities[i]);
            }
        }
    }

    [Inject] ParticleGroup particleGroup;
    [Inject] EndFrameBarrier endFrameBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var lifeTimeJob = new LifetimeJob()
        {
            dt = Time.deltaTime,
            commandBuffer = endFrameBarrier.CreateCommandBuffer(),
            particles = particleGroup.particles,
            entities = particleGroup.entities
        };

        return lifeTimeJob.Schedule(particleGroup.Length, 1, inputDeps);
    }
}
