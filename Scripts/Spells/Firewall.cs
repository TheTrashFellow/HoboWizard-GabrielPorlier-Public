using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * */
public class Firewall : BaseSpell
{
    [SerializeField] private Transform _parentContainer;
    private List<Ennemy> _activeEnnemiesAffected = new List<Ennemy>();

    [SerializeField] private AudioClip _firewallSFX;

    public override void CastSpell(Vector3 spawnPosition, Quaternion spawnDirection, int spellId, int spellLevel)
    {
        base.CastSpell(spawnPosition, spawnDirection, spellId, spellLevel);
        transform.SetPositionAndRotation(spawnPosition, spawnDirection);

        //Rigidbody rb = GetComponent<Rigidbody>();

        //rb.linearVelocity = transform.forward * _launchSpeed;

        AudioManager.Instance.PlayAudioOneTime(_firewallSFX, 1f, transform);

        StartCoroutine(SpellExpires());
        StartCoroutine(DamageOverTime());
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
       
        if (other.gameObject.CompareTag("Ennemy"))
        {
            other.gameObject.GetComponentInParent<Ennemy>().TakeDamage(_damage);
            if(other != null)
                _activeEnnemiesAffected.Add(other.gameObject.GetComponentInParent<Ennemy>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ennemy"))
        {
            _activeEnnemiesAffected.Remove(other.gameObject.GetComponentInParent<Ennemy>());            
        }
    }        

    IEnumerator DamageOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(_tickInterval);

            foreach(Ennemy ennemy in _activeEnnemiesAffected)
            {
                ennemy.TakeDamage(_damage);
            }
        }
    }
}
