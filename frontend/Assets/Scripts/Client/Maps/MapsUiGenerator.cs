using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;


/// <summary>
/// Generates a GO for each map and fills dropdown selector
/// </summary>
public class MapsUiGenerator : MonoBehaviour {
    [SerializeField] RectTransform mapsParent;
    [SerializeField] TMP_Dropdown mapsSelector;
    [SerializeField] List<RectTransform> mapGameObjects;
    [SerializeField] Object poiPrefab;

    private void Start() {
        NetworkDatabase.NDB.MapsDownloaded.AddListener(() => shouldBuildMaps = true);
    }

    private bool shouldBuildMaps;
    private void Update() {
        if (shouldBuildMaps) {
            buildMaps();
            shouldBuildMaps = false;
        }
    }

    private void buildMaps() {
        if (mapGameObjects != null) {
            foreach (RectTransform rt in mapGameObjects)
                Destroy(rt.gameObject);
        }
        mapGameObjects = new List<RectTransform>();

        List<DBMap> allMaps = NetworkDatabase.NDB.GetMaps();
        foreach (DBMap map in allMaps) {
            RectTransform mapGO = buildMapPanel();
            mapGO.gameObject.SetActive(false);
            mapGameObjects.Add(mapGO);
            if (NetworkDatabase.NDB.TryDownloadMap(map)) {
                RectTransform mapGraphic = new GameObject("Map Graphic").AddComponent<RectTransform>();
                mapGraphic.SetParent(mapGO);
                setRectTransformPos(mapGraphic, 0, 0, 1, 1);

                // The map path is local (downloaded)
                Image img = mapGraphic.gameObject.AddComponent<Image>();
                img.preserveAspect = true;
                Sprite loadedSprite = FilePath.loadNewSprite(map.FP.Path);
                img.sprite = loadedSprite;

                float imgWidth = loadedSprite.rect.width, imgHeight = loadedSprite.rect.height;

                float aspectRatioParent = mapGO.rect.width / mapGO.rect.height;
                float aspectRatioImage = imgWidth / imgHeight;
                AspectRatioFitter arf = mapGraphic.gameObject.AddComponent<AspectRatioFitter>();
                if (aspectRatioParent < aspectRatioImage)
                    arf.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                else
                    arf.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                arf.aspectRatio = aspectRatioImage;



                mapGO.gameObject.AddComponent<MapEventsDownloader>().Init(map.MapID, imgWidth, imgHeight, poiPrefab, mapGraphic);
            }
            else {
                Text errText = mapGO.gameObject.AddComponent<Text>();
                errText.text = "Network (or maybe disk) error encountered when loading map!";
                errText.alignment = TextAnchor.MiddleCenter;
                errText.fontSize = 100;
            }
            mapGO.localScale = Vector3.one;
        }

        mapsSelector.ClearOptions();
        mapsSelector.AddOptions(allMaps.Select(map => map.MapName).ToList());
        mapsSelector.onValueChanged.AddListener(delegate {
            for (int i = 0; i < mapGameObjects.Count; i++)
                mapGameObjects[i].gameObject.SetActive(i == mapsSelector.value);
        });
        if (mapGameObjects.Count > 0)
            mapGameObjects[0].gameObject.SetActive(true);
    }

    #region Build GameObject
    private RectTransform buildMapPanel() {
        RectTransform mapPanel = new GameObject("Map Panel").AddComponent<RectTransform>();
        mapPanel.SetParent(mapsParent);
        setRectTransformPos(mapPanel, 0, 0, 1, 1);
        return mapPanel;
    }
    #endregion

    #region Utiliy
    private void setRectTransformPos(RectTransform rt, float x0, float y0, float x1, float y1) {
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
    #endregion
}
