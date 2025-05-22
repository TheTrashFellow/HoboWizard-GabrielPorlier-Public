using System.Collections;
using UnityEngine;

/*
 * Code by Gabriel Porlier

    Small rubble attack of the boss.
 */

public class RubbleAttackRegular : MonoBehaviour
{
    private int _damage;
    private Transform _centerPoint;
    private Rigidbody _rigidbody;
    private bool _parried = false;
    [SerializeField] private float _launchSpeed = 4;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_parried)
        {
            transform.LookAt(_centerPoint.transform);
            _rigidbody.linearVelocity = transform.forward * _launchSpeed;
        }
    }

    public void SetStats(int damage, Transform centerPoint)
    {
        _damage = damage;
        _centerPoint = centerPoint;
        StartCoroutine(DelayedDestroy());
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!_parried)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.GetComponent<Player>().TakeDamage(_damage);

                //Destory rubble sound
                Destroy(gameObject);
            }

            if (other.gameObject.CompareTag("Shield"))
            {
                _parried = other.GetComponentInParent<ShieldManager>().BlockedAttack();
                if (!_parried)
                {
                    //Destory rubble sound
                    Destroy(gameObject);
                }
                else
                {
                    return;
                }
            }
        }        

        if (_parried)
        {
            if (other.gameObject.CompareTag("Ennemy"))
            {
                other.gameObject.GetComponentInParent<Ennemy>().TakeDamage(_damage*2);

                Destroy(gameObject);
            }
        }
        
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(3);

        Destroy(gameObject);
    }
}
