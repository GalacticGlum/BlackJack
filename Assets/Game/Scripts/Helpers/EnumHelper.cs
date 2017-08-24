using System;
using Random = UnityEngine.Random;

public static class EnumHelper
{
    public static T RandomEnumValue<T>()
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        return values[Random.Range(0, values.Length - 1)];
    }
}