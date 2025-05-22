using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.UI;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * */
 
public class Player : MonoBehaviour
{
    private const string KNOWN_SPELL_KEY = "StartingSpell";
    private const string HOLSTER_HANDENESS_KEY = "IsLeftHandedLayout";

    public int _playerHealth = 100;
    public int _playerGold = 50;

    [Header("Rays")]
    [SerializeField] public GameObject _rightCurvedRay;
    [SerializeField] public GameObject _leftCurvedRay;
    [Space]
    [SerializeField] public GameObject _rightStraightRay;
    [SerializeField] public GameObject _leftStraightRay;
    /*
    [Space]
    [Header("Controllers")]
    [SerializeField] public GameObject _interactionManager;
    [SerializeField] public GameObject _rightController;
    [SerializeField] public GameObject _leftController;
    */
    [Space]
    [Header("Target For NavMesh Agent")]
    [SerializeField] public Transform _target = default;
    [Header("Target For Projectiles")]
    [SerializeField] public Transform _projectileTarget = default;

    [Space]
    [Header("MainCamera")]
    [SerializeField] public Camera _mainCamera = default;

    [Space]
    [Header("Equipped Spells")]
    [SerializeField] public List<EquippedSpell> _equippedSpells;
    [SerializeField] public int _startingEquippedSpells = 1;

    [Space]
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverUI;

    [Space]
    [Header("Transition UI")]
    [SerializeField] private GameObject _transitionUI;
    [SerializeField] private float _heightToReach = 1700;
    [SerializeField] private float _transitionDuration = 2f;

    [Space]
    [Header("Buttons for Menus")]
    [SerializeField] private InputActionReference _inventoryMenuInput;
    [SerializeField] private InputActionReference _pauseMenuInput;

    [Space]
    [Header("Pause menu")]
    [SerializeField] private GameObject _pauseMenuPrefab;

    [Space]
    [Header("Providers references for anchor controls")]
    [SerializeField] private SnapTurnProvider _snapTurnProvider;
    [SerializeField] private ContinuousMoveProvider _dynamicMoveProvider;

    [Space]
    [Header("Grimoire menu inventaire")]
    [SerializeField] private GameObject _grimoire;
    [SerializeField] private Transform _grimoireResetPosition;
    [SerializeField] public TrackedDeviceGraphicRaycaster _grimoireGraphicRaycaster;

    [Space]
    [Header("Heart inventaire")]
    [SerializeField] private GameObject _heartContainer;
    [SerializeField] private Transform _heartContainerReset;
    [SerializeField] private TextMeshProUGUI _healtHP;
    [SerializeField] private List<GameObject> _heartPrefabs;
    private int _currentHeartIndex = 0;

    [Space]
    [Header("Coin inventaire")]
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private Transform _coinContainerReset;
    [SerializeField] private TextMeshProUGUI _coinText;

    [Space]
    [Header("Hourglass inventaire")]
    [SerializeField] private GameObject _hourglassPrefab;
    [SerializeField] private Animator _hourglassAnimator;
    [SerializeField] private Transform _hourglassContainerReset;
    [SerializeField] public TextMeshProUGUI _timeText;

    [Space]
    [Header("Holsters info")]
    [SerializeField] private Transform _rightHandHolsterPosition;
    [SerializeField] private Transform _leftHandHolsterPosition;
    [Space]
    [SerializeField] private XRSocketInteractor _wandHolster;
    [SerializeField] private XRSocketInteractor _shieldHolster;
    [Space]
    [SerializeField] private GameObject _wand;
    [SerializeField] private GameObject _shield;
    [Space]
    [SerializeField] private TextMeshProUGUI _handenessDisplay;

    [Space]
    [Header("Transition sound effects")]
    [SerializeField] private List<AudioClip> _transitionClips;
    [SerializeField] private float _betweenNoteTime;

    [Space]
    [Header("Damage/death SFX")]
    [SerializeField] private AudioSource _playerAudioSource;
    [SerializeField] private List<AudioClip> _hurtSFX;
    [SerializeField] private AudioClip _deathSFX;
    [SerializeField] private AudioClip _defeatClip;

    private int _currentHandenessLayout = 0;

    private bool _areEntitiesSpawned = false;

    void Start()
    {
        //***Check player prefs pour le sort choisis pour commencer la run. Assinge ce sort au grimoire
        //***Check aussi pour pr�f�rence de placement dusocket wand/shield par d�fault
        Time.timeScale = 1f;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        GetKnownSpellPlayerPref();
        GetChosenHolsterLayoutType();

        StartCoroutine(LiftTransitionUI());
        StartCoroutine(PlayTransitionOutSound());
    }

    void Update()
    {
        if (_inventoryMenuInput.action.WasPressedThisFrame())
        {
            //Open Menu inventaire. Fait apparaitre/disparaitre entites
            ToggleSpawnInventoryEntities();
        }
        if (_pauseMenuInput.action.WasPressedThisFrame())
        {
            //Open Menu options.
            TogglePauseMenuState();
        }

        _healtHP.text = _playerHealth.ToString();
        CheckHeartPrefabChoice();

        _coinText.text = _playerGold.ToString();
        SetCoinProperties();
    }

    public void TogglePauseMenuState()
    {
        if (_pauseMenuPrefab.activeSelf)
        {
            Time.timeScale = 1f;
            _pauseMenuPrefab.GetComponent<PauseMenu>().ToggleMainPanel();
            _pauseMenuPrefab.SetActive(false);
        }
        else
        {
            Time.timeScale = 0f;
            _pauseMenuPrefab.SetActive(true);
        }
    }

    private void CheckHeartPrefabChoice()
    {
        //0 - HealthyHeart
        //1 - MediumHeart
        //2 - HurtHeart
        if (_playerHealth >= 66)
        {
            _currentHeartIndex = 0;

        }
        else if (_playerHealth >= 33)
        {
            _currentHeartIndex = 1;
        }
        else
        {
            _currentHeartIndex = 2;
        }

        for (int i = 0; i < _heartPrefabs.Count; i++)
        {
            if (i == _currentHeartIndex)
            {
                _heartPrefabs[i].SetActive(true);
            }
            else
            {
                _heartPrefabs[i].SetActive(false);
            }
        }
    }

    private void SetCoinProperties()
    {
        var emission = _coinPrefab.GetComponentInChildren<ParticleSystem>().emission;
        emission.rateOverTime = _playerGold / 100;
    }

    private void ToggleSpawnInventoryEntities()
    {
        if (_areEntitiesSpawned)
        {
            _grimoire.SetActive(false);
            _heartContainer.SetActive(false);
            _coinPrefab.SetActive(false);
            _hourglassPrefab.SetActive(false);
        }
        else
        {
            _grimoire.SetActive(true);
            _grimoire.transform.SetPositionAndRotation(_grimoireResetPosition.position, _grimoireResetPosition.rotation);
            GiveRandomVelocity(_grimoire.GetComponent<Rigidbody>());

            _heartContainer.SetActive(true);
            _heartContainer.transform.SetPositionAndRotation(_heartContainerReset.position, _heartContainerReset.rotation);
            GiveRandomVelocity(_heartContainer.GetComponent<Rigidbody>());

            _coinPrefab.SetActive(true);
            _coinPrefab.transform.SetPositionAndRotation(_coinContainerReset.position, _coinContainerReset.rotation);
            GiveRandomVelocity(_coinPrefab.GetComponent<Rigidbody>());

            _hourglassPrefab.SetActive(true);
            _hourglassPrefab.transform.SetPositionAndRotation(_hourglassContainerReset.position, _hourglassContainerReset.rotation);
            GiveRandomVelocity(_hourglassPrefab.GetComponent<Rigidbody>());
        }
        _areEntitiesSpawned = !_areEntitiesSpawned;
    }

    private void GiveRandomVelocity(Rigidbody entityRb)
    {
        entityRb.angularVelocity = new(SmallFloatRandom(), SmallFloatRandom(), SmallFloatRandom());
        entityRb.linearVelocity = new(SmallFloatRandom(), SmallFloatRandom(), SmallFloatRandom());
    }

    private float SmallFloatRandom()
    {
        return Random.Range(-0.2f, 0.2f);
    }

    public void EquipSpell(int idSpell, int spellLevel)
    {
        _equippedSpells.Add(new EquippedSpell(idSpell, spellLevel));
        _grimoire.GetComponent<GrimmoireMenu>().AddSpellCard(idSpell);
    }

    public void UnequipSpell(int idSpell)
    {
        foreach (EquippedSpell spell in _equippedSpells)
        {
            if (spell.idSpell == idSpell)
            {
                _equippedSpells.Remove(spell);
                _grimoire.GetComponent<GrimmoireMenu>().RemoveSpellCard(idSpell);
                break;
            }
        }

    }

    public void UnequipAllSpells()
    {
        foreach (EquippedSpell spell in _equippedSpells)
        {
            _grimoire.GetComponent<GrimmoireMenu>().RemoveSpellCard(spell.idSpell);
        }

        _equippedSpells.Clear();
    }

    public EquippedSpell GetEquippedSpell(int idSpell)
    {
        return _equippedSpells.Find(spell => spell.idSpell == idSpell);
    }

    public void TakeDamage(int damage)
    {
        _playerHealth -= damage;

        Debug.Log("Yeouch !");

        if (_playerHealth <= 0)
        {
            _playerHealth = 0;
            AudioManager.Instance.PlayAudioOneTime(_deathSFX, 1, _projectileTarget);
            AudioManager.Instance.PlayAudioOneTime(_defeatClip, 1, _projectileTarget);
            HandleDefeat();
        }
        else
        {
            int index = Random.Range(0, _hurtSFX.Count);

            AudioManager.Instance.PlayAudioOneTime(_hurtSFX[index], 1, _projectileTarget);
        }
    }

    //NOT by Gabriel Porlier
    public void Heal(int amount)
    {
        _playerHealth = Mathf.Min(_playerHealth + amount, 100); // Empêche de dépasser 100 HP

        // Tu peux ajouter ici une mise à jour UI ou un son de soin plus tard
        Debug.Log($"🩹 Joueur soigné de {amount} points. HP actuel : {_playerHealth}");
    }


    public void ReturnEquipementToSockets()
    {
        _wand.transform.position = _wandHolster.transform.position;
        _shield.transform.position = _shieldHolster.transform.position;
    }

    //NOT by Gabriel Porlier
    private void HandleDefeat()
    {
        Time.timeScale = 0f;

        if (_playerAudioSource != null)
        {
            _playerAudioSource.enabled = false;
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        StartCoroutine(UnpauseAndReturnToFirstScene());
    }


    public void SetKnownSpellPlayerPref()
    {
        if (_equippedSpells.Count == 0)
        {
            PlayerPrefs.SetInt(KNOWN_SPELL_KEY, -1);
        }
        else
        {
            PlayerPrefs.SetInt(KNOWN_SPELL_KEY, _equippedSpells[0].idSpell);
        }
    }

    private void GetKnownSpellPlayerPref()
    {
        if (_equippedSpells.Count != 0) return;

        int idSpell = PlayerPrefs.GetInt(KNOWN_SPELL_KEY);

        if (idSpell < 0) return;
        _equippedSpells.Add(new EquippedSpell(idSpell, 1));
    }

    private void ApplyChosenHolsterLayoutType()
    {
        if (_currentHandenessLayout == 0)
        {
            _wandHolster.transform.position = _rightHandHolsterPosition.position;
            _shieldHolster.transform.position = _leftHandHolsterPosition.position;
            _handenessDisplay.text = "Défaut : Droitier";
        }
        else
        {
            _wandHolster.transform.position = _leftHandHolsterPosition.position;
            _shieldHolster.transform.position = _rightHandHolsterPosition.position;
            _handenessDisplay.text = "Défaut : Gaucher";
        }
    }

    public void ModifyChosenHolsterLayoutType()
    {
        if (_currentHandenessLayout == 0)
            _currentHandenessLayout = 1;
        else
            _currentHandenessLayout = 0;


        ApplyChosenHolsterLayoutType();
        SetChosenHolsterLayoutType();
    }

    public void SetChosenHolsterLayoutType()
    {
        PlayerPrefs.SetInt(HOLSTER_HANDENESS_KEY, _currentHandenessLayout);
    }

    private void GetChosenHolsterLayoutType()
    {
        _currentHandenessLayout = PlayerPrefs.GetInt(HOLSTER_HANDENESS_KEY);

        ApplyChosenHolsterLayoutType();
    }

    public void HoldingAnchorControls(SelectEnterEventArgs args)
    {
        XRBaseInteractor _currentController = args.interactorObject as XRBaseInteractor;

        if (_currentController.gameObject.tag == "RightController")
        {
            _snapTurnProvider.enabled = false;
        }
        if (_currentController.gameObject.tag == "LeftController")
        {
            _dynamicMoveProvider.enabled = false;
        }
    }

    public void ReleaseAnchorControls(SelectExitEventArgs args)
    {
        _snapTurnProvider.enabled = true;
        _dynamicMoveProvider.enabled = true;
    }

    private IEnumerator UnpauseAndReturnToFirstScene()
    {
        yield return new WaitForSecondsRealtime(4f);

        _playerHealth = 100;
        _playerGold = 0;
        for (int i = 0; i < _equippedSpells.Count; i++)
        {
            if (i != 0)
                _equippedSpells.RemoveAt(i);
        }

        ReturnEquipementToSockets();

        if (_areEntitiesSpawned)
        {
            _grimoire.SetActive(false);
            _heartContainer.SetActive(false);
            _coinPrefab.SetActive(false);
            _hourglassPrefab.SetActive(false);
        }

        /*
        gameOverUI.SetActive(false);
        
        Time.timeScale = 1f;

        SceneManager.LoadScene(0);
        */

        CallSceneChange(0);
    }

    private IEnumerator LiftTransitionUI()
    {
        RectTransform _positionUI = _transitionUI.GetComponent<RectTransform>();

        Vector2 startingPosition = _positionUI.anchoredPosition;
        Vector2 endPosition = new(0f, _heightToReach);

        float elapsedTime = 0;


        while (elapsedTime < _transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _transitionDuration);
            _positionUI.anchoredPosition = Vector2.Lerp(startingPosition, endPosition, t);
            yield return null;
        }

        _transitionUI.SetActive(false);
    }

    private IEnumerator PlayTransitionOutSound()
    {
        for (int i = 0; i < _transitionClips.Count; i++)
        {
            AudioManager.Instance.PlayAudioOneTime(_transitionClips[i], 0.7f, transform);
            yield return new WaitForSecondsRealtime(_betweenNoteTime);
        }
    }

    [ContextMenu("Debug scene change option go back to main menu")]
    public void TestingSceneChange()
    {
        StartCoroutine(SceneChangeTransition(0));
    }

    public void CallSceneChange(int sceneIndex)
    {
        StartCoroutine(SceneChangeTransition(sceneIndex));
        StartCoroutine(PlayTransitionInSound());
    }

    public IEnumerator SceneChangeTransition(int sceneIndex)
    {
        _transitionUI.SetActive(true);

        RectTransform _positionUI = _transitionUI.GetComponent<RectTransform>();

        Vector2 startingPosition = _positionUI.anchoredPosition;
        Vector2 endPosition = new(0f, 0f);

        float elapsedTime = 0;

        while (elapsedTime < _transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / _transitionDuration);
            _positionUI.anchoredPosition = Vector2.Lerp(startingPosition, endPosition, t);
            yield return null;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    private IEnumerator PlayTransitionInSound()
    {

        for (int i = _transitionClips.Count; i > 0; i--)
        {

            AudioManager.Instance.PlayAudioOneTime(_transitionClips[i - 1], 0.7f, transform);
            yield return new WaitForSecondsRealtime(_betweenNoteTime);
        }
    }

    //NOT by Gabriel Porlier
    public IEnumerator FadeOutAudioSource()
    {
        if (_playerAudioSource == null)
            yield break;

        float duration = 2f;
        float interval = 0.1f;
        float steps = duration / interval;
        float initialVolume = _playerAudioSource.volume;

        for (int i = 0; i < steps; i++)
        {
            _playerAudioSource.volume -= initialVolume * 0.1f;
            yield return new WaitForSecondsRealtime(interval);
        }

        _playerAudioSource.volume = 0f; // Ensure volume is exactly 0
    }

    //NOT by Gabriel Porlier
    public IEnumerator FadeInAudioSource()
    {
        if (_playerAudioSource == null)
            yield break;

        float duration = 2f;
        float interval = 0.1f;
        float steps = duration / interval;
        float targetVolume = 0.75f;

        for (int i = 0; i < steps; i++)
        {
            _playerAudioSource.volume += targetVolume * 0.1f;
            yield return new WaitForSecondsRealtime(interval);
        }

        _playerAudioSource.volume = targetVolume;
    }



}





