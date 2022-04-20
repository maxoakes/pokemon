using System.Collections;
using System.Collections.Generic;

public class MoveEffect
{
    public readonly int id;
    public readonly string effectShort;
    public readonly string effectLong;

    public MoveEffect(int id, string effectShort, string effectLong)
    {
        this.id = id;
        this.effectShort = effectShort;
        this.effectLong = effectLong;
    }

    public override string ToString()
    {
        return this.effectShort;
    }
}
