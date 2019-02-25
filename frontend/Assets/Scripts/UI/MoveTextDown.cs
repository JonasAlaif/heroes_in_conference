using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveTextDown : MonoBehaviour
{

    public RectTransform buttonRect;

    public void DownWithTheTextness()
    {
        Vector3 position;
        position = buttonRect.localPosition;
        buttonRect.localPosition = new Vector3(position.x, position.y - 4, position.z);
    }
}
