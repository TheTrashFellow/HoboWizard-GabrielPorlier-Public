using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Code by Gabriel Porlier

    Adapts the regular slime ennemy to fit it's new behaviour with the boss.

    The slime ennemy the boss can spawn with it's slime attack or when parrying the big boulder attack.

    Manages the small slime after a few seconds before being destroyed so it can "activate" them after the "ragdoll" effect.
 */

public class BossRegularSlime : Ennemy
{
    [SerializeField] private float _stepDistance = default;
    [SerializeField] private float _timeBetweenSteps = default;   

    [Space]
    [SerializeField] private Transform _gfx;
    [Space]

    [Header("ForDeathEvent")]
    [SerializeField] private GameObject _smallSlime = default;
    [SerializeField] private Transform _spawn1 = default;
    [SerializeField] private Transform _spawn2 = default;
    [SerializeField] private Animator _animator;
    [SerializeField] public Collider _triggerCollider;
    [SerializeField] public GameObject _slimeSplitVFX;

    private GameObject _slime1;
    private GameObject _slime2;

    private Transform _bossDestination;

    private bool _isDying = false;

    [Space]
    [SerializeField] AudioClip _separatingAudio;
    [SerializeField] private AudioClip _slidingSound;
    private GameObject _slidingSoundObject;

    
    void Start()
    {        
        _navMeshAgent.updatePosition = true; // Laisser le NavMeshAgent g�rer la position
        _navMeshAgent.updateRotation = true; // Laisser le NavMeshAgent g�rer la rotation        
        InvokeRepeating(nameof(NextDestinationTowardsBoss), 1f, _timeBetweenSteps);

        _slidingSoundObject = AudioManager.Instance.SetLoopAudioObject(_slidingSound, 1);
        _slidingSoundObject.transform.position = _damagedEffectTransform.position;
        _slidingSoundObject.transform.SetParent(transform);

        StartCoroutine(DelayedTriggerColliderEnable());
    }

    public void SetDestination(Transform destination)
    {
        _bossDestination = destination;
    }

    private void NextDestinationTowardsBoss()
    {
        if (!_navMeshAgent.enabled) return;
        Vector3 directionToPlayer = (_bossDestination.position - transform.position).normalized;
        Vector3 nextPosition = transform.position + directionToPlayer * _stepDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(nextPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
        }
    }

    public override void TakeDamage(int damage)
    {
        if (!_isDying)
        {
            _healthPointsCurrent -= damage;
            Debug.Log("Took damage : " + damage);

            if (_healthPointsCurrent <= 0)
            {
                Debug.Log("Should be dying");
                _isDying = true;
                GetComponent<NavMeshAgent>().enabled = false;

                _slidingSoundObject.transform.SetParent(null);
                AudioManager.Instance.StopAudioObject(_slidingSoundObject);

                _animator.SetTrigger("TriggerDeath");  
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_triggerCollider.enabled) return;

        if(other.gameObject.tag == "Ennemy")
        {
            try
            {
                other.gameObject.GetComponentInParent<SlimeBoss>().HealBack(0.05f);
                Destroy(gameObject);
            }
            catch
            {
                return;
            }
        }
    }
    public new void Die()
    {
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        _triggerCollider.enabled = false;
        GetComponent<Collider>().enabled = false;

        AudioManager.Instance.PlayAudioOneTime(_separatingAudio, 1, _damagedEffectTransform);

        if (_slimeSplitVFX != null)
        {
            Instantiate(_slimeSplitVFX, transform.position, Quaternion.identity);
        }

        Debug.Log("Death");
        _slime1 = Instantiate(_smallSlime);
        _slime2 = Instantiate(_smallSlime);

        _slime1.GetComponent<BossSmallSlime>().enabled = false; _slime1.GetComponent<NavMeshAgent>().enabled = false; _slime1.GetComponent<Collider>().enabled = true;
        _slime2.GetComponent<BossSmallSlime>().enabled = false; _slime2.GetComponent<NavMeshAgent>().enabled = false; _slime2.GetComponent<Collider>().enabled = true;

        _slime1.transform.SetPositionAndRotation(_spawn1.position, _spawn1.rotation);
        _slime2.transform.SetPositionAndRotation(_spawn2.position, _spawn2.rotation);

        _slime1.GetComponent<Rigidbody>().AddForce(_slime1.transform.forward * 10, ForceMode.Impulse);
        _slime2.GetComponent<Rigidbody>().AddForce(_slime2.transform.forward * 10, ForceMode.Impulse);

        _animator.enabled = false;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        StartCoroutine(DelayedDestroy());
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(4f);

        try
        {        

            _slime1.GetComponent<NavMeshAgent>().enabled = true;
            _slime1.GetComponent<BossSmallSlime>().enabled = true;
            _slime1.GetComponent<Collider>().enabled = false;
            _slime1.GetComponent<Rigidbody>().useGravity = false;
            _slime1.GetComponent<BossSmallSlime>().SetDestination(_bossDestination);
        }
        catch { }

        try
        {
            _slime2.GetComponent<NavMeshAgent>().enabled = true;
            _slime2.GetComponent<BossSmallSlime>().enabled = true;
            _slime2.GetComponent<Collider>().enabled = false;
            _slime2.GetComponent<Rigidbody>().useGravity = false;
            _slime2.GetComponent<BossSmallSlime>().SetDestination(_bossDestination);
        }
        catch { }
           
        

        Destroy(gameObject);
    }

    IEnumerator DelayedTriggerColliderEnable()
    {
        yield return new WaitForSeconds(1);

        _triggerCollider.enabled = true;
        

        yield return new WaitForSeconds(3);
        GetComponent<Rigidbody>().isKinematic = true;
        _navMeshAgent.enabled = true;
    }
}
