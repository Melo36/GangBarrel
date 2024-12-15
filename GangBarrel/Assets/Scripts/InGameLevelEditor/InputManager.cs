using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] 
    private Camera sceneCamera;

    private Vector3 lastPosition;

    [SerializeField] 
    private LayerMask placementLayerMask;

    public event Action onClicked, OnExit;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            onClicked?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExit?.Invoke();
        }
    }

    public bool isPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placementLayerMask))
        {
            lastPosition = hit.point;
        }

        return lastPosition;
    }
}
