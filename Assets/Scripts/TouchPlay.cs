using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPlay : MonoBehaviour
{
    public GameObject marspopupPrefab;
    public GameObject earthPopupPrefab; 

    // Public variables for popup offsets
    public Vector3 marsPopupOffset = new Vector3(0, 0.25f, 0.25f);
    public Vector3 earthPopupOffset = new Vector3(0, 0.25f, 0.25f);

    private GameObject currentMarsPopupInstance;
    private Transform lastMarsObjectHit;

    private GameObject currentEarthPopupInstance;
    private Transform lastEarthObjectHit;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.tag == "mars")
                {
                    if (currentMarsPopupInstance != null && lastMarsObjectHit == hit.transform)
                    {
                        Destroy(currentMarsPopupInstance);
                        currentMarsPopupInstance = null;
                        lastMarsObjectHit = null;
                    }
                    else
                    {
                        if (currentMarsPopupInstance != null)
                        {
                            Destroy(currentMarsPopupInstance);
                        }
                        // Use the public offset for Mars
                        Vector3 pos = hit.point + marsPopupOffset; 
                        currentMarsPopupInstance = Instantiate(marspopupPrefab, pos, transform.rotation); 
                        lastMarsObjectHit = hit.transform;
                    }
                }
                else if (hit.transform.tag == "marsInfo")
                {
                    if (currentMarsPopupInstance != null && hit.transform.gameObject == currentMarsPopupInstance)
                    {
                        Destroy(currentMarsPopupInstance);
                        currentMarsPopupInstance = null;
                        lastMarsObjectHit = null; 
                    }
                    else if (currentMarsPopupInstance == null && hit.transform.parent == null) 
                    {
                        Destroy(hit.transform.gameObject);
                    }
                }
                else if (hit.transform.tag == "Earth")
                {
                    if (currentEarthPopupInstance != null && lastEarthObjectHit == hit.transform)
                    {
                        Destroy(currentEarthPopupInstance);
                        currentEarthPopupInstance = null;
                        lastEarthObjectHit = null;
                    }
                    else
                    {
                        if (currentEarthPopupInstance != null)
                        {
                            Destroy(currentEarthPopupInstance);
                        }
                        // Use the public offset for Earth
                        Vector3 pos = hit.point + earthPopupOffset;
                        currentEarthPopupInstance = Instantiate(earthPopupPrefab, pos, transform.rotation); 
                        lastEarthObjectHit = hit.transform;
                    }
                }
                else if (hit.transform.tag == "EarthInfo")
                {
                    if (currentEarthPopupInstance != null && hit.transform.gameObject == currentEarthPopupInstance)
                    {
                        Destroy(currentEarthPopupInstance);
                        currentEarthPopupInstance = null;
                        lastEarthObjectHit = null; 
                    }
                    else if (currentEarthPopupInstance == null && hit.transform.parent == null)
                    {
                        Destroy(hit.transform.gameObject);
                    }
                }
            }
        }
    }
}
