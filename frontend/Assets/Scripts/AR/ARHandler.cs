using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARHandler : MonoBehaviour
{
    public static Dictionary<string, bool> active = new Dictionary<string, bool>();

    // Start is called before the first frame update
    void Start()
    {
	    // Lock screen rotation in the AR camera, as rotation resets and reloads the GameObjects
        active.Add("challenge", NetworkDatabase.NDB.GetContentGroupActiveByName("challenge"));
        active.Add("resource", NetworkDatabase.NDB.GetContentGroupActiveByName("resource"));


        InvokeRepeating("CheckActive", 2.0f, 5.0f);
    }

    void Update()
    {
        print(ARHandler.active["resource"]);
    }

    void CheckActive()
    {
        active["challenge"] = NetworkDatabase.NDB.GetContentGroupActiveByName("challenge");
        active["resource"] = NetworkDatabase.NDB.GetContentGroupActiveByName("resource");

    }

    public static void GetAchievement(string achievement)
    {
        DBAchievement mChiev = NetworkDatabase.NDB.GetAchievementByName(achievement);
        NetworkDatabase.NDB.SetAchievement(mChiev.AchievementID);
        PopupScript.ps.GotAchievement(mChiev.AchievementName, mChiev.AchievementDescription);
        if (NetworkDatabase.NDB.GetAllWonAchievements().Count == NetworkDatabase.NDB.GetAchievements().Count-1)
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("Mr. smartypants"));
        }

        if (Ore())
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("It's all mine"));
        }
        
        if (Wood())
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("Mourning wood"));
        }
        
        if (Fish())
        {
            NetworkDatabase.NDB.SetAchievement(NetworkDatabase.NDB.GetAchievementIdByName("Ocean man"));
        }
    }

    private static bool Ore()
    {
        return (NetworkDatabase.NDB.GetAchievementWonByName("Grumpy") && 
                NetworkDatabase.NDB.GetAchievementWonByName("Bashful") &&
                    NetworkDatabase.NDB.GetAchievementWonByName("Dopey"));
    }

    private static bool Wood()
    {
        return (NetworkDatabase.NDB.GetAchievementWonByName("It's treeson!") && 
                NetworkDatabase.NDB.GetAchievementWonByName("Timber!!!") &&
                    NetworkDatabase.NDB.GetAchievementWonByName("Run Forest, run!"));
    }

    private static bool Fish()
    {
        return (NetworkDatabase.NDB.GetAchievementWonByName("Finding Nome") && 
                NetworkDatabase.NDB.GetAchievementWonByName("Finding Dyro"));
    }

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
}
