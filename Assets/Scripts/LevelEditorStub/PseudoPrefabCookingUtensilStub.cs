using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditorStub
{
	public class PseudoPrefabCookingUtensilStub : PseudoPrefabStub {

        [SerializeField] public int capacity;
		[SerializeField] public PseudoPrefabSO[] allowedIngredientSOs = new PseudoPrefabSO[0];
		[SerializeField] public PseudoPrefabSO[] modelSOs = new PseudoPrefabSO[0];
		[SerializeField] public GameObject[] models = new GameObject[0];
	}
}
