using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
public class ObjectJumperSystem : ComponentSystem
{
    private struct JumperComponents
    {
        public ObjectJumperComponent objectJumper;
        public ObjectStateComponent objectState;
        public Rigidbody rigidbody;
    }

    protected override void OnUpdate()
    {
        foreach(var entity in GetEntities<JumperComponents>())
        {       
            if(entity.objectState.respawned)
            {
                entity.rigidbody.velocity = Vector3.zero;
                entity.rigidbody.AddForce((entity.objectJumper.forceDirection * entity.objectJumper.jumpStrength), ForceMode.Impulse);
                entity.objectState.respawned = false;
            }
        }
    }
}