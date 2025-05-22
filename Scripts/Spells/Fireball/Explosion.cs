using System.Collections;
using UnityEngine;

/*
    Code by Gabriel Porlier
*/

public class Explosion : MonoBehaviour
{
    private int _damage;

    private Coroutine _coroutine = null;

    public void SetDamage(int damage)
    {
        _damage = damage;
        if (_coroutine == null)
        {
            StartCoroutine(DelayedDestroy());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Ennemy"))
        {
            other.gameObject.GetComponentInParent<Ennemy>().TakeDamage(_damage);
        }
        
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }
}
