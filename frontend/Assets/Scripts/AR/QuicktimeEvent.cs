using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuicktimeEvent : MonoBehaviour
{
    public GameObject obj;
    public GameObject[] fish;

    public GameObject[] hitSphere;

    public GameObject[] feedbackMarker;

    float probability;
    int reactionFrames;

    bool f1;

    void Start()
    {
        for (int i = 0; i < fish.Length; ++i)
        {
            fish[i].SetActive(false);
            hitSphere[i].SetActive(false);

            feedbackMarker[i].SetActive(false);
        }

        // Change this value to make sphere appear more often
        probability = 0.3f;
        reactionFrames = 0;

        obj.SetActive(ARHandler.active["resource"]);
        f1 = false;
    }

    void Update()
    {
        if (reactionFrames > 0)
        {
            reactionFrames--;

            string hit = ARHandler.GetHitIfAny();

            for (int i = 0; i < fish.Length; ++i)
            {
                if (hit.Equals(hitSphere[i].transform.name))
                {
                    Destroy(fish[i]);
                    Destroy(hitSphere[i]);
                    feedbackMarker[i].SetActive(true);
                    if (!f1)
                    {
                        ARHandler.GetAchievement("Finding Nome");
                    }
                    else
                    {
                        ARHandler.GetAchievement("Finding Dyro");
                    }
                }
            }
        }
        else if (ARHandler.active["resource"])
        {
            float selector = Random.value;
            if (selector < 0.5f)
            {
                if (fish[0] != null && hitSphere[0] != null)
                {
                    fish[0].SetActive(false);
                    hitSphere[0].SetActive(false);
                }
            }
            else
            {
                if (fish[0] != null && hitSphere[0] != null)
                {
                    fish[1].SetActive(false);
                    hitSphere[1].SetActive(false);
                }
            }

            float rand = Random.value;
            if (rand < probability)
            {
    		    // Show sphere for 1 second at a time
                reactionFrames = (int) (0.5f / Time.deltaTime);

                if (selector < 0.5f)
                {
                    if (fish[0] != null && hitSphere[0] != null)
                    {
                        fish[0].SetActive(true);
                        hitSphere[0].SetActive(true);
                    }
                }
                else
                {
                    if (fish[0] != null && hitSphere[0] != null)
                    {
                        fish[1].SetActive(true);
                        hitSphere[1].SetActive(true);
                    }
                }
            }
        }
    }
}
