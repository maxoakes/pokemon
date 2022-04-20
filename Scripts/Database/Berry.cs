using System.Collections;
using System.Collections.Generic;

public class Berry
{
    public readonly int id;
    public readonly int itemID;
    public readonly int firmnessID;
    public readonly int naturalGiftPower;
    public readonly int naturalGiftTypeID;
    public readonly int size;
    public readonly int maxHarvest;
    public readonly int growthTime;
    public readonly int soilDryness;
    public readonly int smoothness;

    public Berry(int id, int itemID, int firmnessID, int naturalGiftPower,
        int naturalGiftTypeID, int size, int maxHarvest, int growthTime, int soilDryness, int smoothness)
    {
        this.id = id;
        this.itemID = itemID;
        this.firmnessID = firmnessID;
        this.naturalGiftPower = naturalGiftPower;
        this.naturalGiftTypeID = naturalGiftTypeID;
        this.size = size;
        this.maxHarvest = maxHarvest;
        this.growthTime = growthTime;
        this.soilDryness = soilDryness;
        this.smoothness = smoothness;
    }

    public override string ToString()
    {
        return this.itemID.ToString();
    }
}
