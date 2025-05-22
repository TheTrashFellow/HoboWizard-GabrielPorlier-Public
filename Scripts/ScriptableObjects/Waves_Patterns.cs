using System.Collections.Generic;
using UnityEngine;
using static ChallengeStart;
using static Spell_Library;

/*
 * Code by Gabriel Porlier
 * 
 * 
 * 
 * Level 1 - Faster but restrained patterns. Let the experienced players, rack up the mone early by completing easier patterns.
 *           But not too fast, nor too difficult that newer players don't get overwhelmed
 *           
 *           4 waves. Each waves gives 20 and 5 as bonus. Player can get 100$ in this area
 * 
 * Level 2 - Longuer waves but with more ennemies. First time players could possibly die here. Let them sweat a little before trhowing them
 *           a shop as the next room. 
 *           Experience players should rather still easily get trough the level with most of the bonus achieved
 *           
 *           4 waves. Each waves gives 40 and 10 as bonus. Player can get 200$ in this area
 *           
 * Level 3 - Same wave patterns, but with much less time on hand. Players have to have leveled up spells and mastered parrying to get trough.
 *           Players of all levels could get overwhelmed if not concentrated. (Scale ennemies hp ?)
 *           
 *           
 *           
 *           
 * Level 4 - Should push the players, to use all of their capacities to not get overwhelmed. Only strong strategies and experience can possibly get
 *           the completion bonuses.
 *           
 *           ***INCREASE GOLD COSTS OF SECOND SHOP ?***
 * 
 * */

[CreateAssetMenu(fileName = "WavePatterns", menuName = "CustomLibraries/Wave Patterns")]

public class Waves_Patterns : ScriptableObject
{


    [System.Serializable]
    public class LevelAssociatedPattens
    {
        public int level;
        public int patternsToInclude;
        public int goldReward;
        public int bonusReward;
        public List<WaveData> patterns;
    }

    public List<WaveData> GetPattern(int areaLevel)
    {

        foreach (LevelAssociatedPattens list in wavePatterns)
        {
            if (list.level == areaLevel)
            {
                return list.patterns;
            }
        }
        return null;
    }

    public LevelAssociatedPattens GetLevelInfos(int areaLevel)
    {
        foreach (LevelAssociatedPattens list in wavePatterns)
        {
            if (list.level == areaLevel)
            {
                return list;
            }
        }
        return null;
    }

    public List<LevelAssociatedPattens> wavePatterns = new();

}
