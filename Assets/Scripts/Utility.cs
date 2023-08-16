using System;
using System.Collections;
using System.Collections.Generic;

public static class Utility 
{
    /// <summary>
    /// 重新生成位置
    /// </summary>
    /// <param name="array">要更换位置的数组</param>
    /// <param name="seed">随机种子</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        Random prng = new Random(seed);

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            
            array[randomIndex] = array[i];
            
            array[i] = tempItem;
        }
        return array;
    }
}
