using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct CubeFloaterComponent : IComponentData
{
    public float3 floatDirection;
    public float floatSpeed;
}

public struct CubeRotatorComponent : IComponentData
{
    public float3 direction;
    public float rotationSpeed;
}