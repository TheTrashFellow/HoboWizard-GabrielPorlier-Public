using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using static Unity.Barracuda.TextureAsTensorData;

/*
 * Code by Gabriel Porlier

   It's my attempt at making a centralized audio manager. By pooling GameObjects that contains an audio source, I can call the Instance of the AudioManager from any script to play sounds.

   The AudioManager Instance Script is found on the Player GameObject so it's always available troughout the scenes. 

   There were oversights that caused problems down the line which I'll remember for next time. Also, I messed up when developping at near the end since I kept loosing elements from my pool.
   So I added a function to re-instantiate elements if I were to loose too much. I would have liked to NOT have to do that, but time constraints have no mercy for us.
 * */
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameObject _audioPrefab;
    [SerializeField] public int _poolSize = 20;    

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

    private void Update()
    {
        
    }

    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject instance = Instantiate(_audioPrefab);
            instance.SetActive(false);
            _pool.Enqueue(instance);
        }
    }

    public void ResetPool()
    {
        for(int i = _pool.Count; i < _poolSize - 2; i++)
        {
            GameObject instance = Instantiate(_audioPrefab);
            instance.SetActive(false);
            _pool.Enqueue(instance);
        }
    }

    public void PlayAudioOneTime(AudioClip clip, float volume, Transform soundPosition)
    {
        if (_pool.Count > 0) 
        {
            GameObject instance = _pool.Dequeue();
            instance.SetActive(true);
            instance.GetComponent<AudioSource>().enabled = true;
            instance.transform.position = soundPosition.position;
            //instance.transform.SetParent(soundPosition);
            instance.GetComponent<AudioSource>().PlayOneShot(clip, volume);
            StartCoroutine(RepoolAudio(instance, clip.length + 0.1f));
        }
        else
        {
            Debug.LogWarning("Pool �puis�e ! Augmentez la taille.");
        }
    }

    public GameObject SetLoopAudioObject(AudioClip clip, float volume)
    {
        if (_pool.Count > 0)
        {
            GameObject instance = _pool.Dequeue();
            instance.SetActive(true);
            instance.GetComponent<AudioSource>().enabled = true;
            instance.GetComponent<AudioSource>().clip = clip;
            instance.GetComponent<AudioSource>().loop = true;
            instance.GetComponent<AudioSource>().Play();

            return instance;
        }
        else
        {
            Debug.LogWarning("Pool �puis�e ! Augmentez la taille.");
            return null;            
        }
    }

    public void StopAudioObject(GameObject instance)
    {
        instance.GetComponent<AudioSource>().loop = false;
        instance.GetComponent<AudioSource>().clip = null;
        instance.GetComponent<AudioSource>().Stop();

        instance.SetActive(false);
        _pool.Enqueue(instance);        
    }

    IEnumerator RepoolAudio(GameObject instance, float audioTime)
    {
        yield return new WaitForSeconds(audioTime);

        instance.SetActive(false);
        _pool.Enqueue(instance);
    }
}
