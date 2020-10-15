using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link : MonoBehaviour
{
    public string link;
    bool runOnce = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        CheckInput();
    }

    void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); RaycastHit mouseHit;

            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                if (mouseHit.transform.GetComponent<Link>() != null)
                {
                    if(!runOnce)
                    {
                        StartCoroutine(LoadLink());
                    }
                }
                //else if (mouseHit.transform.GetComponent<Walkable>(). != null)
            }
        }
    }

    public void OpenLink()
    {
        if (!runOnce)
        {
            StartCoroutine(LoadLink());
        }
    }

    IEnumerator LoadLink()
    {
        //yield on a new YieldInstruction that waits for 3 seconds.
        yield return new WaitForSeconds(0);

        Application.OpenURL(link);
        runOnce = true;
    }
}
