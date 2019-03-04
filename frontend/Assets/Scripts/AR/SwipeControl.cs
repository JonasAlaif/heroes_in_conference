using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeControl : MonoBehaviour
{
    public Swipe swipe;
    public GameObject obj;
    public GameObject obj2;
    public GameObject hint;
    public GameObject hintsphere;

    public GameObject[] planks;

    Vector3 moveTo;

    int which;

    void Start()
    {
        moveTo = obj.transform.position;
        hint.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Yak whisperer"));
        hintsphere.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Yak whisperer"));
        for (int i = 0; i < planks.Length; ++i)
        {
            planks[i].SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("Yak whisperer"));
        }

        which = 0;
    }

    void Update()
    {
        // TODO: fix directions
        if (obj.GetComponentInChildren<SkinnedMeshRenderer>().isVisible)
        {
            string item = ARHandler.GetHitIfAny();

            moveTo = Vector3.zero;

            // Based on swipe, manipulate transform vector
            if (swipe.GetUp())
            {
                moveTo += new Vector3(0, 0, 0.001f);
            }
            if (swipe.GetDown())
            {
                moveTo += new Vector3(0, 0, -0.001f);
            }
            if (swipe.GetLeft())
            {
                moveTo += new Vector3(0.001f, 0, 0);
            }
            if (swipe.GetRight())
            {
                moveTo += new Vector3(-0.001f, 0, 0);
            }

            //moveTo = new Vector3(-0.001f, 0, 0);

            if ((planks[which].transform.position - hint.transform.position).magnitude < 0.3f)
            {
                planks[which].transform.position = Vector3.MoveTowards(planks[which].transform.position, planks[which].transform.position + 5 * moveTo, 20 * Time.deltaTime);
            }
            else
            {
                if (which < 4)
                which++;
            }

            if (ARHandler.GetHitIfAny().Equals(hintsphere.name))
            {
                Destroy(hint);
                Destroy(hintsphere);
                ARHandler.GetAchievement("Yak whisperer");
            }
        }
    }
}
