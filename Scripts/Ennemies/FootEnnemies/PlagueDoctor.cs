using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Code by Gabriel Porlier  
 * */
public class PlagueDoctor : Ennemy
{

    [SerializeField] private float _movementRadiusMax = default;    
    [SerializeField] private float _waitTimeBetweenMovements = default;
    [SerializeField] private float _maxDistanceForMelee = default;
    [SerializeField] private float _chanceForRangedAttack = default;

    [Space]
    [Header("For ennemy ranged attack")]
    [SerializeField] private Transform _attackSpawnPoint = default;
    [SerializeField] private GameObject _rangedAttack = default;
    [SerializeField] private GameObject _parryTarget = default;

    [Space]
    private bool _isDestinationSet = false;
    private Vector3 _destination;
    private bool _startedCoroutine = false;

    [SerializeField] private Animator _animator = default;

    [SerializeField] private GameObject _deathVFX = default;
    [SerializeField] private Vector3 _deathVFXOffset = Vector3.zero;

    [SerializeField] private MeleeHitbox _meleeHitbox;

    [SerializeField] private AudioClip _movementSoundLoop;
    private GameObject _movementSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _navMeshAgent.updatePosition = true; // Laisser le NavMeshAgent g�rer la position
        _navMeshAgent.updateRotation = true; // Laisser le NavMeshAgent g�rer la rotation

        try
        {
            _movementSource = AudioManager.Instance.SetLoopAudioObject(_movementSoundLoop, 0);
            _movementSource.transform.position = _damagedEffectTransform.position;
            _movementSource.transform.SetParent(transform);
        }
        catch
        {
            Debug.Log("No audio object available");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerTargetPosition == null || _isDead || _navMeshAgent.enabled == false) return;

        _attackSpawnPoint.LookAt(_projectileTargetPosition);

        // Update animator movement parameter
        _animator.SetBool("IsMoving", _navMeshAgent.velocity.magnitude > 0.1f);

        if (!_isDestinationSet)
        {
            GetNextDestination();
        }
        else
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.pathPending && !_startedCoroutine)
            {
                _startedCoroutine = true;
                StartCoroutine(CheckDistanceWithPlayer());
            }
        }
    }


    private void GetNextDestination()
    {
        // Create a mask that excludes the SpawnCorridor area
        int avoidCorridorMask = NavMesh.AllAreas & ~(1 << NavMesh.GetAreaFromName("Undesirable"));

        while (!_isDestinationSet)
        {
            Vector3 randomPoint = transform.position + (Random.insideUnitSphere * _movementRadiusMax);
            NavMeshHit hit;

            // Use the custom mask here to avoid SpawnCorridor area
            if (NavMesh.SamplePosition(randomPoint, out hit, _movementRadiusMax, avoidCorridorMask))
            {
                _navMeshAgent.SetDestination(hit.position);
                _isDestinationSet = true;

                _movementSource.GetComponent<AudioSource>().volume = 1f;
            }
        }
    }


    //Called trough animation
    public void RangedAttack()
    {        
        GameObject _attack = Instantiate(_rangedAttack);
        _attack.GetComponent<PlagueDoctorAttackRanged>().CastSpell(_attackSpawnPoint.position, _attackSpawnPoint.rotation);
        _attack.GetComponent<PlagueDoctorAttackRanged>().SetSourceGameObject(_parryTarget);

        Debug.Log("Ranged Attack");
    }

    public override void StunEnnemy(int _damage)
    {
        base.StunEnnemy(_damage);
        _animator.SetTrigger("Blocked");
    }

    IEnumerator CheckDistanceWithPlayer()
    {
        // Stop early if dead
        if (_isDead) yield break;

        /*
         * J'ai enlev� cette section, puisqu'elle n'a pas �t� adapt� avec le nouvel asset d'ennemie. 
         * Dommage de perdre l'id�e, mais on a d'autres priorit�s
         * Gabriel Porlier
          
        if (Vector3.Distance(transform.position, _playerTargetPosition.position) <= _maxDistanceForMelee)
        {
            transform.LookAt(_projectileTargetPosition);
            yield return new WaitForSeconds(0.3f);
            if (_isDead) yield break; // Check again before triggering animation
            _animator.SetTrigger("MeleeAttack");
        } ...        
        */

        _movementSource.GetComponent<AudioSource>().volume = 0f;

        if (Random.value < _chanceForRangedAttack)
        {
            yield return new WaitForSeconds(0.3f);
            if (_isDead) yield break; // Check again
            transform.LookAt(_projectileTargetPosition);
            _animator.SetTrigger("RangedAttack");
        }

        yield return new WaitForSeconds(_waitTimeBetweenMovements);
        _isDestinationSet = false;
        _startedCoroutine = false;
    }


    private bool _isDead = false;

    [ContextMenu("Kill Ennemy")]
    protected override void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log("GroundedEnnemy Die called");

        try
        {
            _movementSource.GetComponent<AudioSource>().volume = 1f;
            _movementSource.transform.SetParent(null);
            AudioManager.Instance.StopAudioObject(_movementSource);
        }
        catch
        {
            Debug.Log("No audio object found");
        }
        

        try
        {
            // Stop any running coroutines to prevent animation triggers after death
            StopAllCoroutines();

            _navMeshAgent.enabled = false;

            // Reset animation triggers and flags
            _animator.SetBool("IsMoving", false);
            _animator.ResetTrigger("MeleeAttack");
            _animator.ResetTrigger("RangedAttack");
            _animator.ResetTrigger("Blocked");

            _animator.SetTrigger("TriggerDeath");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception in Die: " + e.Message);
        }
    }

    public void DestroyAfterDeathAnimation()
    {
        if (_deathVFX != null)
        {
            Vector3 spawnPosition = transform.position + transform.TransformVector(_deathVFXOffset);

            GameObject vfxInstance = Instantiate(_deathVFX, spawnPosition, transform.rotation);
            vfxInstance.transform.Rotate(-90f, 0f, 0f);
        }

        Destroy(gameObject);
    }


    public void ActivateMeleeHitbox()
    {
        _meleeHitbox.EnableHitbox();
    }

    public void DeactivateMeleeHitbox()
    {
        _meleeHitbox.DisableHitbox();
    }

}
