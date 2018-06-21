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
// Based on https://forum.unity.com/threads/to-make-it-clear-do-i-have-a-start-function-on-ecs.523943/
// Only runs once on every entity that has the InitializePositionTag component
public class PixelCubePlacer : JobComponentSystem
{
    // Component Group
    public struct SystemData
    {
        [ReadOnly]
        public SharedComponentDataArray<ImageColours> imageColours;

        public int Length;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<InitializePositionTag> initTags;
        public EntityArray entities;
    }
    
    [BurstCompile]
    struct PixelCubePlacerJob : IJobParallelFor
    {
        [ReadOnly]public int width;
        [ReadOnly]public int height;
        [ReadOnly]public EntityArray entities;
        
        // A regular commandBuffer will not work in this case so we use a concurrent one instead
        [WriteOnly]public EntityCommandBuffer.Concurrent commandBuffer;
        [WriteOnly]public ComponentDataArray<Position> positions;

        public void Execute(int i)
        {
            positions[i] = new Position
            {
                Value = new float3(i % width, i / height, 0)
            };
            commandBuffer.RemoveComponent<InitializePositionTag>(entities[i]);
        }
    }

    [Inject]
    private SystemData pixelCubes;

    [Inject]
    private EndFrameBarrier endFrameBarrier;
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // TODO: Would be to have individual colours per cube to correspond pixel colours, but MeshInstanceRenderer seems to be a bit limited in regards to per instance data so far.
        var positionSetterJob = new PixelCubePlacerJob
        {
            positions = pixelCubes.positions,
            entities = pixelCubes.entities,
            width = pixelCubes.imageColours[0].width,
            height = pixelCubes.imageColours[0].height,
            commandBuffer = endFrameBarrier.CreateCommandBuffer()
        };
        return positionSetterJob.Schedule(pixelCubes.Length, 64, inputDeps);
    }
}
