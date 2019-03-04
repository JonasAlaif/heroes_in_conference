using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SearchForPlayerButton : MonoBehaviour {
    [SerializeField] TMP_InputField inputSearch;
    [SerializeField] GameObject networkErrorText, notFoundText;
    [SerializeField] Object playerProfilePrefab, playersShortTab;
    [SerializeField] RectTransform content;

    GameObject[] spawnedPlayerFoundTabs = new GameObject[0];

    public void SearchClicked() {
        notFoundText.SetActive(true);
        networkErrorText.SetActive(false);
        if (inputSearch.text == null)
            return;
        if (inputSearch.text == "")
            return;
        long[] playerIDs;
        string[] playerNames;
        if(!NetworkDatabase.NDB.SearchForPlayerOnServer(inputSearch.text, out playerIDs, out playerNames)) {
            notFoundText.SetActive(false);
            networkErrorText.SetActive(true);
        } else {
            if(playerIDs == null) {
                foreach (GameObject oldTab in spawnedPlayerFoundTabs) {
                    Destroy(oldTab);
                }
                spawnedPlayerFoundTabs = new GameObject[0];
                // Player not found
            } else {
                notFoundText.SetActive(false);
                networkErrorText.SetActive(false);
                // Player found
                GameObject[] newTabs = new GameObject[playerIDs.Length];
                for (int i = 0; i < playerIDs.Length || i < spawnedPlayerFoundTabs.Length; i++) {
                    if (i >= playerIDs.Length) {
                        Destroy(spawnedPlayerFoundTabs[i]);
                        continue;
                    }
                    GameObject playerFoundSelect;
                    if (spawnedPlayerFoundTabs.Length > i)
                        playerFoundSelect = spawnedPlayerFoundTabs[i];
                    else
                        playerFoundSelect = Instantiate(playersShortTab, content) as GameObject;
                    newTabs[i] = playerFoundSelect;
                    Button showPlayerButton = playerFoundSelect.GetComponent<Button>();
                    showPlayerButton.onClick.RemoveAllListeners();
                    long currIdToShow = playerIDs[i];
                    showPlayerButton.onClick.AddListener(() => ShowPlayerWithIdProfile(currIdToShow));
                    playerFoundSelect.GetComponentInChildren<TextMeshProUGUI>().text = playerNames[i];
                }
                spawnedPlayerFoundTabs = newTabs;
            }
        }
    }

    public void ShowPlayerWithIdProfile(long idToShow) {
        DBPlayer player = NetworkDatabase.NDB.GetPlayerById(idToShow);
        GameObject playerGO = Instantiate(playerProfilePrefab, transform) as GameObject;
        //playerGO.GetComponent<Button>().onClick.AddListener(() => Destroy(playerGO));
        Transform playerGOPanel = playerGO.transform.GetChild(1);
        if (NetworkDatabase.NDB.TryDownloadPlayerProfile(player)) {
            playerGOPanel.GetChild(0).GetComponentInChildren<Image>().sprite = FilePath.loadNewSprite(player.FP.Path);
        }
        playerGOPanel.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = player.PlayerName;
    }
}
