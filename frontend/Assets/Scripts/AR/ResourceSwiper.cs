using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSwiper : MonoBehaviour
{
	public GameObject resource;
	public Swipe swipe;

	int swipeCount;
    MeshRenderer meshRenderer;

	void Start()
	{
		swipeCount = 0;
        meshRenderer = resource.GetComponent<MeshRenderer>();

    }

	void Update()
	{
        if ((swipe.GetLeft() || swipe.GetRight()) && meshRenderer.isVisible)
		{
			swipeCount++;
		}
		if (swipeCount == 2)
		{
            Destroy(resource); 
		}
	}
}
