﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New PSI Attack", menuName = "PSI Move")]
public class PsiData : ScriptableObject {

    public string moveName;
    public List<int> moveDamage;
    public List<int> cost;
    public List<string> description;
    public string type;
    public int upgrade;
}
