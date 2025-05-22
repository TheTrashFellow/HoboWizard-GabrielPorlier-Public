using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Code by Gabriel Porlier

    Spawns small or regular slimes depending on conditions. Stays active a few seconds before being destroyed so it can enable the ennemies after the spawning,
    ragdoll effect. 
 * */
public class BossSlimeAttack : MonoBehaviour
{    
    [Space]
    [Header("Spawn prefabs")]    
    [SerializeField] private GameObject _bossRegularSlime;
    [SerializeReference] private Transform _regularSpawn;


    [Space]
    [SerializeField] private GameObject _bossSmallSlime;
    [SerializeField] private Transform _smallSlimeSpawn1;
    [SerializeField] private Transform _smallSlimeSpawn2;

    private GameObject _slime1;
    private GameObject _slime2;

    private int _damage;

    private Transform _centerPoint;

    private bool _parried = false;

    private void Update()
    {
        
    }  

    public void SetStats(int damage, Transform centerPoint)
    {
        _damage = damage;
        _centerPoint = centerPoint;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(_damage);

            SpawnSmallSlimes();

            GetComponent<Collider>().enabled = false; GetComponent<MeshRenderer>().enabled = false;
            return;
        }

        if (other.gameObject.CompareTag("Shield"))
        {
            _parried = other.GetComponentInParent<ShieldManager>().BlockedAttack();
            if (!_parried)
            {
                SpawnSmallSlimes();
                GetComponent<Collider>().enabled = false; GetComponent<MeshRenderer>().enabled = false;
                return;
            }
            else
            {
                Destroy(gameObject);                
                return;
            }
        }

        if(other.gameObject.CompareTag("Ennemy") || other.gameObject.CompareTag("Spell") || other.gameObject.CompareTag("Consummable"))
        {
            return;
        }


        SpawnRegularSlime();
    }

    private void SpawnRegularSlime()
    {
        GetComponent<Collider>().enabled = false;
        //gameObject.GetComponent<Collider>().enabled = false;
        GameObject slime = Instantiate(_bossRegularSlime, _regularSpawn.position, _regularSpawn.rotation);
        
        
        //slime.transform.position = _regularSpawn.position;        
        //slime.GetComponent<BossRegularSlime>().enabled = false;
        //slime.GetComponent<NavMeshAgent>().enabled = false;
        //slime.GetComponent<Collider>().enabled = true;
        StartCoroutine(DelayedDestroy(slime));
    }

    private void SpawnSmallSlimes()
    {
        GetComponent<Collider>().enabled = false;
        _slime1 = Instantiate(_bossSmallSlime); _slime2 = Instantiate(_bossSmallSlime);

        _slime1.GetComponent<BossSmallSlime>().enabled = false; _slime1.GetComponent<NavMeshAgent>().enabled = false; _slime1.GetComponent<Collider>().enabled = true;
        _slime2.GetComponent<BossSmallSlime>().enabled = false; _slime2.GetComponent<NavMeshAgent>().enabled = false; _slime2.GetComponent<Collider>().enabled = true;

        _slime1.transform.SetPositionAndRotation(_smallSlimeSpawn1.position, _smallSlimeSpawn1.rotation);
        _slime2.transform.SetPositionAndRotation(_smallSlimeSpawn2.position, _smallSlimeSpawn2.rotation);

        _slime1.GetComponent<Rigidbody>().AddForce(_slime1.transform.forward * 30, ForceMode.Impulse);
        _slime2.GetComponent<Rigidbody>().AddForce(_slime2.transform.forward * 30, ForceMode.Impulse);

        StartCoroutine(DelayedDestroy());
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(3);

        if(_slime1 != null)
        {
            _slime1.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);
            _slime1.transform.rotation = Quaternion.identity;            
            _slime1.GetComponent<NavMeshAgent>().enabled = true; 
            _slime1.GetComponent<BossSmallSlime>().enabled = true; 
            _slime1.GetComponent<Collider>().enabled = false;
            _slime1.GetComponent<Rigidbody>().useGravity = false;

            _slime1.GetComponent<BossSmallSlime>().SetDestination(_centerPoint);
        }
        
        if(_slime2 != null)
        {
            _slime2.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);
            _slime2.transform.rotation = Quaternion.identity;
            _slime2.GetComponent<NavMeshAgent>().enabled = true; 
            _slime2.GetComponent<BossSmallSlime>().enabled = true; 
            _slime2.GetComponent<Collider>().enabled = false;
            _slime2.GetComponent<Rigidbody>().useGravity = false;

            _slime2.GetComponent<BossSmallSlime>().SetDestination(_centerPoint);
        }        

        Destroy(gameObject);
    }

    IEnumerator DelayedDestroy(GameObject slime)
    {
        yield return new WaitForSeconds(2);

        slime.GetComponent<BossRegularSlime>().SetDestination(_centerPoint);

        /*
        if (slime != null)
        {
            slime.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);
            slime.transform.rotation = Quaternion.identity;
            slime.GetComponent<BossRegularSlime>().enabled = true;
            slime.GetComponent<NavMeshAgent>().enabled = true;
            slime.GetComponent<BossRegularSlime>().SetDestination(_centerPoint);
        }*/

    }
}
