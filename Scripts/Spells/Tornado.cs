using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * */

public class Tornado : BaseSpell
{
    [SerializeField] private Transform _parentContainer;
    private List<Ennemy> _activeEnnemiesAffected = new List<Ennemy>();

    private float _rotationSpeed;

    private Coroutine _zoneCoroutine;

    [SerializeField] private AudioClip _tornadoSFX;

    private void Start()
    {
        _rotationSpeed = _launchSpeed * 10;
    }

    void Update()
    {
        //transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
    }

    public override void CastSpell(Vector3 spawnPosition, Quaternion spawnDirection, int spellId, int spellLevel)
    {
        base.CastSpell(spawnPosition, spawnDirection, spellId, spellLevel);
        _parentContainer.transform.SetPositionAndRotation(spawnPosition, spawnDirection);

        AudioManager.Instance.PlayAudioOneTime(_tornadoSFX, 3f, transform);

        StartCoroutine(SpellExpires());
        _zoneCoroutine = StartCoroutine(DamageOverTime());
    }

    protected override void GetSetSpellLevelData(int level)
    {
        SpellData spell = _library.GetSpellById(_spellId);

        _levelData = spell.spellLevelStats[level - 1];

        _damage = _levelData.damage;
        _duration = _levelData.duration;
        _tickInterval = _levelData.tickRate;

        _scale = _levelData.scale;
        _parentContainer.localScale = new(_scale, _scale, _scale);
    }

    protected override void OnTriggerEnter(Collider other)
    {        

        if (other.CompareTag("Ennemy"))
        {
            Ennemy ennemy = other.GetComponentInParent<Ennemy>();

            if (ennemy != null && !_activeEnnemiesAffected.Contains(ennemy))
            {
                _activeEnnemiesAffected.Add(ennemy);

                Rigidbody enemyRb = ennemy.GetComponent<Rigidbody>();
                if (ennemy._canBeMoved)
                {
                    enemyRb.isKinematic = false;
                    //ennemy.GetComponent<Ennemy>().enabled = false;
                    ennemy.GetComponent<Collider>().enabled = true;
                    //ennemy.GetComponent<Rigidbody>().useGravity = true;
                    other.GetComponentInParent<NavMeshAgent>().enabled = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ennemy"))
        {
            Ennemy ennemy = other.GetComponentInParent<Ennemy>();
            if (ennemy != null && _activeEnnemiesAffected.Contains(ennemy))
            {
                _activeEnnemiesAffected.Remove(ennemy);

                if (ennemy._canBeMoved)
                {
                    StartCoroutine(EnnemyOut(ennemy));
                }                
            }
        }
    }

    IEnumerator DamageOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(_tickInterval);

            foreach (Ennemy ennemy in _activeEnnemiesAffected)
            {
                ennemy.TakeDamage(_damage);

                if (ennemy == null || !ennemy._canBeMoved) continue;

                Rigidbody rb = ennemy.GetComponent<Rigidbody>();

                Vector3 adjustedUp = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                Vector3 direction = (adjustedUp - rb.transform.position).normalized;
                rb.AddForce(direction * 50, ForceMode.Impulse);
            }
        }
    }

    protected override IEnumerator SpellExpires()
    {
        yield return new WaitForSeconds(_duration);

        StopCoroutine(_zoneCoroutine);
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(2);

        foreach (Ennemy ennemy in _activeEnnemiesAffected)
        {
            if (ennemy != null && ennemy._canBeMoved)
            {
                try
                {
                    //ennemy.GetComponent<Rigidbody>().isKinematic = true;
                    ennemy.GetComponent<NavMeshAgent>().enabled = true;
                    ennemy.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                    //ennemy.GetComponent<Collider>().enabled = false;
                    //ennemy.GetComponent<Rigidbody>().useGravity = false;
                    ennemy.GetComponent<Ennemy>().enabled = true;
                }
                catch { }
                
            }
        }

        Destroy(_parentContainer.gameObject);
    }

    IEnumerator EnnemyOut(Ennemy ennemy)
    {
        yield return new WaitForSeconds(2);

        if (ennemy != null && !_activeEnnemiesAffected.Contains(ennemy))
        {
            //ennemy.GetComponent<Rigidbody>().isKinematic = true;
            ennemy.GetComponent<NavMeshAgent>().enabled = true;
            ennemy.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            //ennemy.GetComponent<Collider>().enabled = false;
            //ennemy.GetComponent<Rigidbody>().useGravity = false;
            ennemy.GetComponent<Ennemy>().enabled = true;
        }
    }
}
