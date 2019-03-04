using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryScript : MonoBehaviour
{
    public bool[] slotFull;
    public GameObject[] slot;
    public void goBackToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Start()
    {
        slotFull[0] = slotFull[1] = slotFull[2] = slotFull[3] = true; //resources
        slotFull[5] = true;
        DBAchievement YetiAchievement = NetworkDatabase.NDB.GetAchievementByName("Yeti slayer");
        if (YetiAchievement != null)
        {
            slotFull[4] = NetworkDatabase.NDB.GetAchievementWonById(YetiAchievement.AchievementID);
            slotFull[8] = NetworkDatabase.NDB.GetAchievementWonById(YetiAchievement.AchievementID);
            
        }
        DBAchievement YakAchievement = NetworkDatabase.NDB.GetAchievementByName("Yak whisperer");
        if (YakAchievement != null)
        {
            slotFull[6] = NetworkDatabase.NDB.GetAchievementWonById(YakAchievement.AchievementID);
            slotFull[7] = NetworkDatabase.NDB.GetAchievementWonById(YakAchievement.AchievementID);
            slotFull[10]= NetworkDatabase.NDB.GetAchievementWonById(YakAchievement.AchievementID);
        }
        DBAchievement ZygAchievement=NetworkDatabase.NDB.GetAchievementByName("TNT I'm Zygomite");
        if(ZygAchievement!=null)
        {
            slotFull[9]= NetworkDatabase.NDB.GetAchievementWonById(ZygAchievement.AchievementID);
            slotFull[12]= NetworkDatabase.NDB.GetAchievementWonById(ZygAchievement.AchievementID);
        }
        DBAchievement CrabAchievement=NetworkDatabase.NDB.GetAchievementByName("Crab rave");
        if(CrabAchievement!=null)
        {
            slotFull[11]= NetworkDatabase.NDB.GetAchievementWonById(CrabAchievement.AchievementID);
            slotFull[13]= NetworkDatabase.NDB.GetAchievementWonById(CrabAchievement.AchievementID);
        }
        for (int i = 0; i < slotFull.Length; i++)
        {
            slot[i].SetActive(slotFull[i]);
            slot[i].transform.GetChild(0).gameObject.SetActive(slotFull[i]);
        }
    }
}
