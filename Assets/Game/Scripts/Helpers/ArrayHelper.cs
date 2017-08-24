using UnityEngine;

public static class ArrayHelper
{
    public static T Choose<T>(this T[] array) => array[Random.Range(0, array.Length - 1)];
}