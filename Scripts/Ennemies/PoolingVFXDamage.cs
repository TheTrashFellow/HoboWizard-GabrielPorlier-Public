using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Barracuda.TextureAsTensorData;
using UnityEngine.Rendering;

/*
    Code by Gabriel Porlier

    I made a pool for the damaged effect my colleague added to the game for parry hits. I extended it's use to any hits and made it into an instance so it would be easily accessible. 
*/

public class PoolingVFXDamage : MonoBehaviour
{
    public static PoolingVFXDamage Instance { get; private set; }

    [SerializeField] private GameObject _parryHitVFX = default;

    [SerializeField] private List<AudioClip> _hitSFX;

    private int _poolSize = 10;

    private Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject instance = Instantiate(_parryHitVFX);
            instance.SetActive(false);
            _pool.Enqueue(instance);
        }
    }

    public void EnnemyDamagedVFX(Transform destination)
    {
        int audioIndex = Random.Range(0, _hitSFX.Count);

        if (_pool.Count > 0)
        {
            GameObject instance = _pool.Dequeue();
            instance.SetActive(true);
            instance.transform.position = destination.position;

            AudioManager.Instance.PlayAudioOneTime(_hitSFX[audioIndex], 1f, destination);

            StartCoroutine(RepoolVFX(instance));
        }
        else
        {
            Debug.LogWarning("Pool �puis�e ! Augmentez la taille.");
        }
    }

    IEnumerator RepoolVFX(GameObject instance)
    {
        yield return new WaitForSeconds(1f);

        instance.SetActive(false);
        _pool.Enqueue(instance);
    }
}
