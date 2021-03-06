﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct FloaterComponent : IComponentData
{
    public float3 floatDirection;
    public float floatSpeed;
}

public struct RotatorComponent : IComponentData
{
    public float3 direction;
    public float rotationSpeed;
}

public struct InitializePositionTag : IComponentData { }

public struct ImageColours : ISharedComponentData
{
    public Color[] colors;
    public int width;
    public int height;
}

[Serializable]
public struct Particle : IComponentData
{
    public float mass;
    [HideInInspector] public float inverseMass;
    [HideInInspector] public float3 force;
    [HideInInspector] public float3 acceleration;
    [HideInInspector] public float3 velocity;

    public float lifeTime;
    public float3 gravity;
}

[Serializable]
public struct ParticleEmitter : IComponentData
{
    public float emissionRate;
    [HideInInspector] public float emissionTimer;

    // Pyramid emission 
    public uint particlesPerEmission;
    public float3 emitterPosition;
    public float3 emissionDirection;
    public float rangeX;
    public float rangeY;
    public float rangeZ;
    public float initialSpeed;

    // TODO: Unused for now. The problem is: how do we handle particle pooling with ECS?
    public int maxParticles;
}

public struct NeedsMeshInstanceRendererTag : ISharedComponentData { };

[Serializable]
public struct ParticleAttractor : IComponentData
{
    public float attractorStrength;
    public float radius;
}