using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore3 : MonoBehaviour
{
    public GameObject resource;
    public GameObject obj;
    public GameObject resourceCollected;

    int hitCounter;

	void Start()
	{
        hitCounter = 0;
        resource.SetActive(ARHandler.active["resource"]);
        obj.SetActive(ARHandler.active["resource"]);
        resourceCollected.SetActive(false);
	}

	void Update()
	{
        if (ARHandler.GetHitIfAny().Equals(resource.name))
        {
            hitCounter++;
        }

		if (hitCounter == 3)
		{
            Destroy(resource);
            resourceCollected.SetActive(true);
            Destroy(obj);
            ARHandler.GetAchievement("Dopey");
		}
	}
}
