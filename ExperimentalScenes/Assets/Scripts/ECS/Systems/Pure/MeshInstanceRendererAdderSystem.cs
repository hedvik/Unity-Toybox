using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

// This system allows us to jobify most of the particle emission outside of adding the MeshInstanceRenderer as it cannot be put into jobs at the moment.
public class MeshInstanceRendererAdderSystem : ComponentSystem
{
    private struct TagGroup
    {
        [WriteOnly] public EntityArray entities;
        [ReadOnly] public int Length;
        [ReadOnly] public ComponentDataArray<NeedsMeshInstanceRendererTag> taggedEntities;
    }

    [Inject] private TagGroup tagGroup;

    protected override void OnUpdate()
    {
        for(int i = 0; i < tagGroup.Length; i++)
        {
            PostUpdateCommands.AddSharedComponent(tagGroup.entities[i], BootstrapperParticles.particleLook);
            PostUpdateCommands.RemoveComponent<NeedsMeshInstanceRendererTag>(tagGroup.entities[i]);
        }
    }
}
