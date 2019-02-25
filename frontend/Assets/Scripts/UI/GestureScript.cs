using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRubyShared;
using UnityEngine.SceneManagement;

public class GestureScript : MonoBehaviour
{
    public GameObject Camera;
    public GameObject toRotate;
    public FingersScript FingerScript;
    private PanGestureRecognizer panGesture;
    private RotateGestureRecognizer rotateGesture;
    private PanGestureRecognizer swipeGesture;

    private void PanGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            float deltaX = panGesture.DeltaX / 10.0f;
            float deltaY = panGesture.DeltaY / 10.0f;
            Vector3 pos = Camera.transform.position;
            pos.x += deltaX;
            pos.y += deltaY;
            Camera.transform.position = pos;
        }
    }

    private void CreatePanGesture()
    {
        panGesture = new PanGestureRecognizer();
        panGesture.MinimumNumberOfTouchesToTrack = panGesture.MaximumNumberOfTouchesToTrack = 2;
        panGesture.StateUpdated += PanGestureCallback;
        FingersScript.Instance.AddGesture(panGesture);
    }

    private void SwipeGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            float deltaX = swipeGesture.DeltaX / 50.0f;
            float deltaY = swipeGesture.DeltaY / 50.0f;
            toRotate.transform.RotateAround(Vector3.up, deltaX);
            toRotate.transform.RotateAround(Vector3.right, deltaY);
        }
    }

    private void CreateSwipeGesture()
    {
        swipeGesture = new PanGestureRecognizer();
        swipeGesture.MinimumNumberOfTouchesToTrack = swipeGesture.MaximumNumberOfTouchesToTrack = 1;
        swipeGesture.StateUpdated += SwipeGestureCallback;
        FingersScript.Instance.AddGesture(swipeGesture);
    }

    private void RotateGestureCallback(GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            Camera.transform.Rotate(0.0f, 0.0f, rotateGesture.RotationRadiansDelta * Mathf.Rad2Deg);
        }
    }

    private void CreateRotateGesture()
    {
        rotateGesture = new RotateGestureRecognizer();
        rotateGesture.StateUpdated += RotateGestureCallback;
        FingersScript.Instance.AddGesture(rotateGesture);
    }


    void Start()
    {
        GameObject[] modelFinder;
        modelFinder = GameObject.FindGameObjectsWithTag("ModelToView");
        foreach (GameObject p in modelFinder)
        {
            if (p.active)
            {
                toRotate = p;
                break;
            }
        }
        CreatePanGesture();
        CreateRotateGesture();
        CreateSwipeGesture();
        panGesture.AllowSimultaneousExecution(rotateGesture);
    }

}
