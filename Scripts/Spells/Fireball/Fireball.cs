using System.Collections;

using UnityEngine;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * */
public class Fireball : BaseSpell
{
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private Transform _parentContainer;

    [SerializeField] private AudioClip _fireballSFX;
    [SerializeField] private AudioClip _explosionSFX;

    public override void CastSpell(Vector3 spawnPosition, Quaternion spawnDirection, int spellId, int spellLevel)
    {
        base.CastSpell(spawnPosition, spawnDirection, spellId, spellLevel);

        transform.SetPositionAndRotation(spawnPosition, spawnDirection);

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * _launchSpeed;

        AudioManager.Instance.PlayAudioOneTime(_fireballSFX, 1f, transform);

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

        if(other.gameObject.CompareTag("EnnemyAttack"))
        {
            Destroy(other.gameObject);
            return;
        }

        if(other.gameObject.CompareTag("Consummable") || other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Wand") || other.gameObject.CompareTag("Shield") || other.gameObject.CompareTag("Spell"))
        {
            return;
        }

        if(other.gameObject.CompareTag("Ennemy"))
        {
            other.GetComponentInParent<Ennemy>().TakeDamage(_damage);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0,0,0);

        //GetComponentInChildren<Animator>().enabled = true;
        //GetComponentInChildren<Animator>().SetTrigger("Explosion");

        ExplosionEnd();
    }

    public void ExplosionEnd()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, transform.rotation);
        explosion.GetComponent<Explosion>().SetDamage(_damage / 2);
        explosion.transform.SetParent(null);
        explosion.transform.localScale = new(_scale + 2.5f, _scale + 2.5f, _scale + 2.5f);


        AudioManager.Instance.PlayAudioOneTime(_explosionSFX, 1f, transform);

        
        Destroy(_parentContainer.gameObject);
    }
}
