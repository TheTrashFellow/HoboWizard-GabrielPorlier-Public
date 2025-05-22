using UnityEngine;

/*
    Code by Gabriel Porlier
*/

public class ConsumableInteractions : MonoBehaviour
{
    [SerializeField] private Player _player;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Consummable"))
        {
            other.gameObject.GetComponent<Consummable>().ConsummableEffect(_player);
        }
    }
}
