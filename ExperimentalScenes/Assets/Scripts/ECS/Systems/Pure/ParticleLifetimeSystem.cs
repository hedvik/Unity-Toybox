using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.PostLateUpdate))]
public class ParticleLifetimeSystem : JobComponentSystem
{
    struct ParticleGroup
    {
        public int Length;
        public ComponentDataArray<Particle> particles;
        public SubtractiveComponent<DisabledComponentTag> disabledTags;
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
            particle.lifeTimer -= dt;
            particles[i] = particle;

            if (particle.lifeTimer < 0)
            {
                commandBuffer.AddComponent(entities[i], new DisabledComponentTag());
                commandBuffer.RemoveComponent<MeshInstanceRenderer>(entities[i]);
            }
        }
    }

    [Inject] ParticleGroup particleGroup;
    [Inject] EndFrameBarrier barrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var lifeTimeJob = new LifetimeJob()
        {
            dt = Time.deltaTime,
            commandBuffer = barrier.CreateCommandBuffer(),
            particles = particleGroup.particles,
            entities = particleGroup.entities
        };

        var handle = lifeTimeJob.Schedule(particleGroup.Length, 1, inputDeps);
        return handle;
    }
}
