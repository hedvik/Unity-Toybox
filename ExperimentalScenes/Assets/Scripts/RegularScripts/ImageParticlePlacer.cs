using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageParticlePlacer : MonoBehaviour {
    public float stepSize = 1;
    public Vector2 startPos = new Vector2(0, 0);
    public float particleSize = 0.1f;

    private ParticleSystem particleSys;

    private void Awake()
    {
        particleSys = GetComponent<ParticleSystem>();
        var sys = particleSys.main;
        sys.startSize = new ParticleSystem.MinMaxCurve(particleSize);
    }

    public void PlaceParticles(Color[] colors, int width, int height)
    {
        particleSys.Play();

        var x = 0;
        var y = 0;
        for (var i = 0; i < colors.Length; i++)
        {            
            if (x == width)
            {
                y++;
                x = 0;
            }

            if (colors[i].a > 0)
            {
                var emitParams = new ParticleSystem.EmitParams();
                emitParams.position = new Vector3(startPos.x + (x * stepSize), startPos.y + (y * stepSize), 0);
                emitParams.velocity = new Vector3(0, 0, 0);
                emitParams.startColor = colors[i];
                particleSys.Emit(emitParams, 1);
            }

            x++;
        }
    }
}
