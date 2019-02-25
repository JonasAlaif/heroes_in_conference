using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraReset : MonoBehaviour
{
    GameObject Model;
    GameObject Camera;
    Vector3 initPos;
    Quaternion initRot;
    void Start()
    {
        initPos = Camera.transform.position;
        initRot  = Model.transform.localRotation;
    }

    public void clickedReset()
    {
        SceneManager.LoadScene("ModelViewer");
    }
}
