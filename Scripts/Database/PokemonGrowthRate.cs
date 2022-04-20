using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGrowthRate
{
    Dictionary<int, int> slowGrowthRate;
    Dictionary<int, int> mediumGrowthRate;
    Dictionary<int, int> fastGrowthRate;
    Dictionary<int, int> mediumSlowGrowthRate;
    Dictionary<int, int> slowThenFastGrowthRate;
    Dictionary<int, int> fastThenSlowGrowthRate;

    public PokemonGrowthRate()
    {
        this.slowGrowthRate = new Dictionary<int, int>();
        this.mediumGrowthRate = new Dictionary<int, int>();
        this.fastGrowthRate = new Dictionary<int, int>();
        this.mediumSlowGrowthRate = new Dictionary<int, int>();
        this.slowThenFastGrowthRate = new Dictionary<int, int>();
        this.fastThenSlowGrowthRate = new Dictionary<int, int>();
    }

    public void addGrowthRateElement(int id, int level, int experience)
    {
        switch (id)
        {
            case 1:
                this.slowGrowthRate.Add(level, experience);
                break;
            case 2:
                this.mediumGrowthRate.Add(level, experience);
                break;
            case 3:
                this.fastGrowthRate.Add(level, experience);
                break;
            case 4:
                this.mediumSlowGrowthRate.Add(level, experience);
                break;
            case 5:
                this.slowThenFastGrowthRate.Add(level, experience);
                break;
            case 6:
                this.fastThenSlowGrowthRate.Add(level, experience);
                break;
            default:
                break;
        }
    }
    public int GetExperienceAtLevel(int id, int level)
    {
        switch (id)
        {
            case 1:
                return this.slowGrowthRate[level];
            case 2:
                return this.mediumGrowthRate[level];
            case 3:
                return this.fastGrowthRate[level];
            case 4:
                return this.mediumGrowthRate[level];
            case 5:
                return this.slowThenFastGrowthRate[level];
            case 6:
                return this.fastThenSlowGrowthRate[level];
            default:
                Debug.LogError($"Unknown growthRateID: {id}. Using mediumGrowthRate.");
                return this.mediumGrowthRate[level];
        }
    }
}
