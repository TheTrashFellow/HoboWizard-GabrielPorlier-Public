using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * */
public class GrimmoireMenu : MonoBehaviour
{
    [Header("Player informations")]
    [SerializeField] private Player _player;
    [SerializeField] private Transform _playerContainer;

    [Space]
    [SerializeField] private Spell_Library _library;

    [Space]
    [Header("Pages and cards (Important d'etre en ordre !")]
    [SerializeField] private List<GameObject> _panels;
    [SerializeField] private List<GameObject> _cardPlacements;

    private int _activePanel = 0;

    private List<GameObject> _cards = new();

    private void Start()
    {        

        for(int i =0; i < _panels.Count; i++)
        {
            if(i == _activePanel)
            {
                _panels[i].SetActive(true);
            }
            else
            {
                _panels[i].SetActive(false);
            }
        }

        transform.SetParent(null, true);

        foreach(EquippedSpell spell in _player._equippedSpells)
        {
            SpellData data = _library.GetSpellById(spell.idSpell);
            GameObject spellCard = data.spellCardPrefab;           
            _cards.Add(Instantiate(spellCard));

        }
        PlaceCardsInGrimoire();
    }

    public void AddSpellCard(int spellId)
    {        
        SpellData data = _library.GetSpellById(spellId);
        GameObject spellCard = data.spellCardPrefab;
        _cards.Add(Instantiate(spellCard));

        PlaceCardsInGrimoire();
    }

    public void RemoveSpellCard(int spellId)
    {
        foreach(GameObject card in _cards)
        {
            if(card.GetComponent<SpellCard>()._spellId == spellId)
            {
                _cards.Remove(card);
                Destroy(card);                
                PlaceCardsInGrimoire(); 
                return;
            }
        }
        
    }

    private void PlaceCardsInGrimoire()
    {
        for(int i = 0; i < _cards.Count; i++)
        {
            _cards[i].transform.SetParent(_cardPlacements[i].transform, false);

            RectTransform cardTransform = _cards[i].GetComponent<RectTransform>();

            cardTransform.anchoredPosition = Vector3.zero;
            cardTransform.localRotation = Quaternion.identity;
            cardTransform.localScale = Vector3.one;

            //Pour Reset position Z aussi d'un element UI :
            Vector3 zeroPositionZ = cardTransform.localPosition;
            zeroPositionZ.z = 0f;
            cardTransform.localPosition = zeroPositionZ;
        }
    }

    public void GoToNextPage()
    {
        _panels[_activePanel].SetActive(false);
        _activePanel++;
        _panels[_activePanel].SetActive(true);        
    }

    public void GoToPreviousPage()
    {
        _panels[_activePanel].SetActive(false);
        _activePanel--;
        _panels[_activePanel].SetActive(true);
    }

}
