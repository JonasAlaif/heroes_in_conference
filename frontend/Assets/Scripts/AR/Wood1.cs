using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood1 : MonoBehaviour
{
	public GameObject resource;
	public Swipe swipe;

	int swipeCount;
    MeshRenderer meshRenderer;

	void Start()
	{
		swipeCount = 0;
        resource.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Run Forest, run!"));
        meshRenderer = resource.GetComponent<MeshRenderer>();

    }

	void Update()
	{
        if (meshRenderer.isVisible)
        {
            if (swipe.GetRight() || swipe.GetLeft())
		    {
			    swipeCount++;
		    }
		    if (swipeCount == 2)
		    {
                Destroy(resource);
                ARHandler.GetAchievement("Run Forest, run!");
		    }
        }
	}
}
