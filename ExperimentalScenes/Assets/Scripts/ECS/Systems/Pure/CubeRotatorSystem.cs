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

    // Parallel ForLoop example
    [BurstCompile]
    struct CubeRotatorForLoopJob : IJobParallelFor
    {
        public float dt;
        public ComponentDataArray<Rotation> rotations;

        [ReadOnly]
        public ComponentDataArray<CubeRotatorComponent> cubeFloaterComponent;

        public void Execute(int i)
        {
            rotations[i] = new Rotation
            {
                Value = math.mul(math.normalize(rotations[i].Value), math.axisAngle(cubeFloaterComponent[i].direction, cubeFloaterComponent[i].rotationSpeed * dt))
            };
        }
    }

    // More or less same job as above, but on specific components which could be seen as a bit cleaner in this case.
    // This approach does not require data injection so we do not need the cubes struct here. 
    [BurstCompile]
    struct CubeRotatorComponentJob : IJobProcessComponentData<Rotation, CubeRotatorComponent>
    {
        public float dt;

        public void Execute(ref Rotation rotation, [ReadOnly]ref CubeRotatorComponent cubeFloaterComponent)
        {
            rotation.Value = math.mul(math.normalize(rotation.Value), math.axisAngle(cubeFloaterComponent.direction, cubeFloaterComponent.rotationSpeed * dt));
        }
    }

    // Job Scheduling is handled in OnUpdate here
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // For Loop example
        // We pass on the data from our entities to the job
        //var rotatorJob = new CubeRotatorForLoopJob
        //{
        //    rotations = cubes.rotations,
        //    cubeFloaterComponent = cubes.cubeFloaterComponent,
        //    dt = Time.deltaTime
        //};
        //return rotatorJob.Schedule(cubes.Length, 64, inputDeps);

        // IJobProcessComponentData example
        var rotatorJob = new CubeRotatorComponentJob() { dt = Time.deltaTime };
        return rotatorJob.Schedule(this, 64, inputDeps);
    }
}
