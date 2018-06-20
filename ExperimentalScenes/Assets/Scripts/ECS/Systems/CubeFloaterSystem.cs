using System.Collections;
using System.Collections.Generic;
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
    }

    // Injects all entities with the specified ComponentDataArray's we specified in SystemData
    [Inject]
    private SystemData cubes;

    protected override void OnUpdate()
    {
        var dt = Time.deltaTime;
        for (int i = 0; i < cubes.Length; i++)
        {
            var position = cubes.position[i];
            position.Value += new float3(Random.Range(-1.0f, 1.0f), 1, Random.Range(-1.0f, 1.0f)) * dt;
            cubes.position[i] = position;
        }
    }
}
