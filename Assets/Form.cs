using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tool
{
    public static string GetGameTag(GameTag _value)
    {
        return _value.ToString();
    }
    public static bool IsEnterFirstScene = false;
}

public enum GameTag
{
    None,
    Enemy,
    Player,
    Item
}
