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
public class EntityRotatorSystem : JobComponentSystem
{
    // Component Group
    public struct SystemData
    {
        public int Length;
        public ComponentDataArray<Rotation> rotations;
        public SubtractiveComponent<DisabledComponentTag> disabledTags;

        [ReadOnly] public ComponentDataArray<RotatorComponent> rotatorComponent;
    }

    // Parallel ForLoop example
    [BurstCompile]
    struct RotatorForLoopJob : IJobParallelFor
    {
        [ReadOnly] public float dt;
        public ComponentDataArray<Rotation> rotations;

        [ReadOnly]
        public ComponentDataArray<RotatorComponent> rotatorComponent;

        public void Execute(int i)
        {
            rotations[i] = new Rotation
            {
                Value = math.mul(math.normalize(rotations[i].Value), math.axisAngle(rotatorComponent[i].direction, rotatorComponent[i].rotationSpeed * dt))
            };
        }
    }

    // More or less same job as above, but with specified components which could be seen as a bit cleaner in this case.
    // This approach does not require data injection so we do not need cubes[] here. 
    [BurstCompile]
    struct RotatorJob : IJobProcessComponentData<Rotation, RotatorComponent>
    {
        [ReadOnly] public float dt;

        public void Execute(ref Rotation rotation, [ReadOnly]ref RotatorComponent rotatorComponent)
        {
            rotation.Value = math.mul(math.normalize(rotation.Value), math.axisAngle(rotatorComponent.direction, rotatorComponent.rotationSpeed * dt));
        }
    }

    // Injects all entities with the specified ComponentDataArray's we specified in SystemData
    [Inject]
    private SystemData filteredEntities;

    // Job Scheduling is handled in OnUpdate here
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // For Loop example
        // We pass on the data from our entities to the job
        var rotatorJob = new RotatorForLoopJob
        {
            rotations = filteredEntities.rotations,
            rotatorComponent = filteredEntities.rotatorComponent,
            dt = Time.deltaTime
        };
        return rotatorJob.Schedule(filteredEntities.Length, 64, inputDeps);

        // IJobProcessComponentData example
        //var rotatorJob = new RotatorJob() { dt = Time.deltaTime };
        //return rotatorJob.Schedule(this, 64, inputDeps);
    }
}
