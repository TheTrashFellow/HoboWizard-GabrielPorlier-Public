using UnityEngine;
using System.Collections.Generic;

/*
 * Code by Gabriel Porlier
 * */

[CreateAssetMenu(fileName = "SpellLibrary", menuName = "CustomLibraries/Spell Library")]
public class Spell_Library : ScriptableObject
{
    [System.Serializable]
    public class SpellData
    {
        public int idSpell;        
        public string spellName;
        public string spellElement;
        //public Texture2D stencilTexture;
        public Color spellColor;
        public bool isCurvedRay;
        public GameObject reticle;
        public GameObject blockedReticle;
        public GameObject spellPrefab;
        public List<SpellLevelStats> spellLevelStats;
        public GameObject spellCardPrefab;
    }

    [System.Serializable]
    public class SpellLevelStats
    {
        public int damage;
        public float duration;
        public float tickRate;
        public float scale;

        public SpellLevelStats(SpellLevelStats spellLevelStats)
        {
            damage = spellLevelStats.damage;
            duration = spellLevelStats.duration;
            tickRate = spellLevelStats.tickRate;
            scale = spellLevelStats.scale;
        }

        
    }

    public SpellData GetSpellById(int idSpell)
    {
        
        foreach (SpellData spell in spells)
        {
            if (spell.idSpell == idSpell)
            {
                return spell;
            }
        }
        return null;
    }
    public SpellData GetSpellByName(string spellName)
    {

        foreach (SpellData spell in spells)
        {
            if (spell.spellName == spellName)
            {
                return spell;
            }
        }
        return null;
    }

    public GameObject GetSpellPrefab(int idSpell)
    {    
        foreach(SpellData spell in spells)
        {
            if(spell.idSpell == idSpell)
            {
                return spell.spellPrefab;
            }
        }

        return null;
    }

    public List<SpellData> spells = new List<SpellData>(); // Liste de toutes les recettes

}