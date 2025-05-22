using UnityEngine;
using UnityEngine.AI;

/*
 * Code by Gabriel Porlier

   My honest attempt at using inheritance. Could have been used more by me and my colleagues.     
 */
public class Ennemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public int _healthPoints = 100; // Max HP
    [SerializeField] protected int _healthPointsCurrent;
    [SerializeField] public int _damage = 10;

    [Header("References")]
    [SerializeField] protected NavMeshAgent _navMeshAgent = default;
    [SerializeField] protected Transform _damagedEffectTransform;

    [Space]
    [Header("Condition")]
    [SerializeField] public bool _canBeMoved = true;

    protected Player _player;
    protected Transform _playerTargetPosition;
    protected Transform _projectileTargetPosition;    
    

    protected virtual void Awake()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        _playerTargetPosition = _player._target;
        _projectileTargetPosition = _player._projectileTarget;

        _navMeshAgent = GetComponent<NavMeshAgent>();

        _healthPointsCurrent = _healthPoints;
    }
    
    public virtual void TakeDamage(int damage)
    {
        if (_healthPointsCurrent <= 0) return;

        _healthPointsCurrent -= damage;
        PoolingVFXDamage.Instance.EnnemyDamagedVFX(_damagedEffectTransform);

        if (_healthPointsCurrent <= 0)
        {
            Die();
        }
    }

    
    protected virtual void Die()
    {        
        Destroy(gameObject);        
    }

    public virtual void StunEnnemy(int damage)
    {
        TakeDamage(damage);
    }

    public int GetCurrentHP() => _healthPointsCurrent;
    public int GetMaxHP() => _healthPoints;
}
