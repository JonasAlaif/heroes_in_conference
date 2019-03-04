using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintFind : MonoBehaviour
{
	public GameObject obj;
    public GameObject hint;
    public GameObject collide;

	void Start()
	{
        obj.SetActive(ARHandler.active["challenge"]);
	}

	void Update()
    { 
        if (ARHandler.GetHitIfAny().Equals(hint.name))
        {
            Destroy(hint);
            ARHandler.GetAchievement("What a steal!");
        }
        else if (ARHandler.GetHitIfAny().Equals(collide.name))
        {
            obj.transform.forward = (Vector3.ProjectOnPlane(-Camera.main.transform.forward, new Vector3(0, 1, 0)));
        }
	}
}
