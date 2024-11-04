using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StencilGround : MonoBehaviour
{
    [SerializeField]
    private MainManager mainManager;


    private void OnMouseEnter()
    {
        if (mainManager != null)
        {
            mainManager.OnDrag(true);
        }
    }

    private void OnMouseExit()
    {
        if (mainManager != null)
        {
            mainManager.OnDrag(false);
        }
    }
}
