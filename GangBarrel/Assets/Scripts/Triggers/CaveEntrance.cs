using UnityEngine;

namespace Triggers
{
    public class CaveEntrance : MonoBehaviour
    {
        public RoundManager roundManager;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player entered the cave. Round-based mode started.");
            }
        }
    }
}
