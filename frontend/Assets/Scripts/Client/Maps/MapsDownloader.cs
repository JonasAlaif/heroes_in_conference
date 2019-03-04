using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapsDownloader : MonoBehaviour {
    private void OnEnable() {
        NetworkDatabase.NDB.DownloadAllMaps();
    }
}
