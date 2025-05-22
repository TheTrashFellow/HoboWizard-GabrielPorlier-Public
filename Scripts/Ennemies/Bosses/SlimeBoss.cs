using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Code by Gabriel Porlier

    Developed and integrated within it's boss arena prefab by me.

    Fixes from final animation integration from my colleague, by me. (True for attacks and adds of the boss as well)
 */
public class SlimeBoss : Ennemy
{
    [Header("Projectiles Prefabs")]
    [SerializeField] private GameObject _slimeProjectile = default;
    [SerializeField] private GameObject _rubbleProjectile = default;
    [SerializeField] private GameObject _bigRubbleProjectile = default;

    [Space]
    [Header("For placement and movements")]
    [SerializeField] private Transform _centerPoint;
    [SerializeField] private float _rotationSpeed = 60f;
    [SerializeField] private bool _isInLevel = true;

    [Space]
    [Header("Health Bar UI")]
    [SerializeField] private Transform _healthBarReference;

    [Space]
    [Header("For projectile")]
    [SerializeField] private float _launchAngle = 45f;
    [SerializeField] private Transform _launchPoint = default;
    [SerializeField] private int _amountBeforeBigRubble = 3;
    [SerializeField] private Transform _bossCenter;
    [SerializeField] private float _timeBetweenAttacks = 5f;

    [Space]
    [Header("For Separation Event")]
    [SerializeField] private GameObject _bossCore = default;
    [SerializeField] private int _coresAmount = 4;
    [SerializeField] private float _radiusCoreSpawn = 10f;
    [SerializeField] private Transform[] _slimeSpawnPoint;

    private int _attackCounter = 0;
    [SerializeField] private Animator _animator = default;
    private bool _readyToAttack = true;
    private GameObject[] _cores = new GameObject[4];
    private Coroutine _bigCooldown;

    [Space]
    [Header("Boss SFXs")]
    [SerializeField] private AudioClip _bossSpawnSFX;
    [SerializeField] private AudioClip _bossAttackSFX;

    protected override void Awake()
    {
        base.Awake(); // Ensures health is initialized

        for (int i = 0; i < _coresAmount; i++)
            _cores[i] = null;

        AudioManager.Instance.PlayAudioOneTime(_bossSpawnSFX, 1f, transform);
    }

    void Start()
    {
        if (_isInLevel)
        {
            _centerPoint = GameObject.Find("BossSpawnPoint")?.transform;

            if (_centerPoint == null)
                Debug.LogError("BossSpawnPoint not found in the scene.");
        }

        SetHpBar();
        InvokeRepeating(nameof(StartAttackAnimation), 3f, _timeBetweenAttacks);
    }

    void Update()
    {
        if (_centerPoint != null)
        {
            Vector3 direction = _playerTargetPosition.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private void StartAttackAnimation()
    {
        if (_readyToAttack)
            _animator.SetTrigger("Attack");
    }

    public void AttackPlayer()
    {
        AudioManager.Instance.PlayAudioOneTime(_bossAttackSFX, 1f, _launchPoint);

        GameObject projectile = null;
        _attackCounter++;

        if (_attackCounter == _amountBeforeBigRubble)
        {
            _attackCounter = 0;
            projectile = Instantiate(_bigRubbleProjectile, _launchPoint.position, Quaternion.identity);
            projectile.GetComponent<RubbleAttackBig>().SetStats(_damage * 2, _bossCenter);
            _readyToAttack = false;
            _bigCooldown = StartCoroutine(BigCooldown());
        }
        else
        {
            int damage = (int)Math.Ceiling(_healthPointsCurrent * 0.05f);

            if (UnityEngine.Random.Range(1, 3) == 1 || damage >= _healthPointsCurrent)
            {
                projectile = Instantiate(_slimeProjectile, _launchPoint.position, Quaternion.identity);
                projectile.GetComponent<BossSlimeAttack>().SetStats(_damage * 2, _centerPoint);
                TakeDamage(damage); // Self damage
            }
            else
            {
                projectile = Instantiate(_rubbleProjectile, _launchPoint.position, Quaternion.identity);
                projectile.GetComponent<RubbleAttackRegular>().SetStats(_damage, _bossCenter);
            }
        }

        ShootProjectile(projectile, _projectileTargetPosition.position);
    }

    private void ShootProjectile(GameObject projectile, Vector3 target)
    {
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        Vector3 dir = target - _launchPoint.position;
        float h = dir.y;
        dir.y = 0;
        float distance = dir.magnitude;
        float angle = _launchAngle * Mathf.Deg2Rad;

        float g = Physics.gravity.y;
        float v2 = (g * distance * distance) / (2 * (h - distance * Mathf.Tan(angle)) * Mathf.Pow(Mathf.Cos(angle), 2));

        if (v2 <= 0)
        {
            Debug.LogWarning("Target too close or too far for given angle.");
            return;
        }

        float velocity = Mathf.Sqrt(v2);
        Vector3 velocityVec = dir.normalized * velocity * Mathf.Cos(angle);
        velocityVec.y = velocity * Mathf.Sin(angle);

        rb.linearVelocity = velocityVec;
    }

    public void HealBack(float percentage)
    {
        int amount = (int)Math.Ceiling(_healthPoints * percentage);
        _healthPointsCurrent += amount;

        if (_healthPointsCurrent > _healthPoints)
            _healthPointsCurrent = _healthPoints;

        SetHpBar();
    }

    protected override void Die()
    {
        Debug.Log("Boss Die called");

        try
        {
            _animator.SetTrigger("TriggerDeath");
        }
        catch (Exception e)
        {
            Debug.LogError("Exception in Die: " + e.Message);
        }
    }

    public void DestroyAfterDeathAnimation()
    {
        Destroy(gameObject);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage); // Decrements _healthPointsCurrent
        SetHpBar();
    }

    public override void StunEnnemy(int damage)
    {
        int percentageDamage = (int)Math.Ceiling(0.15 * _healthPointsCurrent);

        if (percentageDamage >= _healthPointsCurrent)
        {
            Die();
        }
        else
        {
            TakeDamage(percentageDamage + 10);
            SeparateBoss();
        }
    }

    public void SeparateBoss()
    {
        for (int i = 0; i < _coresAmount; i++)
        {
            _cores[i] = Instantiate(_bossCore, _slimeSpawnPoint[i].position, _slimeSpawnPoint[i].rotation);
            //_cores[i].GetComponent<BossRegularSlime>().enabled = false;
            //_cores[i].GetComponent<NavMeshAgent>().enabled = false;
            //_cores[i].GetComponent<Collider>().enabled = true;
            _cores[i].GetComponent<Rigidbody>().isKinematic = false;
            _cores[i].GetComponent<Rigidbody>().AddForce(_cores[i].transform.forward * 150, ForceMode.Impulse);
            
        }

        StartCoroutine(CoreActivation());
    }

    private Vector3 RandomTargetInRoom()
    {
        while (true)
        {
            Vector3 randomPoint = transform.position + (UnityEngine.Random.insideUnitSphere * _radiusCoreSpawn);
            if (NavMesh.SamplePosition(randomPoint, out var hit, _radiusCoreSpawn, NavMesh.AllAreas))
            {
                if (Vector3.Distance(hit.position, _centerPoint.position) > 3)
                    return hit.position;
            }
        }
    }

    private void SetHpBar()
    {
        float scaleValue = Mathf.Clamp((float)_healthPointsCurrent / _healthPoints, 0f, 1f);
        
        _healthBarReference.localScale = new Vector3(scaleValue, 1, 1);
    }

    IEnumerator BigCooldown()
    {
        yield return new WaitForSeconds(_timeBetweenAttacks);
        _readyToAttack = true;
    }

    IEnumerator CoreActivation()
    {
        yield return new WaitForSeconds(3);

        for (int i = 0; i < _coresAmount; i++)
        {
            try
            {

                _cores[i].GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                _cores[i].GetComponent<Rigidbody>().isKinematic = true;
                _cores[i].transform.rotation = Quaternion.identity;
                // _cores[i].GetComponent<BossRegularSlime>().enabled = true;
                // _cores[i].GetComponent<NavMeshAgent>().enabled = true;
                _cores[i].GetComponent<BossRegularSlime>().SetDestination(_centerPoint);
            }
            catch { }
        }
    }
}
