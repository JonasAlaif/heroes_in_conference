using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEventsDownloader : MonoBehaviour {
    Object poiPrefab;
    RectTransform theMap;
    long myMapId;
    float width, height;
    bool init = false;

    List<GameObject> allPois;

    public void Init(long newMapId, float width, float height, Object poiPrefab, RectTransform theMap) {
        myMapId = newMapId;
        this.width = width;
        this.height = height;
        this.poiPrefab = poiPrefab;
        this.theMap = theMap;
        init = true;
    }

    private void OnEnable() {
        if (!init)
            return;
        DBMap mapWithPois = NetworkDatabase.NDB.GetMapPOI(myMapId);
        if (mapWithPois != null) {
            allPois = new List<GameObject>(mapWithPois.dBMapPOIs.Count);
            foreach (DBMapPOI pOI in mapWithPois.dBMapPOIs) {
                float xPos = (pOI.X / 1000f), yPos = (pOI.Y / 1000f);
                if(width > height) {
                    xPos /= width / height;
                } else {
                    yPos /= height / width;
                }
                GameObject poiGO = Instantiate(poiPrefab, theMap) as GameObject;
                RectTransform poiRt = poiGO.GetComponent<RectTransform>();

                poiRt.anchoredPosition = new Vector2(theMap.rect.width * xPos, theMap.rect.height * yPos);

                PoiPrefabReferences refs = poiGO.GetComponent<PoiPrefabReferences>();
                refs.nameText.text = pOI.MapPoiName;
                refs.descText.text = pOI.MapPoiDescription;

                allPois.Add(poiGO);
            }
        }
    }

    private void OnDisable() {
        foreach (GameObject poi in allPois) {
            Destroy(poi);
        }
    }
}
