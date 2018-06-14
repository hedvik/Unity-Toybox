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
