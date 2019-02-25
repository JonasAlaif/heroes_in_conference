using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryScript : MonoBehaviour
{
    public bool[] slotFull;
    public GameObject[] slot;
    public void goBackToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
