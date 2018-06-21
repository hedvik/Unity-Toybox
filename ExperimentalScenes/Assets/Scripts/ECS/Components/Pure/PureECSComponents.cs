using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

public struct InitializePositionTag : IComponentData
{

}

public struct ImageColours : ISharedComponentData
{
    public Color[] colors;
    public int width;
    public int height;
}
