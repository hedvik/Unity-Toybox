﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
public class ObjectRotatorSystem : ComponentSystem
{
    // The components we want to work with in the system
    private struct SystemComponents
    {
        public ObjectRotatorComponent rotator;

        [WriteOnly]
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var entity in GetEntities<SystemComponents>())
        {
            entity.rotator.angle += Time.deltaTime * entity.rotator.speed;
            entity.transform.rotation = Quaternion.AngleAxis(entity.rotator.angle, entity.rotator.axis);
        }
    }
}