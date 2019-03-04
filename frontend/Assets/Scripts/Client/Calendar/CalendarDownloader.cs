using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalendarDownloader : MonoBehaviour {
    private void OnEnable() {
        NetworkDatabase.NDB.DownloadAllEvents();
    }
}
