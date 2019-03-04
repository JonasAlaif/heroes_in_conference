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
        hint.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("What a steal!"));
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
            // Rotate 'obj' to face the camera (only in the y axis)
            obj.transform.forward = (Vector3.ProjectOnPlane(-Camera.main.transform.forward, new Vector3(0, 1, 0)));
        }
	}
}
