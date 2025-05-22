using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static ChallengeStart;
using static Waves_Patterns;

/*
 * Code by Jonathan Gremmo
 * 
 * Integration of WavePatterns flexibility by Scriptable object by Gabriel Porlier
 * 
 * */

public class RoomLoader : MonoBehaviour
{
    public GameObject startingAreaPrefab;
    public GameObject[] arenaPrefabs;       // Normal arena prefabs (used only once)
    public GameObject[] specialArenaPrefabs; // Special arena prefabs (can repeat)
    public GameObject[] bossArenaPrefabs;   // Boss arena prefabs

    public int totalArenas = 6; // Number of arenas to spawn

    private GameObject startingAreaInstance;
    private GameObject bossArenaInstance;

    private List<GameObject> availableNormalArenas; // List to track unused normal arenas

    [SerializeField] private Waves_Patterns wavesPattern;

    void Start()
    {
        // Initialize list of unused normal arenas
        availableNormalArenas = new List<GameObject>(arenaPrefabs);

        LoadRooms();
    }

    void LoadRooms()
    {
        startingAreaInstance = Instantiate(startingAreaPrefab, Vector3.zero, Quaternion.identity);

        GameObject previousArena = startingAreaInstance;
        int areaLevel = 1;

        for (int i = 1; i <= totalArenas; i++)
        {
            GameObject newArena;
            if (i % 3 == 0) // Every third arena is a special one
            {
                newArena = InstantiateRandomSpecialArena();
            }
            else
            {
                newArena = InstantiateUniqueNormalArena(areaLevel);
                areaLevel++;
            }

            PositionArenaAtAnchorPoint(newArena, previousArena);
            previousArena = newArena;
        }

        bossArenaInstance = InstantiateRandomBossArena();
        PositionArenaAtAnchorPoint(bossArenaInstance, previousArena);
    }

    // Instantiate a normal arena that hasn't been used yet
    GameObject InstantiateUniqueNormalArena(int areaLevel)
    {
        if (availableNormalArenas.Count > 0)
        {
            int randomIndex = Random.Range(0, availableNormalArenas.Count);
            GameObject selectedArena = availableNormalArenas[randomIndex];
            availableNormalArenas.RemoveAt(randomIndex); // Remove to prevent reuse

            List<WaveData> waves = wavesPattern.GetPattern(areaLevel);
            LevelAssociatedPattens toInclude = wavesPattern.GetLevelInfos(areaLevel);

            List<WaveData> toSendWaves = GetRandomElements(waves, toInclude.patternsToInclude);

            ChallengeStart area = selectedArena.GetComponentInChildren<ChallengeStart>();

            area.waves = toSendWaves;
            area.goldReward = toInclude.goldReward;
            area.bonusGoldReward = toInclude.bonusReward;

            return Instantiate(selectedArena);
        }
        else
        {
            Debug.LogWarning("No more unique normal arenas available. Consider adding more or reducing totalArenas.");
            return null;
        }
    }

    List<T> GetRandomElements<T>(List<T> list, int count)
    {
        // Shuffle and take the desired number
        return list.OrderBy(x => Random.value).Take(count).ToList();
    }

    GameObject InstantiateRandomSpecialArena()
    {
        if (specialArenaPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, specialArenaPrefabs.Length);
            return Instantiate(specialArenaPrefabs[randomIndex]);
        }
        else
        {
            Debug.LogWarning("No special arena prefabs assigned.");
            return null;
        }
    }

    GameObject InstantiateRandomBossArena()
    {
        if (bossArenaPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, bossArenaPrefabs.Length);
            return Instantiate(bossArenaPrefabs[randomIndex]);
        }
        else
        {
            Debug.LogWarning("No boss arena prefabs assigned.");
            return null;
        }
    }

    void PositionArenaAtAnchorPoint(GameObject arena, GameObject previousArena)
    {
        if (arena == null || previousArena == null) return;

        Transform anchorPoint = previousArena.transform.Find("AnchorPoint");

        if (anchorPoint != null)
        {
            arena.transform.position = anchorPoint.position;
            arena.transform.rotation = anchorPoint.rotation;
        }
        else
        {
            Debug.LogWarning("AnchorPoint not found in the previous arena prefab.");
        }
    }
}
