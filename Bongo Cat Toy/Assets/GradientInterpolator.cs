using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradientInterpolator : MonoBehaviour {
    public Sprite gradientSprite;
    public float speed = 5;
    public int sampleOffsetY = 50;

    private Color color;
    private float lerpTimer = 0;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        lerpTimer += Time.deltaTime * speed;
        if(lerpTimer >= 1)
        {
            lerpTimer -= 1;
        }

        image.color = gradientSprite.texture.GetPixel((int)Mathf.Lerp(0, gradientSprite.texture.width, lerpTimer), sampleOffsetY);
    }
}
