using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(GameObjectEntity))]
public class ObjectPoolComponent : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public Transform parent;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
}
