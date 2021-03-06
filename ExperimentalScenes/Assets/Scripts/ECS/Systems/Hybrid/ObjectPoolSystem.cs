﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
public class ObjectPoolSystem : ComponentSystem
{
    private struct SystemComponents
    {
        public ObjectPoolComponent objectPool;
    }

    private float spawnTimer = 0;

    // Spawns the prefabs for each pool and sets then to inactive. 
    protected override void OnStartRunning()
    {
        foreach (var entity in GetEntities<SystemComponents>())
        {
            entity.objectPool.poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach(var pool in entity.objectPool.pools)
            {
                var objectPoolQueue = new Queue<GameObject>();
                GameObject obj;

                for(int i = 0; i < pool.size; i++)
                {
                    if (pool.parent != null)
                    {
                        obj = GameObject.Instantiate(pool.prefab, pool.parent);
                    }
                    else
                    {
                        obj = GameObject.Instantiate(pool.prefab);
                    }

                    obj.SetActive(false);
                    objectPoolQueue.Enqueue(obj);
                }

                entity.objectPool.poolDictionary.Add(pool.tag, objectPoolQueue);
            }
        }
    }

    protected override void OnUpdate()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= 0.02f)
        {
            SpawnFromPool("cube", new Vector3(-20, 5, 0), Quaternion.identity);
            spawnTimer -= 0.02f;
        }
    }

    private GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        foreach (var entity in GetEntities<SystemComponents>())
        {
            if (!entity.objectPool.poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Invalid Tag: " + tag + " received!");
                return null;
            }

            // Still kinda working on gameObjects rather than entities here. Not sure how to mainly do entity work yet. 
            var objectToSpawn = entity.objectPool.poolDictionary[tag].Dequeue();

            // Probably want to do some null checking
            objectToSpawn.GetComponent<ObjectStateComponent>().respawned = true;
            objectToSpawn.gameObject.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            entity.objectPool.poolDictionary[tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }

        return null;
    }
}
