using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NetworkErrorPopup : MonoBehaviour {
    public static Popup toBuild = null;
    private void Update() {
        if(toBuild != null) {
            BuildPopup(toBuild);
            toBuild = null;
        }
    }

    private void BuildPopup(Popup toBuild) { //Func<bool> ToDoOnButtonPress, Sprite backgroundSprite, Sprite buttonBackgroundSprite, RectTransform parent, Font textFont) {
        RectTransform popupPanel = buildPopupPanel(toBuild.parent, toBuild.backgroundSprite);
        buildErrorText("Network connection error!", popupPanel, toBuild.textFont);
        buildRetryButton(popupPanel, delegate {
            if (toBuild.ToDoOnButtonPress())
                Destroy(popupPanel.gameObject);
        }, toBuild.buttonBackgroundSprite, toBuild.textFont);
        popupPanel.localScale = Vector3.one;
    }

    #region Build GOs
    /// <summary>
    /// Build panel for popup
    /// </summary>
    private static RectTransform buildPopupPanel(RectTransform canvas, Sprite backgroundSprite) {
        RectTransform popupPanel = new GameObject("Popup Panel").AddComponent<RectTransform>();
        popupPanel.SetParent(canvas);
        setRectTransformPos(popupPanel, 0, 0, 1, 0.1f);
        addBorder(popupPanel.gameObject, backgroundSprite);
        AddEventTriggerToDestroyOnClick(popupPanel.gameObject);
        return popupPanel;
    }
    private static RectTransform buildErrorText(string error, RectTransform backgrnPanel, Font textFont) {
        RectTransform errorText = new GameObject("Popup Text").AddComponent<RectTransform>();
        errorText.SetParent(backgrnPanel);
        setRectTransformPos(errorText, 0.06f, 0.3f, 0.6f, 0.7f);
        Text errorTextUI = errorText.gameObject.AddComponent<Text>();
        errorTextUI.alignment = TextAnchor.MiddleLeft;
        errorTextUI.fontSize = 32;
        errorTextUI.font = textFont;
        errorTextUI.color = Color.white;
        errorTextUI.text = error;
        return errorText;
    }
    private static RectTransform buildRetryButton(RectTransform backgrnPanel, UnityAction ToDoOnButtonPress, Sprite buttonBackgroundSprite, Font textFont) {
        RectTransform retryButton = new GameObject("Popup Retry Button").AddComponent<RectTransform>();
        retryButton.SetParent(backgrnPanel);
        setRectTransformPos(retryButton, 0.6f, 0.2f, 0.95f, 0.8f);
        addBorder(retryButton.gameObject, buttonBackgroundSprite);
        Button retryButtonBtn = retryButton.gameObject.AddComponent<Button>();
        retryButtonBtn.onClick.AddListener(ToDoOnButtonPress);

        RectTransform retryText = new GameObject("Text").AddComponent<RectTransform>();
        retryText.SetParent(retryButton);
        setRectTransformPos(retryText, 0, 0, 1, 1);
        Text errorTextUI = retryText.gameObject.AddComponent<Text>();
        errorTextUI.alignment = TextAnchor.MiddleCenter;
        errorTextUI.fontSize = 32;
        errorTextUI.font = textFont;
        errorTextUI.color = Color.white;
        errorTextUI.text = "Retry";
        return retryButton;
    }


    #endregion

    #region Utility Methods
    private static void setRectTransformPos(RectTransform rt, float x0, float y0, float x1, float y1) {
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void addBorder(GameObject toAddTo, Sprite theBorder) {
        Image dtBorder = toAddTo.AddComponent<Image>();
        dtBorder.sprite = theBorder;
        dtBorder.type = Image.Type.Sliced;
    }

    private static void AddEventTriggerToDestroyOnClick(GameObject toDestroy) {
        EventTrigger popupET = toDestroy.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => Destroy(toDestroy.gameObject));
        popupET.triggers.Add(entry);
    }
    #endregion

    public class Popup {
        public readonly Func<bool> ToDoOnButtonPress;
        public readonly Sprite backgroundSprite, buttonBackgroundSprite;
        public readonly RectTransform parent;
        public readonly Font textFont;

        public Popup(Func<bool> toDoOnButtonPress, Sprite backgroundSprite, Sprite buttonBackgroundSprite, RectTransform parent, Font textFont) {
            ToDoOnButtonPress = toDoOnButtonPress;
            this.backgroundSprite = backgroundSprite;
            this.buttonBackgroundSprite = buttonBackgroundSprite;
            this.parent = parent;
            this.textFont = textFont;
        }
    }
}
