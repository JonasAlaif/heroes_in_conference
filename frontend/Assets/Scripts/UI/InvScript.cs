using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InvScript : MonoBehaviour
{
    public void GoToInventory()
    {
        SceneManager.LoadScene("InventoryScene");
    }
}
