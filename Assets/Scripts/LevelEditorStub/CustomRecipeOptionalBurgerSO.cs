using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditorStub
{
    [CreateAssetMenu(menuName = "LevelEditor/CustomRecipeOptionalBurgerSO")]
    public class CustomRecipeOptionalBurgerSO : CustomRecipeSO
    {
        [Header("Optional Burger")]
        [SerializeField] public PseudoPrefabSO bunSO;
        [SerializeField] public int ingredientContainerCapacity;
        [SerializeField] public PseudoPrefabSO[] ingredientModelSOs;
        [SerializeField] public GameObject[] ingredientModels;
    }
}
