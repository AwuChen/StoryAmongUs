using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUpdate : MonoBehaviour
{
    GameObject LP;
    private void Start()
    {
        
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    if (LP != null)
        //    {
        //        int multiplier = Input.GetKey(KeyCode.RightArrow) ? 1 : -1;

        //        //LP.GetComponent<PlayerManager>().Interact(multiplier.ToString());
        //    }
        //    if (LP == null)
        //    {
        //        LP = GameObject.FindGameObjectWithTag("LocalPlayer");
        //    }
        //}
    }


    public void UpdateWorld(string obj)
    {
        if (LP == null)
        {
            LP = GameObject.FindGameObjectWithTag("LocalPlayer");
        }
        if (LP != null)
        {
            LP.GetComponent<PlayerManager>().Interact(obj);
        }
    }
}
