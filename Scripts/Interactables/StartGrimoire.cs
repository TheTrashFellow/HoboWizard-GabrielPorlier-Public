using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * */

[System.Serializable]
public class AttributePanel
{
    public string AttributeName;
    public GameObject Panel;
}


public class StartGrimoire : MonoBehaviour
{
    [Header("General references")]
    [SerializeField] private Spell_Library _library;
    private Player _player;

    [Header("UI Section")]   
    [SerializeField] private GameObject _grimoire;
    [SerializeField] private Button _exitButton;

    [Space]
    [Header("Bookmark buttons")]
    [SerializeField] private Button _fireSpellsButton;
    [SerializeField] private Button _windSpellsButton;

    [Space]
    [Header("Attribute Panels")]
    [SerializeField] private List<AttributePanel> _panels;

    [Space]
    [Header("SpellCards")]
    [SerializeField] private List<GameObject> _cards;

    [Space]
    [Header("Identifiant nombre de sorts")]
    [SerializeField] private TextMeshProUGUI _identifierAmountSpells;

    [Space]
    [Header("Animator UI")]
    [SerializeField] private Animator _uiAnimator;

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioClip _openBook;
    [SerializeField] private AudioClip _activeInteractionSound;
    [SerializeField] private AudioClip _closeInteractionSound;
    [SerializeField] private AudioClip _spellSelected;
    [SerializeField] private AudioClip _errorSound;
    [SerializeField] private float _audioVolume;    

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
                
        foreach(EquippedSpell spell in _player._equippedSpells)
        {
            SpellData spellData = _library.GetSpellById(spell.idSpell);
            string spellName = spellData.spellName;
            foreach(GameObject card in _cards)
            {
                if(card.name == spellName)
                {
                    card.GetComponent<SpellCard>().SetSelectedPin();
                }
            }  
            
        }
        SetIdentifierAmountSpells();
    }


    #region XRInteracting
    private XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Try casting to get the actual interactor and its manager
        if (args.interactorObject is XRBaseInteractor interactor)
        {
            var manager = interactor.interactionManager;
            if (manager != null)
            {
                manager.SelectExit((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable);
            }
        }
    }
    #endregion

#region UI_Managing
    public void ToggleGrimoire()
    {
        if (!_grimoire.activeSelf)
        {
            _grimoire.SetActive(true);
            AudioManager.Instance.PlayAudioOneTime(_openBook, 0.7f, transform);
        }
        else
        {
            _grimoire.SetActive(false);
            AudioManager.Instance.PlayAudioOneTime(_closeInteractionSound, _audioVolume, transform);
        }
            
    }

    //String du boutton et texte associé au panel doivent être le même 
    public void ChangePanel()
    {
        GameObject buttonActual = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (buttonActual == null) return;
                     
        string buttonText = buttonActual.GetComponentInChildren<TextMeshProUGUI>()?.text;
        
        foreach (AttributePanel panel in _panels)
        {
            if(panel.AttributeName == buttonText)
            {
                panel.Panel.SetActive(true);
                
            }
            else
            {
                panel.Panel.SetActive(false);
            }
        }

        AudioManager.Instance.PlayAudioOneTime(_activeInteractionSound, _audioVolume, transform);
    }

    public void SpellCardSelected()
    {
        GameObject buttonActual = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (buttonActual == null) return;
        string buttonName = buttonActual.name;
        Debug.Log(buttonName);

        SpellData spell = _library.GetSpellByName(buttonName);

        Debug.Log(spell.idSpell);

        bool _spellFoundInEquippedSpells = false;

        foreach (EquippedSpell equippedSpell in _player._equippedSpells)
        {
            SpellData spellData = _library.GetSpellById(equippedSpell.idSpell);
            
            if(spell == spellData)
            {
                _player.UnequipSpell(spell.idSpell);                
                _spellFoundInEquippedSpells = true;
                Debug.Log("Unequiping");
                AudioManager.Instance.PlayAudioOneTime(_closeInteractionSound, _audioVolume, transform);

                buttonActual.GetComponent<SpellCard>().UnsetSelectedPin();
                break;
            }
        }

        if (!_spellFoundInEquippedSpells)
        {
            if(_player._equippedSpells.Count < _player._startingEquippedSpells)
            {                
                _player.EquipSpell(spell.idSpell, 1);
                buttonActual.GetComponent<SpellCard>().SetSelectedPin();
                AudioManager.Instance.PlayAudioOneTime(_spellSelected, _audioVolume, transform);
            }
            else
            {
                _uiAnimator.SetTrigger("LimitReached");
                AudioManager.Instance.PlayAudioOneTime(_errorSound, _audioVolume, transform);
            }
        }
       

        SetIdentifierAmountSpells();
    }
   
    private void SetIdentifierAmountSpells()
    {
        _identifierAmountSpells.text = _player._equippedSpells.Count + "/" + _player._startingEquippedSpells;
    }

    public void UnequipAllSpells()
    {
        _player.UnequipAllSpells();

        foreach(GameObject card in _cards)
        {
            card.GetComponent<SpellCard>().UnsetSelectedPin();
        }

        AudioManager.Instance.PlayAudioOneTime(_closeInteractionSound, _audioVolume, transform);

        SetIdentifierAmountSpells();
    }
    
#endregion

}
