using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

/*
    Code adapted by Gabriel Porlier
*/

public class SlimeSmall : Ennemy
{
    [Header("Slime Jump Stats")]
    public float jumpHeight = 2f; // Hauteur du saut
    public float jumpCooldown = 1.5f; // Temps entre les sauts
    public float jumpDuration = 0.5f; // Durée du saut
    public float jumpDistance = 2f; // Distance horizontale du saut

    private bool isJumping = false; // Indique si le slime est en train de sauter
    private float originalSpeed; // Stocke la vitesse originale du NavMeshAgent
    private float damageInterval = 1f; // Intervalle entre chaque dégât
    private int damagePerTick = 10; // Dégâts infligés à chaque tick
    private GameObject healthBarPrefab; // Prefab de la barre de vie
    private Slider healthBar; // Référence à la barre de vie instanciée
    private Camera _mainCamera; // Référence à la caméra principale
    private float healthBarFixedY; // Hauteur fixe de la barre de vie
    private TextMeshProUGUI healthText; // Référence au texte de la barre de vie

    [SerializeField] private Animator _animator = default;

    [SerializeField] private GameObject _deathVFX = default;
    [SerializeField] private Vector3 _deathVFXOffset = Vector3.zero;

    [Space]
    [SerializeField] private AudioClip _hittingGroundAudio;

    void Start()
    {        
        //_navMeshAgent.updatePosition = true; // Laisser le NavMeshAgent gérer la position
        //_navMeshAgent.updateRotation = true; // Laisser le NavMeshAgent gérer la rotation
        //originalSpeed = _navMeshAgent.speed; // Stocker la vitesse originale
        //StartCoroutine(DamageOverTime());

        // Récupérer la caméra principale
        //_mainCamera = _player._mainCamera;

        // Instancier la barre de vie
        /* Quelque chose de plus Expressif qu'une health bar ??? 
        if (healthBarPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
            healthBar = healthBarInstance.GetComponentInChildren<Slider>();
            healthBarInstance.transform.SetParent(transform, false); // Faire de la barre de vie un enfant du slime
            healthBarFixedY = healthBarInstance.transform.position.y; // Stocker la hauteur fixe
            healthText = healthBarInstance.GetComponentInChildren<TextMeshProUGUI>();
        }*/
    }

    void Update()
    {
        if (_playerTargetPosition == null || !_navMeshAgent.enabled) return;

        Vector3 direction = _playerTargetPosition.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 40 * Time.deltaTime);

        // Mettre à jour la destination du NavMeshAgent
        _navMeshAgent.SetDestination(_playerTargetPosition.position);

        // Lancer le saut si le slime n'est pas déjà en train de sauter
        if (!isJumping)
        {
            StartCoroutine(Jump());
        }

        /* Quelque chose de plus Expressif qu'une health bar ??? 
        if (healthBar != null)
        {
            // Fixer la position en Y pour éviter les mouvements verticaux
            Vector3 healthBarPosition = transform.position;
            healthBarPosition.y = healthBarFixedY; // Utiliser la hauteur fixe
            healthBar.transform.position = healthBarPosition;

            // Faire en sorte que la barre de vie regarde toujours la caméra
            healthBar.transform.LookAt(_mainCamera.transform);
            healthBar.transform.Rotate(0, 180, 0); // Inverser l'orientation pour que la barre soit lisible
        }*/
    }

    private IEnumerator Jump()
    {
        isJumping = true;

        _animator.SetTrigger("TriggerAttack");

        // S'assurer que le NavMeshAgent a une destination valide
        _navMeshAgent.SetDestination(_playerTargetPosition.position);

        // Attendre que le NavMeshAgent calcule un chemin valide
        while (_navMeshAgent.pathPending) // Attendre que le chemin soit calculé
        {
            yield return null;
        }

        // Vérifier si le chemin est valide
        if (_navMeshAgent.path.status != NavMeshPathStatus.PathComplete)
        {
            isJumping = false; // Annuler le saut si aucun chemin valide n'est trouvé
            yield break;
        }

        // Récupérer la première position du chemin pour déterminer la direction
        Vector3 jumpDirection;
        if (_navMeshAgent.path.corners.Length > 1)
        {
            jumpDirection = (_navMeshAgent.path.corners[1] - transform.position).normalized;
        }
        else
        {
            // Si aucune direction n'est trouvée, utiliser une direction par défaut vers le joueur
            jumpDirection = (_playerTargetPosition.position - transform.position).normalized;
        }

        // Calculer la cible du saut
        Vector3 jumpTarget = transform.position + jumpDirection * jumpDistance;

        // Simuler l'arc du saut
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < jumpDuration)
        {
            float progress = elapsedTime / jumpDuration;

            // Calculer l'arc vertical (mouvement parabolique)
            float height = Mathf.Sin(Mathf.PI * progress) * jumpHeight;

            // Interpoler la position
            Vector3 currentPosition = Vector3.Lerp(startPosition, jumpTarget, progress) + Vector3.up * height;
            _navMeshAgent.Warp(currentPosition); // Synchroniser la position avec le NavMeshAgent

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        AudioManager.Instance.PlayAudioOneTime(_hittingGroundAudio, 1, _damagedEffectTransform);

        // Attendre avant le prochain saut
        yield return new WaitForSeconds(jumpCooldown);
        isJumping = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Empêcher le mouvement en réglant la vitesse à 0
            _navMeshAgent.speed = 0;
            AudioManager.Instance.PlayAudioOneTime(_hittingGroundAudio, 1, _damagedEffectTransform);
        }
        
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Rétablir la vitesse originale
            _navMeshAgent.speed = originalSpeed;

           
        }
    }

    protected override void Die()
    {
        try
        {
            _navMeshAgent.enabled = false; 
            GameObject vfxInstance = Instantiate(_deathVFX, transform.position, transform.rotation);
            vfxInstance.transform.Rotate(-90f, 0f, 0f);
            Destroy(gameObject);
            /*if (_deathVFX != null)
            {
                Vector3 spawnPosition = transform.position + transform.TransformVector(_deathVFXOffset);

                GameObject vfxInstance = Instantiate(_deathVFX, spawnPosition, transform.rotation);
                vfxInstance.transform.Rotate(-90f, 0f, 0f);
            }

            Destroy(gameObject);*/

        }
        catch { }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().TakeDamage(_damage);
            Die();            
        }

        if (other.gameObject.CompareTag("Shield"))
        {            
            Die();
        }
    }

    /*
    private void UpdateHealthBarColor()
    {
        // Récupérer l'objet "Fill" du Slider
        Image fillImage = healthBar.fillRect.GetComponent<Image>();

        if (fillImage != null)
        {
            // Calculer la couleur en fonction de la santé
            if (_healthPoints > 50)
            {
                // De vert à jaune (progressivement)
                float t = (_healthPoints - 50) / 50f; // Normaliser entre 0 et 1
                fillImage.color = Color.Lerp(Color.yellow, Color.green, t);
            }
            else
            {
                // De jaune à rouge (progressivement)
                float t = _healthPoints / 50f; // Normaliser entre 0 et 1
                fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
            }
        }
    }*/

    private IEnumerator DamageOverTime()
    {
        while (_healthPoints > 0)
        {
            TakeDamage(damagePerTick); // Infliger des dégâts
            yield return new WaitForSeconds(damageInterval); // Attendre avant le prochain tick
        }
    }
}