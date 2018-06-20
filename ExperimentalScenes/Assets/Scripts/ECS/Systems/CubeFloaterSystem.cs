using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CubeFloaterSystem : ComponentSystem
{
    public struct SystemData
    {
        public int Length;
        public ComponentDataArray<Position> position;

        [ReadOnly]
        public ComponentDataArray<CubeFloaterComponent> cubeFloaterComponent;
    }

    // Injects all entities with the specified ComponentDataArray's we specified in SystemData
    [Inject]
    private SystemData cubes;

    protected override void OnUpdate()
    {
        var dt = Time.deltaTime;
        for (int i = 0; i < cubes.Length; i++)
        {
            // Read
            var position = cubes.position[i];

            // Modify
            position.Value += cubes.cubeFloaterComponent[i].floatDirection * cubes.cubeFloaterComponent[i].floatSpeed * dt;

            // Write
            cubes.position[i] = position;
        }
    }
}
