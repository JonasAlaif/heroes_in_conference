using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood2 : MonoBehaviour
{
	public GameObject resource;
	public Swipe swipe;

	int swipeCount;
    MeshRenderer meshRenderer;

    void Start()
	{
		swipeCount = 0;
        resource.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Timber!!!"));
        meshRenderer = resource.GetComponent<MeshRenderer>();
    }

	void Update()
	{
        if (meshRenderer.isVisible)
        {
            if (swipe.GetLeft() || swipe.GetRight())
		    {
			    swipeCount++;
	    	}
		    if (swipeCount == 2)
    		{
                Destroy(resource);
                ARHandler.GetAchievement("Timber!!!");
    		}
	    }
    }
}
