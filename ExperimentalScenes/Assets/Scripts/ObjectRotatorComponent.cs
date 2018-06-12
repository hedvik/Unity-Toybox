using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(GameObjectEntity))]
public class ObjectRotatorComponent : MonoBehaviour {
    public float speed;
    public Vector3 axis;

    [HideInInspector] public float angle = 0;
}

public class ObjectRotatorSystem : ComponentSystem
{
    // The components we want to work with in the system
    private struct SystemComponents
    {
        public ObjectRotatorComponent rotator;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach(var entity in GetEntities<SystemComponents>())
        {
            entity.rotator.angle += Time.deltaTime * entity.rotator.speed;
            entity.transform.rotation = Quaternion.AngleAxis(entity.rotator.angle, entity.rotator.axis);
        }
    }
}
