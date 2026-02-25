using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChar", menuName = "Chars/New Character")]
public class charSO : ScriptableObject
{
    public int hp;
    public Ability[] abilities;

    public Sprite frontsprite;
    public Sprite backsprite;
}