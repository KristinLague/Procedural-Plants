using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "L-System/RulesObject", order = 1)]
public class Rules : ScriptableObject
{
    public string RuleName;
    public List<Rule> AppliedRules;
}
