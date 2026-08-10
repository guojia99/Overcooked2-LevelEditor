using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace LevelEditorStub
{
    [CreateAssetMenu(menuName = "LevelEditor/PseudoPrefabSORecipe", order = -1000)]
    public class PseudoPrefabSORecipe : PseudoPrefabSO
    {
        public int score;
    }
}