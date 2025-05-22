using System.Collections;
using UnityEngine;

/*
 * Code by Gabriel Porlier 

    If parried, go back towards the ennemy that shot it. Could have used inheritance for these ennemy projectiles / projectiles in general, or an interface ? 
 * */
public class FlyingEnnemiesAttack : MonoBehaviour
{
    [SerializeField] protected float _launchSpeed;
    [SerializeField] protected float _duration;
    private int _damage;

    private bool _parried = false;
    public GameObject _attacker;
    private Rigidbody _rigidbody;

    private Transform _target;

    [SerializeField] private AudioClip _attackSound;
    GameObject _audioSourceObject;
    private void Update()
    {
        if (_parried && _attacker != null)
        {
            Transform targetPoint = _attacker.transform.Find("TargetPoint");
            if (targetPoint != null)
            {
                transform.LookAt(targetPoint);
                _rigidbody.linearVelocity = transform.forward * _launchSpeed;
            }
        }
        else
        {
            transform.LookAt(_target.transform);
            _rigidbody.linearVelocity = transform.forward * _launchSpeed;
        }

        if (_attacker == null)
        {
            Destroy(gameObject);
        }
    }

    public void CastSpell(Vector3 spawnPosition, Quaternion spawnDirection, Transform target)
    {
        transform.SetPositionAndRotation(spawnPosition, spawnDirection);
        _rigidbody = GetComponent<Rigidbody>();
        _target = target;

        _audioSourceObject = AudioManager.Instance.SetLoopAudioObject(_attackSound, 1);
        _audioSourceObject.transform.position = transform.position;
        _audioSourceObject.transform.SetParent(transform);

        StartCoroutine(SpellExpires());
    }

    public void SetSourceGameObject(GameObject attacker)
    {
        _attacker = attacker;
        _damage = _attacker.GetComponentInParent<Ennemy>()._damage;
    }

    IEnumerator SpellExpires()
    {
        yield return new WaitForSeconds(_duration);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }

        if (!_parried)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.GetComponent<Player>().TakeDamage(_damage);
                Destroy(gameObject);
            }

            if (other.gameObject.CompareTag("Shield"))
            {
                _parried = other.GetComponentInParent<ShieldManager>().BlockedAttack();
                if (!_parried)
                {
                    Destroy(gameObject);
                }
                _launchSpeed *= 2;
            }
        }
        else
        {
            if(other.gameObject.CompareTag("Ennemy"))
            {
                Ennemy ennemy = other.GetComponentInParent<Ennemy>();
                int damage = ennemy._healthPoints/2;
                ennemy.TakeDamage(damage);
                
                Destroy(gameObject);
                
            }
            if(other.gameObject.CompareTag("EnnemyAttack"))
            {
                Destroy(other.gameObject);
            }
        }
        
    }

    private void OnDestroy()
    {
        _audioSourceObject.transform.SetParent(null);
        AudioManager.Instance.StopAudioObject(_audioSourceObject);
    }


}
