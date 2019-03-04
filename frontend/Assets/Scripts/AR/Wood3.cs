using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood3 : MonoBehaviour
{
	public GameObject resource;
	public Swipe swipe;

	int swipeCount;
    MeshRenderer meshRenderer;

    void Start()
	{
		swipeCount = 0;
        resource.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("It's treeson!"));
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
                ARHandler.GetAchievement("It's treeson!");
    		}
	    }
    }
}
