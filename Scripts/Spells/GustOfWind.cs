using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 */

public class GustOfWind : BaseSpell
{
    [SerializeField] private float _blastForce = 20;
    [SerializeField] private Transform _parentContainer;
    private List<Ennemy> _activeEnnemiesAffected = new List<Ennemy>();

    [SerializeField] private AudioClip _gustOfWindSFX;

    public override void CastSpell(Vector3 spawnPosition, Quaternion spawnDirection, int spellId, int spellLevel)
    {
        base.CastSpell(spawnPosition, spawnDirection, spellId, spellLevel);
        transform.SetPositionAndRotation(spawnPosition, spawnDirection);

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * _launchSpeed;

        AudioManager.Instance.PlayAudioOneTime(_gustOfWindSFX, 1f, transform);

        StartCoroutine(SpellExpires());
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
        base.OnTriggerEnter(other);

        if (_shouldNotDetectContact)
        {
            _shouldNotDetectContact = false;
            return;
        }

        if(other.gameObject.CompareTag("Obstacle"))
        {

        }

        if (other.gameObject.CompareTag("Ennemy"))
        {
            Ennemy ennemy = other.GetComponentInParent<Ennemy>();

            if (_activeEnnemiesAffected.Contains(ennemy)) return;

            ennemy.GetComponentInParent<Ennemy>().TakeDamage(_damage);

            if (ennemy == null || !ennemy._canBeMoved ) return;

            _activeEnnemiesAffected.Add(ennemy);
            

            Rigidbody enemyRb = ennemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = false;
                //ennemy.GetComponent<Ennemy>().enabled = false;
                ennemy.GetComponentInChildren<Collider>().enabled = false;
                ennemy.GetComponent<Collider>().enabled = true;
                
                ennemy.GetComponent<Rigidbody>().useGravity = true;
                other.GetComponentInParent<NavMeshAgent>().enabled = false;

                Vector3 direction = (other.transform.position - transform.position).normalized;
                direction.y = 0.50f; 
                float forceMagnitude = _blastForce;
                enemyRb.AddForce(direction * forceMagnitude, ForceMode.Impulse);
            }
                        
        }
        
        
    }


    protected override IEnumerator SpellExpires()
    {
        yield return new WaitForSeconds(_duration);

        foreach(Ennemy ennemy in _activeEnnemiesAffected)
        {
            if(ennemy != null)
            {
                try
                {
                    ennemy.GetComponent<Rigidbody>().isKinematic = true;
                    ennemy.GetComponent<NavMeshAgent>().enabled = true;
                    ennemy.GetComponent<Collider>().enabled = false;
                    ennemy.GetComponentInChildren<Collider>().enabled = true;
                    ennemy.GetComponent<Rigidbody>().useGravity = false;
                    ennemy.GetComponent<Ennemy>().enabled = true;
                }
                catch { }
                
            }            
        }

        Destroy(gameObject);
    }
}
