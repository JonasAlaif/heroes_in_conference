using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SlotScript : MonoBehaviour
{
    public int index;
    private InventoryScript inventory;
    public static int modelIndex;

    public void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("InvPanel").GetComponent<InventoryScript>();
    }

    public void OpenModelViewer()
    {
        if (inventory.slotFull[index])
        {
            modelIndex = index;
            SceneManager.LoadScene("ModelViewer");
        }

    }
}


