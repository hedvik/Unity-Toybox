using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Standard ComponentSystem example with Pure ECS
//public class CubeFloaterSystem : ComponentSystem
//{
//    public struct SystemData
//    {
//        public int Length;
//        public ComponentDataArray<Position> position;

//        [ReadOnly]
//        public ComponentDataArray<CubeFloaterComponent> cubeFloaterComponent;
//    }

//    // Injects all entities with the specified ComponentDataArray's we specified in SystemData
//    [Inject]
//    private SystemData cubes;

//    protected override void OnUpdate()
//    {
//        var dt = Time.deltaTime;
//        for (int i = 0; i < cubes.Length; i++)
//        {
//            // Read
//            var position = cubes.position[i];

//            // Modify
//            position.Value += cubes.cubeFloaterComponent[i].floatDirection * cubes.cubeFloaterComponent[i].floatSpeed * dt;

//            // Write
//            cubes.position[i] = position;
//        }
//    }
//}

// Same as above, but using the Job System instead
// This change alone made the application move from ~200fps to ~650fps at 10k entities
public class EntityFloaterSystem : JobComponentSystem
{
    [BurstCompile]
    struct EntityFloaterJob : IJobProcessComponentData<Position, FloaterComponent>
    {
        [ReadOnly] public float dt;

        public void Execute([WriteOnly]ref Position position, [ReadOnly]ref FloaterComponent floater)
        {
            position.Value += floater.floatDirection * floater.floatSpeed * dt;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var floaterJob = new EntityFloaterJob() { dt = Time.deltaTime };
        return floaterJob.Schedule(this, inputDeps);
    }
}