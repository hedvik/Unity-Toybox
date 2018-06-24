using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
public class ParticlePhysicsSystem : JobComponentSystem
{
    struct ParticleGroup
    {
        public int Length;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Particle> particles;
        public SubtractiveComponent<DisabledComponentTag> disabledTags;
    }

    [BurstCompile]
    struct PhysicsJob : IJobProcessComponentData<Position, Particle>
    {
        [ReadOnly] public float fdt;

        public void Execute(ref Position position, ref Particle particle)
        {
            particle.force += particle.gravity;
            particle.acceleration = particle.force * particle.inverseMass;
            particle.velocity += particle.acceleration * fdt;
            position.Value += particle.velocity * fdt;
            particle.force = new float3(0, 0, 0);

            // HACK: Budget collision and velocity dissipation on collision ;)
            if(position.Value.y < 20)
            {
                particle.velocity.y = -particle.velocity.y * 0.8f;
            }
        }
    }

    [BurstCompile]
    struct PhysicsJobForLoop : IJobParallelFor
    {
        [ReadOnly] public float fdt;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Particle> particles;

        public void Execute(int i)
        {
            var particle = particles[i];
            var position = positions[i];

            particle.force += particle.gravity;
            particle.acceleration = particle.force * particle.inverseMass;
            particle.velocity += particle.acceleration * fdt;
            position.Value += particle.velocity * fdt;
            particle.force = new float3(0, 0, 0);

            // HACK: Budget collision and velocity dissipation on collision ;)
            if (position.Value.y < 20)
            {
                particle.velocity.y = -particle.velocity.y * 0.8f;
            }
            particles[i] = particle;
            positions[i] = position;
        }
    }

    [Inject] private ParticleGroup filteredParticles;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //var physicsJob = new PhysicsJobNonSubtractive() { fdt = Time.fixedDeltaTime };
        var physicsJob = new PhysicsJobForLoop()
        {
            fdt = Time.fixedDeltaTime,
            particles = filteredParticles.particles,
            positions = filteredParticles.positions
        };
        var handle = physicsJob.Schedule(filteredParticles.Length, 1, inputDeps);
        return handle;
    }
}
