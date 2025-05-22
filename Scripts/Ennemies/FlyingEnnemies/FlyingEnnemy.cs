using UnityEngine;
using UnityEngine.AI;

/*
 * Code by Gabriel Porlier
 * */
public class FlyingEnnemy : Ennemy
{
    [SerializeField] private float _desiredDistance = default;
    [SerializeField] private GameObject _projectile = default;
    [SerializeField] private GameObject _parryTarget = default;

    [SerializeField] private Transform _projectileSpawnPoint = default;
    [SerializeField] private float _attackFrequency = default;

    [SerializeField] private Animator _animator = default;

    [SerializeField] private GameObject _deathVFX = default;
    [SerializeField] private Vector3 _deathVFXOffset = Vector3.zero;

    private Vector3 _lastKnownPlayerPosition = default;
    private Vector3 _currentDestination = default;

    [SerializeField] private AudioClip _wingBeatAudio;
    private GameObject _wingBeatObject;

    void Start()
    {
        _navMeshAgent.updatePosition = true; // Laisser le NavMeshAgent g�rer la position
        //_navMeshAgent.updateRotation = true; // Laisser le NavMeshAgent g�rer la rotation 
        _lastKnownPlayerPosition = _playerTargetPosition.position;

        int delay = Random.Range(3, 8);

        try
        {
            _wingBeatObject = AudioManager.Instance.SetLoopAudioObject(_wingBeatAudio, 1f);
            _wingBeatObject.transform.position = _damagedEffectTransform.position;
            _wingBeatObject.transform.SetParent(transform);
        }
        catch
        {
            Debug.Log("No audio object available");
        }
        

        InvokeRepeating(nameof(Attack), delay, _attackFrequency);
    }

    void Update()
    {
        if (_playerTargetPosition == null || !_navMeshAgent.enabled) return;

        transform.LookAt(_projectileTargetPosition);

        if ((_lastKnownPlayerPosition != _playerTargetPosition.position))
        {
            SetDestination();
        }      
        _lastKnownPlayerPosition = _playerTargetPosition.position;

    }    

    private void SetDestination()
    {
        Debug.Log("In SetDestination");

        float distanceToTarget = Vector3.Distance(_navMeshAgent.transform.position, _player.transform.position);
        Vector3 directionAway = (_navMeshAgent.transform.position - _player.transform.position).normalized;
        Vector3 rawNewPosition = _navMeshAgent.transform.position + directionAway * (_desiredDistance - distanceToTarget);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawNewPosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();
            if (_navMeshAgent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                _navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                Debug.Log("Path to new position is invalid.");
            }
        }
        else
        {
            Debug.Log("New position is not on the NavMesh.");
            //_navMeshAgent.SetDestination(_playerTargetPosition.position);
        }
    }

    private void Attack()
    {
        if (_navMeshAgent.enabled)
        {
            _animator.SetTrigger("TriggerAttack");

            GameObject thisProjectile = Instantiate(_projectile);
            thisProjectile.GetComponent<FlyingEnnemiesAttack>().CastSpell(_projectileSpawnPoint.position, _projectileSpawnPoint.rotation, _projectileTargetPosition);
            thisProjectile.GetComponent<FlyingEnnemiesAttack>().SetSourceGameObject(gameObject);
        }
    }

    // Context menu option should have been into Ennemy base script.
    [ContextMenu("Kill Ennemy")]
    protected override void Die()
    {
        try
        {
            _wingBeatObject.transform.SetParent(null);
            AudioManager.Instance.StopAudioObject(_wingBeatObject);
        }
        catch
        {
            Debug.Log("No object found");
        }
        

        Debug.Log("FlyingEnnemy Die called");
        _animator.SetTrigger("TriggerDeath");
        
    }

    /*
 * Code by Jonathan Gremmo
 * */
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

}
