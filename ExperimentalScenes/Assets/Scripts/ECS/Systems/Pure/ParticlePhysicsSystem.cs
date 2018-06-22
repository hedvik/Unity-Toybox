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
    [BurstCompile]
    struct PhysicsJob : IJobProcessComponentData<Position, Particle>
    {
        [ReadOnly] public float fdt;

        public void Execute([WriteOnly]ref Position position, ref Particle particle)
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

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var physicsJob = new PhysicsJob() { fdt = Time.fixedDeltaTime };
        return physicsJob.Schedule(this, 1, inputDeps);
    }
}
