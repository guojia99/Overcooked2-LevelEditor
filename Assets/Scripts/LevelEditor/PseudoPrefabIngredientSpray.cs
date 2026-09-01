using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditor
{
    public class PseudoPrefabIngredientSpray : PseudoPrefab
    {
        public override void Setup()
        {
            PseudoPrefabIngredientSprayStub ingredientSprayStub = (PseudoPrefabIngredientSprayStub)stub;
            IngredientSpray ingredientSpray = childGameObject.GetComponent<IngredientSpray>();
            ingredientSpray.m_OrderPrefab = PseudoPrefabManager.LoadAsset(ingredientSprayStub.orderPrefabSO);
        }
    }
}