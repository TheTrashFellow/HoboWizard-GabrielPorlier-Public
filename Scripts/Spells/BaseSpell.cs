using System.Collections;
using UnityEngine;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * */
public class BaseSpell : MonoBehaviour
{
    [Header("BaseSpell parameters")]    
    [SerializeField] protected string _spellName;
    [SerializeField] protected float _launchSpeed;
    [SerializeField] protected float _duration;
    [SerializeField] protected int _damage;
    [SerializeField] protected float _tickInterval;
    [SerializeField] protected float _scale;



    [Space]
    [Header("BaseSpell References")]
    [SerializeField] protected Spell_Library _library = default;
    [SerializeField] protected Object _parentGameobject;


    protected int _spellId;
    protected SpellLevelStats _levelData;
    protected bool _shouldNotDetectContact = false;

        

    public virtual void CastSpell(Vector3 spawnPosition, Quaternion spawnDirection, int spellId, int spellLevel)
    {
        _spellId = spellId;
        GetSetSpellLevelData(spellLevel);
    }

    protected virtual void GetSetSpellLevelData(int level)
    {        
        SpellData spell = _library.GetSpellById(_spellId);
        
        _levelData = spell.spellLevelStats[level-1];

        _damage = _levelData.damage;
        _duration = _levelData.duration;
        _tickInterval = _levelData.tickRate;
       
        _scale = _levelData.scale;        
        gameObject.transform.localScale = new(_scale, _scale, _scale);
        
    }

    protected virtual void OnTriggerEnter(Collider other)
    {        
        if(other.gameObject.tag == "Wand")
        {            
            _shouldNotDetectContact = true;
            return;
        }
        if (other.gameObject.CompareTag("Player"))
        {
            _shouldNotDetectContact = true;
            return;
        }
    }

    protected virtual IEnumerator SpellExpires()
    {
        yield return new WaitForSeconds(_duration);
        Destroy(gameObject);
        if(_parentGameobject)
        {
            Destroy(_parentGameobject);
        }
        
    }
}

[System.Serializable]
public class SpellLevelData
{
    public int Damage;
    public float Duration;
    public float TickRate;
    public float Scale;
}
