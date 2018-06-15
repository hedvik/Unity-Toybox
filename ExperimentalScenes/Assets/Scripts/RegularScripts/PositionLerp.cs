using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionLerp : MonoBehaviour {
    public Vector3 destination;
    public float speed;

    private Vector3 originalPosition = new Vector3();
    private float lerpTimer = 0;

	// Use this for initialization
	void Start () {
        originalPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (lerpTimer > 1)
            return;

        lerpTimer += Time.deltaTime * speed;
        transform.position = Vector3.Lerp(originalPosition, destination, lerpTimer);
	}
}
