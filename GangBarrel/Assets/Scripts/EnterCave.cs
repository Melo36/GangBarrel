using UnityEngine;

public class EnterCave : MonoBehaviour
{
     public Transform insideCavePosition;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = insideCavePosition.position;
        }
    }
}
