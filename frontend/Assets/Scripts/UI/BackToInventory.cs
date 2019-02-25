using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToInventory : MonoBehaviour
{
    public void BackToInv()
    {
        SceneManager.LoadScene("InventoryScene");
    }
}
