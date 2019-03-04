using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARHandler : MonoBehaviour
{
    // Hardcoded content groups
    public GameObject[] resources;
    public GameObject[] challenges;

    void Start()
    {
    }

    void Update()
    {
        // Update content groups based on NetworkDatabase
        toggleContentGroup(challenges, NetworkDatabase.NDB.GetContentGroupActiveByName("challenge"));
        toggleContentGroup(resources, NetworkDatabase.NDB.GetContentGroupActiveByName("resource"));
    }

    public static void GetAchievement(string achievement)
    {
        // Get achievement object 'achievement'
        DBAchievement mChiev = NetworkDatabase.NDB.GetAchievementByName(achievement);
        // Set achievement in Network Database
        NetworkDatabase.NDB.SetAchievement(mChiev.AchievementID);
        // Trigger popup window with achievement title and description
        PopupScript.ps.GotAchievement(mChiev.AchievementName, mChiev.AchievementDescription);

        // Get final achievement for getting all achievements
        if (NetworkDatabase.NDB.GetAllWonAchievements().Count == NetworkDatabase.NDB.GetAchievements().Count-1)
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("Mr. smartypants"));
        }

        if (ore())
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("It's all mine"));

            DBAchievement mChiev2 = NetworkDatabase.NDB.GetAchievementByName("It's all mine");
            NetworkDatabase.NDB.SetAchievement(mChiev2.AchievementID);
            PopupScript.ps.GotAchievement(mChiev2.AchievementName, mChiev2.AchievementDescription);
        }
        
        if (wood())
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("Mourning wood"));
            
            DBAchievement mChiev2 = NetworkDatabase.NDB.GetAchievementByName("Mourning wood");
            NetworkDatabase.NDB.SetAchievement(mChiev2.AchievementID);
            PopupScript.ps.GotAchievement(mChiev2.AchievementName, mChiev2.AchievementDescription);
        }
        
        if (fish())
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("Ocean man"));
            
            DBAchievement mChiev2 = NetworkDatabase.NDB.GetAchievementByName("Ocean man");
            NetworkDatabase.NDB.SetAchievement(mChiev2.AchievementID);
            PopupScript.ps.GotAchievement(mChiev2.AchievementName, mChiev2.AchievementDescription);
        }
    }

    // Return all ore collected
    private static bool ore()
    {
        return (NetworkDatabase.NDB.GetAchievementWonByName("Grumpy") && 
                NetworkDatabase.NDB.GetAchievementWonByName("Bashful") &&
                    NetworkDatabase.NDB.GetAchievementWonByName("Dopey"));
    }

    // Return all wood collected
    private static bool wood()
    {
        return (NetworkDatabase.NDB.GetAchievementWonByName("It's treeson!") && 
                NetworkDatabase.NDB.GetAchievementWonByName("Timber!!!") &&
                    NetworkDatabase.NDB.GetAchievementWonByName("Run Forest, run!"));
    }

    // Return all fish collected
    private static bool fish()
    {
        return (NetworkDatabase.NDB.GetAchievementWonByName("Finding Nome") && 
                NetworkDatabase.NDB.GetAchievementWonByName("Finding Dyro"));
    }

    // Return name of object interacted with (touched) or empty string if none
    public static string GetHitIfAny()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.transform.name;
            }
        }
        return "";
    }

    // Set imagetargets activeness to content group from Database
    private void toggleContentGroup(GameObject[] group, bool active)
    {
        for (int i = 0; i < group.Length; ++i)
        {
            group[i].SetActive(active);
        }
    }
}
