using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/*
 * Code by Jonathan Gremmo
 * 
 * Integration of timer entity logic by Gabriel Porlier
 * Integration of flexible ennemies wave patterns by scriptable object by Gabriel Porlier
 * */

public class ChallengeStart : MonoBehaviour
{
    private List<GameObject> activeEnemies = new List<GameObject>();

    [Header("Spawner Points")]
    public GameObject[] groundedSpawners;
    public GameObject[] flyingSpawners;
    public GameObject[] smallSpawners;

    [Header("Enemy Prefabs")]
    public GameObject groundedEnemyPrefab;
    public GameObject flyingEnemyPrefab;
    public GameObject smallEnemyPrefab;

    [Header("Player")]
    public Player player; // Don't assign in Inspector — it finds the Player at runtime

    [Header("Invisible Walls")]
    public GameObject invisibleWalls; // Reference to the InvisibleWalls object
    private GameObject invisWallEnd;    // Reference to the InvisWallEnd child

    [Header("UI")]
    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI txtHealth;
    public TextMeshProUGUI txtGold;

    [Space]
    [SerializeField] private AudioClip _roomFinishedJingle;

    [System.Serializable]
    public class WaveData
    {
        public int groundedCount;
        public int flyingCount;
        public int smallCount;
        public float timeBeforeNextWave = 10f;
    }

    [Header("Wave Settings")]
    public List<WaveData> waves = new List<WaveData>();

    // Gold reward settings
    public int goldReward = 50; // Base gold reward
    public int bonusGoldReward = 20; // Bonus if wave ends early

    private bool hasTriggered = false;
    private int currentWaveIndex = 0;
    private bool waveInProgress = false;

    private GameObject enemyContainer; // Reference to the "EnemyContainer" GameObject

    private void Awake()
    {
        // Auto-find the Player in the scene
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();

            if (player == null)
            {
                Debug.LogError("Player not found in the scene! Make sure a Player with the Player script is active.");
            }
        }

        // Ensure InvisibleWalls is set in the Inspector
        if (invisibleWalls != null)
        {
            invisibleWalls.SetActive(false); // Make sure it's initially inactive
            // Find the InvisWallEnd child object
            invisWallEnd = invisibleWalls.transform.Find("InvisWallEnd")?.gameObject;
        }

        // Automatically find the EnemyContainer in the scene by name
        enemyContainer = GameObject.Find("EnemyContainer");
        if (enemyContainer == null)
        {
            Debug.LogError("EnemyContainer not found in the scene! Please ensure it exists and is named 'EnemyContainer'.");
        }
    }

    private void Start()
    {



    }

    private void Update()
    {
        
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            // Activate the InvisibleWalls object when the player triggers the challenge
            if (invisibleWalls != null)
            {
                invisibleWalls.SetActive(true); // Enable the InvisibleWalls
            }

            player = other.GetComponent<Player>();

            txtTimer = player._timeText;

            StartCoroutine(HandleWaves());
        }
    }

    private IEnumerator HandleWaves()
    {
        // Show the timer UI at the start of the challenge
        if (txtTimer != null)
            txtTimer.gameObject.SetActive(true); // Show the timer UI

        while (currentWaveIndex < waves.Count)
        {
            waveInProgress = true;
            WaveData wave = waves[currentWaveIndex];

            activeEnemies.Clear();
            SpawnEnemies(wave);
            currentWaveIndex++;

            float timeLeft = wave.timeBeforeNextWave;
            while (timeLeft > 0f)
            {
                if (activeEnemies.Count == 0)
                    break;

                // ? Update UI timer
                if (txtTimer != null)
                    txtTimer.text = "Prochaine Vague Dans: " + Mathf.CeilToInt(timeLeft).ToString();

                timeLeft -= Time.deltaTime;
                yield return null;
            }

            IncreasePlayerGold(timeLeft, wave.timeBeforeNextWave);

            // Clear the timer display after each wave (optional)
            if (txtTimer != null)
                txtTimer.text = "";  // Hide the timer text after the wave
        }

        waveInProgress = false;

        
        StartCoroutine(WaitForAllEnemiesDefeated());
    }

    private IEnumerator WaitForAllEnemiesDefeated()
    {
        txtTimer.text = "Élimine le reste";

        while (activeEnemies.Count > 0)
        {
            yield return null;
        }

        txtTimer.text = "En attente";

        // All enemies are defeated: disable the wall
        if (invisWallEnd != null)
        {
            invisWallEnd.SetActive(false);
            AudioManager.Instance.ResetPool();

            AudioManager.Instance.PlayAudioOneTime(_roomFinishedJingle, 1f, transform);
        }
    }

    private void SpawnEnemies(WaveData wave)
    {
        SpawnEnemyGroup(groundedEnemyPrefab, groundedSpawners, wave.groundedCount);
        SpawnEnemyGroup(flyingEnemyPrefab, flyingSpawners, wave.flyingCount);
        SpawnEnemyGroup(smallEnemyPrefab, smallSpawners, wave.smallCount);
    }

    private void SpawnEnemyGroup(GameObject prefab, GameObject[] spawners, int count)
    {
        if (spawners.Length == 0 || prefab == null || enemyContainer == null) return;

        int spawnCount = Mathf.Min(count, spawners.Length);
        List<GameObject> availableSpawners = new List<GameObject>(spawners);

        for (int i = 0; i < spawnCount; i++)
        {
            int index = Random.Range(0, availableSpawners.Count);
            GameObject spawner = availableSpawners[index];
            GameObject spawnedEnemy = Instantiate(prefab, spawner.transform.position, spawner.transform.rotation);

            // Set the spawned enemy as a child of the EnemyContainer
            spawnedEnemy.transform.SetParent(enemyContainer.transform);

            activeEnemies.Add(spawnedEnemy);

            EnemyDespawnWatcher watcher = spawnedEnemy.AddComponent<EnemyDespawnWatcher>();
            watcher.challengeStart = this;

            availableSpawners.RemoveAt(index);
        }
    }

    private void IncreasePlayerGold(float timeLeft, float waveDuration)
    {
        if (player == null) return;

        // Always give base reward
        player._playerGold += goldReward;

        // Bonus reward if wave was cleared quickly (more than half the time left)
        if (timeLeft > waveDuration * 0.5f)
        {
            player._playerGold += bonusGoldReward;
        }
    }

    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
}
