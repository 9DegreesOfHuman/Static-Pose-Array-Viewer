using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform parent;
    public float travelSpeed = 1.0f;
    private void Update()
    {
        if (parent.childCount > 0)
        {
            if (Input.GetMouseButton(0))
            {
                // Debug.Log(parent.GetChild(0).name);
                transform.LookAt(parent.GetChild(0));
                Vector2 rotateDir = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

                transform.Translate(travelSpeed * rotateDir);
            }
        }
    }
}
