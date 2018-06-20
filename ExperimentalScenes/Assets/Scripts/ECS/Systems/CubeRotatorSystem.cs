using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Example of using the Burst Compiler and Job System with ECS
public class CubeRotatorSystem : JobComponentSystem
{
    // Component Group
    public struct SystemData
    {
        public int Length;
        public ComponentDataArray<Rotation> rotations;

        [ReadOnly]
        public ComponentDataArray<CubeRotatorComponent> cubeFloaterComponent;
    }

    // Injects all entities with the specified ComponentDataArray's we specified in SystemData
    [Inject]
    private SystemData cubes;

    // The struct containing the parallel job
    [BurstCompile]
    struct CubeRotatorLocalRotation : IJobParallelFor
    {
        public float dt;
        public ComponentDataArray<Rotation> rotations;

        [ReadOnly]
        public ComponentDataArray<CubeRotatorComponent> cubeFloaterComponent;

        public void Execute(int i)
        {
            var speed = cubeFloaterComponent[i].rotationSpeed;
            if (speed > 0.0f)
            {
                rotations[i] = new Rotation
                {
                    Value = math.mul(math.normalize(rotations[i].Value), math.axisAngle(cubeFloaterComponent[i].direction, speed * dt))
                };
            }
        }
    }

    // Job Scheduling is handled in OnUpdate here
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var rotationSpeedLocalRotationJob = new CubeRotatorLocalRotation
        {
            rotations = cubes.rotations,
            cubeFloaterComponent = cubes.cubeFloaterComponent,
            dt = Time.deltaTime
        };
        return rotationSpeedLocalRotationJob.Schedule(cubes.Length, 64, inputDeps);
    }
}
