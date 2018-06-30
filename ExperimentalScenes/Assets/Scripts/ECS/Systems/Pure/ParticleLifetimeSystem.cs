using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
public class ParticleLifetimeSystem : JobComponentSystem
{
    struct ParticleGroup
    {
        public int Length;
        public ComponentDataArray<Particle> particles;
        public ComponentDataArray<Position> positions;
        public SubtractiveComponent<DisabledComponentTag> disabledTags;
        public EntityArray entities;
    }

    //[BurstCompile]
    struct LifetimeJob : IJobParallelFor
    {
        public ComponentDataArray<Particle> particles;

        [ReadOnly] public float dt;
        [ReadOnly] public EntityArray entities;
        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;
        [WriteOnly] public ComponentDataArray<Position> positions;

        public void Execute(int i)
        {
            var particle = particles[i];
            particle.lifeTimer -= dt;
            particles[i] = particle;

            if (particle.lifeTimer < 0)
            {
                commandBuffer.AddComponent(entities[i], new DisabledComponentTag());
                positions[i] = new Position(){ Value = new float3(0, 10000, 0) };
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
            entities = particleGroup.entities,
            positions = particleGroup.positions
        };

        var handle = lifeTimeJob.Schedule(particleGroup.Length, 1, inputDeps);
        return handle;
    }
}
