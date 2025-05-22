using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/*
 * Code adapted by Gabriel Porlier

    Adapts the small slime ennemy to fit it's new behaviour with the boss. 
 * */
public class BossSmallSlime : Ennemy
{
 
    [Header("Slime Jump Stats")]
    public float jumpHeight = 2f; // Hauteur du saut
    public float jumpCooldown = 1.5f; // Temps entre les sauts
    public float jumpDuration = 0.5f; // Dur�e du saut
    public float jumpDistance = 2f; // Distance horizontale du saut

    private bool isJumping = false; // Indique si le slime est en train de sauter
    private float originalSpeed; // Stocke la vitesse originale du NavMeshAgent

    private Transform _destinationPoint;

    [Space]
    [Header("Death VFX")]
    [SerializeField] private GameObject _deathVFX = default;
    [SerializeField] private Vector3 _deathVFXOffset = Vector3.zero;

    [Space]
    [SerializeField] private AudioClip _hittingGroundAudio;

    void Start()
    {
        _navMeshAgent.updatePosition = true; // Laisser le NavMeshAgent g�rer la position
        _navMeshAgent.updateRotation = true; // Laisser le NavMeshAgent g�rer la rotation
        originalSpeed = _navMeshAgent.speed; // Stocker la vitesse originale        
    }

    void Update()
    {
        if (_destinationPoint == null) return;
        if (!_navMeshAgent.enabled) return;        

        // Lancer le saut si le slime n'est pas d�j� en train de sauter
        if (!isJumping)
        {
            StartCoroutine(Jump());
        }

        Vector3 direction = _destinationPoint.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 40 * Time.deltaTime);

    }

    public void SetDestination(Transform destination)
    {
        _destinationPoint = destination;
    }

    private IEnumerator Jump()
    {
        isJumping = true;

        try
        {
            // S'assurer que le NavMeshAgent a une destination valide
            _navMeshAgent.SetDestination(_destinationPoint.position);
        }
        catch
        {
            Debug.Log("Couldn't set destination");
        }

        // Attendre que le NavMeshAgent calcule un chemin valide
        while (_navMeshAgent.pathPending) // Attendre que le chemin soit calcul�
        {
            yield return null;
        }

        // V�rifier si le chemin est valide
        if (_navMeshAgent.path.status != NavMeshPathStatus.PathComplete)
        {
            isJumping = false; // Annuler le saut si aucun chemin valide n'est trouv�
            yield break;
        }

        // R�cup�rer la premi�re position du chemin pour d�terminer la direction
        Vector3 jumpDirection;
        if (_navMeshAgent.path.corners.Length > 1)
        {
            jumpDirection = (_navMeshAgent.path.corners[1] - transform.position).normalized;
        }
        else
        {
            // Si aucune direction n'est trouv�e, utiliser une direction par d�faut vers le joueur
            jumpDirection = (_destinationPoint.position - transform.position).normalized;
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

    public new void Die()
    {
        try
        {
            _navMeshAgent.enabled = false;
            GameObject vfxInstance = Instantiate(_deathVFX, transform.position, transform.rotation);
            vfxInstance.transform.Rotate(-90f, 0f, 0f);
            Destroy(gameObject);            

        }
        catch { }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Emp�cher le mouvement en r�glant la vitesse � 0
            _navMeshAgent.speed = 0;
            AudioManager.Instance.PlayAudioOneTime(_hittingGroundAudio, 1, _damagedEffectTransform);
        }

    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // R�tablir la vitesse originale
            _navMeshAgent.speed = originalSpeed;
        }
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
            bool _parried = other.gameObject.GetComponentInParent<ShieldManager>().BlockedAttack();
            if (_parried)
            {
                //Sound Effect et reward different ?
                Die();
            }
            else
            {
                Die();
            }
        }

        if (other.gameObject.CompareTag("Ennemy"))
        {
            try
            {
                other.gameObject.GetComponentInParent<SlimeBoss>().HealBack(0.02f);

                AudioManager.Instance.PlayAudioOneTime(_hittingGroundAudio, 2, _damagedEffectTransform);

                Destroy(gameObject);
            }
            catch
            {
                Debug.Log("Couldn't heal boss ? ");
                return;
            }
                        
        }
    }

    IEnumerator DelayedDestroyed()
    {
        yield return new WaitForSeconds(15);

        Die();
    }

    /*
    private void UpdateHealthBarColor()
    {
        // R�cup�rer l'objet "Fill" du Slider
        Image fillImage = healthBar.fillRect.GetComponent<Image>();

        if (fillImage != null)
        {
            // Calculer la couleur en fonction de la sant�
            if (_healthPoints > 50)
            {
                // De vert � jaune (progressivement)
                float t = (_healthPoints - 50) / 50f; // Normaliser entre 0 et 1
                fillImage.color = Color.Lerp(Color.yellow, Color.green, t);
            }
            else
            {
                // De jaune � rouge (progressivement)
                float t = _healthPoints / 50f; // Normaliser entre 0 et 1
                fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
            }
        }
    }*/
    

}
