using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeDetect : MonoBehaviour
{
	public GameObject obj;
    public GameObject hint;
    public GameObject sphere;

    int shakeCount;

    SkinnedMeshRenderer skin;

    // How often to update the accelerometer
    float updateAcceleration = 1.0f / 60.0f;
	// Threshold for magnitude of shake vector
	float minShake;
	// Value of filter vector
	Vector3 lowPassValue;
	// Acceleration vectors
	Vector3 acceleration;
	Vector3 deltaAcceleration;

	void Start()
	{
		lowPassValue = Input.acceleration;
        // Recommended value according to certain manufacturers
        minShake = 2.0f;
        obj.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("King of the slimes"));
        hint.SetActive(false);
        sphere.SetActive(false);
        shakeCount = 0;
        skin = obj.GetComponentInChildren<SkinnedMeshRenderer>();
	}

    void Update()
	{
        if (skin.isVisible)
        {
            acceleration = Input.acceleration;
            lowPassValue = Vector3.Lerp(lowPassValue, acceleration, updateAcceleration);
            deltaAcceleration = acceleration - lowPassValue;

            // If shake magnitude threshold hit, reduce obj
            if (deltaAcceleration.sqrMagnitude >= minShake)
            {
                obj.transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);
                shakeCount++;

                if (shakeCount > 10)
                {
                    Destroy(obj);
                    hint.SetActive(true);
                    sphere.SetActive(true);
                }
            }

            if (ARHandler.GetHitIfAny().Equals(sphere.name))
            {
                Destroy(hint); 
                Destroy(sphere);
                ARHandler.GetAchievement("King of the slimes");
            }
        }
	}
}
