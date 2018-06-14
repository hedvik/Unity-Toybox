using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageDisintegrator : MonoBehaviour {
    public Texture2D image;
    public ImageParticlePlacer particlePlacer;

    private void Start()
    {
        var colorData = image.GetPixels();
        particlePlacer.PlaceParticles(colorData, image.width, image.height);
    }
}
