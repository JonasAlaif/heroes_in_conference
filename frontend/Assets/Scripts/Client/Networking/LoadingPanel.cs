using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingPanel : MonoBehaviour {
    public static LoadingPanel LP;

    [SerializeField] GameObject loadingPanel;
    [SerializeField] TextMeshProUGUI loadingText;

    private bool setActive;
    private bool setInactive;
    private bool addDot;
    private void Awake() {
        LP = this;
    }

    private void Update() {
        if (setActive) {
            loadingPanel.SetActive(true);
            loadingText.text = "Loading";
            setActive = false;
        }
        if (setInactive) {
            loadingPanel.SetActive(false);
            setInactive = false;
        }
        if (addDot) {
            loadingText.text += ".";
            addDot = false;
        }
    }

    public void StartLoading() {
        setActive = true;
        
    }

    public void AddLoadingDot() {
        addDot = true;
    }

    public void FinishLoading() {
        setInactive = true;
    }
}
