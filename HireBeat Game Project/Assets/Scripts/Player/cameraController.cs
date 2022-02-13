using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public float scrollSpeed = 10;
    public float minSize;
    public float maxSize;
    public GameObject UICamera;
    public Camera zoomCamera;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && zoomCamera.isActiveAndEnabled && zoomCamera.orthographic)
        {
            zoomCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            if (zoomCamera.orthographicSize < minSize) zoomCamera.orthographicSize = minSize;
            else if (zoomCamera.orthographicSize > maxSize) zoomCamera.orthographicSize = maxSize;
        }
        /*else
        {
            zoomCamera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        }*/
    }

    public void turnOnUICamera()
    {
        UICamera.SetActive(true);
    }

    public void turnOffUICamera()
    {
        UICamera.SetActive(false);
    }
}
