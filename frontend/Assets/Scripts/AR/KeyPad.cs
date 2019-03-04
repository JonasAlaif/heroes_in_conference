using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPad : MonoBehaviour
{
    // Main asset
    public GameObject obj;
    // Keypad collider objects
	public GameObject[] numpad;
    // Keypad visual markers
    public GameObject[] shells;

    public GameObject hint;
    public GameObject hintsphere;

    // Feedback requirements
    public GameObject[] feedbackMarker;
    public Material[] feedbackMaterial;

	int[] inputCode;
	int pushCount;

    // Initial setup of imagetarget: activating assets, creating code input array
	void Start()
	{
		inputCode = new int[4];
        hint.SetActive(false);
        hintsphere.SetActive(false);
        for (int i = 0; i < numpad.Length; i++)
        {
            numpad[i].SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Crab rave"));
            shells[i].SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Crab rave"));
        }

        for (int i = 0; i < feedbackMarker.Length; i++)
        {
            feedbackMarker[i].SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Crab rave"));
        }


        pushCount = 0;
	}

	void Update()
	{
        // If 4 values have been entered, reset the counter, check if correct, act accordingly
        if (pushCount == 4)
		{
			pushCount = 0;
			// Change values for custom code
			if (inputCode[0] == 1 && inputCode[1] == 2 && inputCode[2] == 3 && inputCode[3] == 4)
			{
				for (int i = 0; i < numpad.Length; i++)
                {
                    Destroy(numpad[i]);
                    Destroy(shells[i]);
                    Destroy(feedbackMarker[i]);
                }
                hint.SetActive(true);
                hintsphere.SetActive(true);
            }

			else
            {
                // Reset feedbackMarker materials
                for (int i = 0; i < feedbackMarker.Length; i++)
                {
                    SimpleChangeMaterial(feedbackMarker[i], feedbackMaterial[0]);
                }
			}
		}

		// Find which button was pressed
		switch (ARHandler.GetHitIfAny())
		{
			case "key1" :
				inputCode[pushCount] = 1;
                SimpleChangeMaterial(feedbackMarker[pushCount], feedbackMaterial[1]);
                pushCount++;
				break;
			case "key2" :
                inputCode[pushCount] = 2;
                SimpleChangeMaterial(feedbackMarker[pushCount], feedbackMaterial[1]);
                pushCount++;
                break;
			case "key3" :
				inputCode[pushCount] = 3;
                SimpleChangeMaterial(feedbackMarker[pushCount], feedbackMaterial[1]);
                pushCount++;
                break;
			case "key4" :
				inputCode[pushCount] = 4;
                SimpleChangeMaterial(feedbackMarker[pushCount], feedbackMaterial[1]);
                pushCount++;
                break;
            case "hintsphere":
                Destroy(hint);
                Destroy(hintsphere);
                ARHandler.GetAchievement("Crab rave");
                break;
            default:
                break;
		} 
	}

    void SimpleChangeMaterial(GameObject o, Material mat)
    {
        o.GetComponent<Renderer>().material = mat;
    }
}
