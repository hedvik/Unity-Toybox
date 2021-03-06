﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
public class ParticleAttractorSystem : JobComponentSystem
{
    private struct AttractorGroup
    {
        public readonly int Length;
        public ComponentDataArray<ParticleAttractor> particleAttractors;
        public ComponentDataArray<Position> particleAttractorPositions;
    }

    [BurstCompile]
    private struct AttractorJob : IJobProcessComponentData<Position, Particle>
    {
        [ReadOnly] public ComponentDataArray<ParticleAttractor> particleAttractors;
        [ReadOnly] public ComponentDataArray<Position> particleAttractorPositions;

        public void Execute([ReadOnly] ref Position position, [WriteOnly] ref Particle particle)
        {
            for (int i = 0; i < particleAttractors.Length; i++)
            {
                // Checking whether the particle position is within the attractor sphere radius
                var calc = (math.pow((position.Value.x - particleAttractorPositions[i].Value.x), 2) +
                            math.pow((position.Value.y - particleAttractorPositions[i].Value.y), 2) +
                            math.pow((position.Value.z - particleAttractorPositions[i].Value.z), 2));
                var powRadius = math.pow(particleAttractors[i].radius, 2);
                var attractionVector = math.normalize(particleAttractorPositions[i].Value - position.Value) * particleAttractors[i].attractorStrength;

                particle.force += math.select(new float3(0, 0, 0), attractionVector, calc < powRadius ? true : false);
            }
        }
    }

    [Inject] private AttractorGroup attractors;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var attractorJob = new AttractorJob()
        {
            particleAttractors = attractors.particleAttractors,
            particleAttractorPositions = attractors.particleAttractorPositions
        };
        var handle = attractorJob.Schedule(this, inputDeps);
        return handle;
    }
}