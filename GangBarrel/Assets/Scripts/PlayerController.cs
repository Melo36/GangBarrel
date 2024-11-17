using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Camera mainCamera;

    private float speed = 10;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject bullet = Instantiate(bulletPrefab);
            Destroy(bullet, 5f);
            Vector3 mousePosition = new Vector3();
            Plane plane = new Plane(Vector3.up, 0);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (plane.Raycast(ray, out float distance))
            {
                mousePosition = ray.GetPoint(distance);
            }

            mousePosition = new Vector3(mousePosition.x, transform.position.y, mousePosition.z);
            Vector3 direction = (mousePosition - transform.position).normalized;
            bullet.GetComponent<Rigidbody>().AddForce(direction * speed, ForceMode.VelocityChange);
        }
    }
}
