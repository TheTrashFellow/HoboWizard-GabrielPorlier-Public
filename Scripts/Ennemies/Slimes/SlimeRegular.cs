using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Code by Gabriel Porlier
 * */
public class SlimeRegular : Ennemy
{
    [SerializeField] private float _stepDistance = default;
    [SerializeField] private float _timeBetweenSteps = default;
    [SerializeField] private float _distanceToExplode = default;

    [Header("ForDeathEvent")]
    [SerializeField] private GameObject _smallSlime = default;
    [SerializeField] private Transform _spawn1 = default;
    [SerializeField] private Transform _spawn2 = default;

    [SerializeField] private Animator _animator = default;

    private GameObject _slime1;
    private GameObject _slime2;

    private bool _isDying = false;

    [SerializeField] private GameObject _slimeSplitVFX = default;
    [SerializeField] private Collider _triggerCollider;

    [Space]
    [SerializeField] AudioClip _separatingAudio;
    [SerializeField] private AudioClip _slidingSound;
    private GameObject _slidingSoundObject;
    
    void Start()
    {
        _navMeshAgent.updatePosition = true; // Laisser le NavMeshAgent g�rer la position
        _navMeshAgent.updateRotation = true; // Laisser le NavMeshAgent g�rer la rotation        
        InvokeRepeating(nameof(NextDestinationTowardsPlayer), 1f, _timeBetweenSteps);

        try
        {
            _slidingSoundObject = AudioManager.Instance.SetLoopAudioObject(_slidingSound, 1);
            _slidingSoundObject.transform.position = _damagedEffectTransform.position;
            _slidingSoundObject.transform.SetParent(transform);
        }
        catch
        {
            Debug.Log("No audio object available");
        }
        

        SlimeAnimationRelay relay = GetComponentInChildren<SlimeAnimationRelay>();
        if (relay != null)
        {
            relay.slimeParent = this;
        }
    }

    private void Update()
    {

        if (!_isDying)
        {
            if (Vector3.Distance(_playerTargetPosition.position, transform.position) <= _distanceToExplode)
            {
                _navMeshAgent.enabled = false;
                _isDying = true;

                try
                {
                    _slidingSoundObject.transform.SetParent(null);
                    AudioManager.Instance.StopAudioObject(_slidingSoundObject);
                }
                catch
                {
                    Debug.Log("No object found");
                }
                

                _animator.SetTrigger("TriggerDeath");
            }
        }        
    }

    private void NextDestinationTowardsPlayer()
    {
        if (!_navMeshAgent.enabled) return;
        Vector3 directionToPlayer = (_playerTargetPosition.position - transform.position).normalized;
        Vector3 nextPosition = transform.position + directionToPlayer * _stepDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(nextPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            NavMeshPath path = new();
            if(_navMeshAgent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                _navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                Debug.Log("Path blocked");
            }
            
        }
        else
        {
            Debug.Log("Desired position off of Navmesh");
            _navMeshAgent.SetDestination(_playerTargetPosition.position);
        }
    }

    public override void TakeDamage(int damage)
    {
        if (!_isDying)
        {            
            _healthPointsCurrent -= damage;
            PoolingVFXDamage.Instance.EnnemyDamagedVFX(_damagedEffectTransform);

            if (_healthPointsCurrent <= 0)
            {
                _navMeshAgent.enabled = false;
                _isDying = true;
                _animator.SetTrigger("TriggerDeath");
            }
        }
       
    }

    [ContextMenu("Kill Ennemy")]
    public new void Die()
    {
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        _triggerCollider.enabled = false;
        GetComponent<Collider>().enabled = false;

        AudioManager.Instance.PlayAudioOneTime(_separatingAudio, 1, _damagedEffectTransform);

        _slime1 = Instantiate(_smallSlime, _spawn1.position, _spawn1.rotation);
        _slime2 = Instantiate(_smallSlime, _spawn2.position, _spawn2.rotation);

        _slime1.GetComponent<Rigidbody>().freezeRotation = true;
        _slime2.GetComponent<Rigidbody>().freezeRotation = true;

        _slime1.GetComponent<NavMeshAgent>().enabled = false; _slime1.GetComponent<Collider>().enabled = true;
        _slime2.GetComponent<NavMeshAgent>().enabled = false; _slime2.GetComponent<Collider>().enabled = true;

        _slime1.GetComponent<Rigidbody>().AddForce(_slime1.transform.forward * 50, ForceMode.Impulse);
        _slime2.GetComponent<Rigidbody>().AddForce(_slime2.transform.forward * 50, ForceMode.Impulse);
       

        StartCoroutine(Destroy());
        if (_slimeSplitVFX != null)
        {
            Instantiate(_slimeSplitVFX, transform.position, Quaternion.identity);
        }
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(2f);

        try
        {
            _slime1.GetComponent<Rigidbody>().linearVelocity = new Vector3(0,0,0);
            

            _slime1.GetComponent<Rigidbody>().useGravity = false;
            

            _slime1.GetComponent<NavMeshAgent>().enabled = true; _slime1.GetComponent<SlimeSmall>().enabled = true; _slime1.GetComponent<Collider>().enabled = false;
            
        }
        catch
        {

        }

        try
        {
            _slime2.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);

            _slime2.GetComponent<Rigidbody>().useGravity = false;

            _slime2.GetComponent<NavMeshAgent>().enabled = true; _slime2.GetComponent<SlimeSmall>().enabled = true; _slime2.GetComponent<Collider>().enabled = false;
        }
        catch { }

        Destroy(gameObject);
    }

}
