using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class AchievementScript : MonoBehaviour {
    [SerializeField] RectTransform achievementContent;
    [SerializeField] Object achievementPrefab;

    public Dictionary<long, GameObject> achievementButtons = new Dictionary<long, GameObject>();
    public Dictionary<long, DBAchievement> dbAchievements;

    public void Start() {
        NetworkDatabase.NDB.AchievementsDownloaded.AddListener(() => buildAchievements = true);
    }

    List<long> gotAchievements;
    private bool buildAchievements = true, showWon;
    private void Update() {
        if (buildAchievements) {
            GenerateAchievementGOs();
            buildAchievements = false;
        }
        if (showWon) {
            ShowWon();
            showWon = false;
        }
    }

    private void GenerateAchievementGOs() {
        dbAchievements = NetworkDatabase.NDB.GetAchievements();

        Debug.Log("Displaying " + dbAchievements.Count + " achs");
        foreach (GameObject achGo in achievementButtons.Values) {
            Destroy(achGo);
        }
        achievementButtons.Clear();
        foreach (DBAchievement ach in dbAchievements.Values) {
            GameObject achGO = Instantiate(achievementPrefab, achievementContent) as GameObject;
            achGO.transform.name = "Achievement " + ach.AchievementName;
            TextMeshProUGUI[] texts = achGO.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = ach.AchievementName;
            texts[1].text = ach.AchievementDescription;
            if(!NetworkDatabase.NDB.GetAchievementWonById(ach.AchievementID))
                achGO.transform.GetChild(3).gameObject.SetActive(false);
            achievementButtons.Add(ach.AchievementID, achGO);
        }
        achievementContent.anchoredPosition = new Vector2(0, -5000);
    }

    private void ShowWon() {
        List<DBAchievement> allWon = NetworkDatabase.NDB.GetAllWonAchievements();
        foreach (DBAchievement ach in allWon) {
            achievementButtons[ach.AchievementID].transform.GetChild(3).gameObject.SetActive(true);
        }
    }

    public void GetAndSetAchieved() {
        new Thread(delegate () {
            NetworkDatabase.NDB.UpdateAllWonAchievementsServer();
            showWon = true;

        }).Start();
    }
}
