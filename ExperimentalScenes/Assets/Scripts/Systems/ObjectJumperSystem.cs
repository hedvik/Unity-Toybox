using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
public class ObjectJumperSystem : ComponentSystem
{
    private struct SystemComponents
    {
        public ObjectJumperComponent objectJumper;
        public Rigidbody rigidbody;
    }

    protected override void OnUpdate()
    {
        foreach (var entity in GetEntities<SystemComponents>())
        {
            entity.rigidbody.AddForce(entity.objectJumper.forceDirection * entity.objectJumper.jumpStrength, ForceMode.Impulse);
        }
    }
}