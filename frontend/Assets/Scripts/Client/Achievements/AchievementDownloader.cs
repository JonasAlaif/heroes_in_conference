using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementDownloader : MonoBehaviour {
    [SerializeField] AchievementScript achS;

    private void OnEnable() {
        achS.GetAndSetAchieved();
    }
}
