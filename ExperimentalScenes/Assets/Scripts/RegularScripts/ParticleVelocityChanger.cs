using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleVelocityChanger : MonoBehaviour {
    public float velocityStrength = 1;

    private ParticleSystem particleSys;
    private List<ParticleSystem.Particle> enteredParticles = new List<ParticleSystem.Particle>();

    private void Start()
    {
        particleSys = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        
    }

    private void OnParticleTrigger()
    {
        int numEnter = particleSys.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enteredParticles);
        for(int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle particle = enteredParticles[i];
            particle.velocity = new Vector3(Random.Range(5, 10), Random.Range(-10, 10), 0).normalized * velocityStrength;
            enteredParticles[i] = particle;
        }
        particleSys.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enteredParticles);
    }
}
