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

// There are a lot of references to templates going on here so working with a standard ComponentSystem is a bit easier since Jobs cannot take references like the mesh.
// This is regardless the primary part of the particle system that could need optimisations.
public class ParticleEmitterSystem : ComponentSystem
{
    private struct EmitterGroup
    {
        public ComponentDataArray<ParticleEmitter> emitters;
        [ReadOnly] public ComponentDataArray<Position> positions;
        [ReadOnly] public int Length;
    }

    [Inject] EmitterGroup emitterGroup;

    protected override void OnUpdate()
    {
        var dt = Time.deltaTime;
        for (int i = 0; i < emitterGroup.Length; i++)
        {
            var emitter = emitterGroup.emitters[i];
            emitter.emissionTimer += dt;
            if (emitter.emissionTimer >= emitter.emissionRate)
            {
                emitter.emissionTimer = 0;
                for (int j = 0; j < emitterGroup.emitters[i].particlesPerEmission; j++)
                {
                    // Generating emission vector
                    var direction = new float3(
                        Random.Range(-emitterGroup.emitters[i].rangeX, emitterGroup.emitters[i].rangeX),
                        Random.Range(-emitterGroup.emitters[i].rangeY, emitterGroup.emitters[i].rangeY),
                        Random.Range(-emitterGroup.emitters[i].rangeZ, emitterGroup.emitters[i].rangeZ)
                        );

                    EmitParticle(emitterGroup.positions[i].Value, (emitterGroup.emitters[i].emissionDirection + direction) * emitterGroup.emitters[i].initialSpeed);
                }
            }
            emitterGroup.emitters[i] = emitter;
        }
    }

    private void EmitParticle(float3 position, float3 velocity)
    {
        PostUpdateCommands.CreateEntity(BootstrapperParticles.particleArchetype);

        var rotatorComponent = new RotatorComponent();
        rotatorComponent.rotationSpeed = Random.Range(1.0f, 5.0f);
        rotatorComponent.direction = new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

        var newParticle = BootstrapperParticles.particleTemplate;
        newParticle.velocity = velocity;

        PostUpdateCommands.SetComponent(new Position() { Value = position });
        PostUpdateCommands.SetComponent(new Rotation() { Value = new quaternion() });
        PostUpdateCommands.SetComponent(rotatorComponent);
        PostUpdateCommands.SetComponent(newParticle);
        PostUpdateCommands.AddSharedComponent(BootstrapperParticles.particleLook);
    }
}