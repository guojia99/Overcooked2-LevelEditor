using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LevelEditor
{
	public class PseudoPrefabCookingUtensil : PseudoPrefab
	{
        public override void Setup()
        {
            PseudoPrefabCookingUtensilStub cookingUtensilStub = (PseudoPrefabCookingUtensilStub)stub;

            IngredientContainer ingredientContainer = childGameObject.GetComponent<IngredientContainer>();
            ingredientContainer.m_capacity = cookingUtensilStub.capacity;

            SpecificPseudoPrefabTag specificPseudoPrefabTag = GetComponent<SpecificPseudoPrefabTag>();
            if (specificPseudoPrefabTag != null && !string.IsNullOrEmpty(specificPseudoPrefabTag.prefabTag))
            {
                if (specificPseudoPrefabTag.prefabTag == "ToastingFork")
                {
                    BoxCollider boxCollider = childGameObject.GetComponents<BoxCollider>()[0];
                    boxCollider.size = new Vector3(1f, 0.1f, 0.4f);
                }
            }

            if (cookingUtensilStub.allowedCookingStationTypes != null &&
                cookingUtensilStub.allowedCookingStationTypes.Length > 0 &&
                childGameObject.GetComponent<CookingHandler>() != null)
            {
                MultiCookingStationTypes multiCookingStationTypes = childGameObject.AddComponent<MultiCookingStationTypes>();
                multiCookingStationTypes.cookingStationTypes = cookingUtensilStub.allowedCookingStationTypes.Select(x => (CookingStationType)x).ToArray();
            }

            var contentsCosmeticDecisions = childGameObject.RequestComponentRecursive<ContentsCosmeticDecisions>();
            if (contentsCosmeticDecisions != null)
            {
                contentsCosmeticDecisions.m_contentsYPositionWhenEmpty = -0.2f;
                contentsCosmeticDecisions.m_prefabLookup = null;
            }

            if (childGameObject.GetComponent<MixableContainer>() != null)
            {
                MixableContainer mixableContainer = childGameObject.GetComponent<MixableContainer>();
                if (cookingUtensilStub.allowedIngredientSOs != null && cookingUtensilStub.allowedIngredientSOs.Length > 0)
                {
                    mixableContainer.m_ApprovedIngredients = cookingUtensilStub.allowedIngredientSOs
                        .Select(x => RecipeHelper.GetIngredientOrderNode(x))
                        .ToArray();
                }
            }

            else if (cookingUtensilStub.allowedIngredientSOs != null && cookingUtensilStub.allowedIngredientSOs.Length > 0)
            {
                CookableContainer cookableContainer = childGameObject.GetComponent<CookableContainer>();
                OrderToPrefabLookup oldLookup = cookableContainer.m_approvedContentsList;
                OrderToPrefabLookup newLookup = ScriptableObject.Instantiate(oldLookup);

                var oldLookupArray = (OrderToPrefabLookup.ContentPrefabLookup[])oldLookup.GetType()
                    .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(oldLookup);
                GameObject m_prefab_default = oldLookupArray[0].m_prefab;
                List<OrderToPrefabLookup.ContentPrefabLookup> allowedIngredients = new List<OrderToPrefabLookup.ContentPrefabLookup>();

                for (int i = 0; i < cookingUtensilStub.allowedIngredientSOs.Length; i++)
                {
                    PseudoPrefabSO ingredientSO = cookingUtensilStub.allowedIngredientSOs[i];
                    if (ingredientSO is PseudoPrefabSORecipe)
                    {
                        OrderDefinitionNode orderDefinitionNode = PseudoPrefabManager.LoadAsset<OrderDefinitionNode>(ingredientSO);
                        GameObject prefab;
                        if (i < cookingUtensilStub.models.Length && cookingUtensilStub.models[i] != null)
                            prefab = cookingUtensilStub.models[i];
                        else if (i < cookingUtensilStub.modelSOs.Length && cookingUtensilStub.modelSOs[i] != null)
                            prefab = PseudoPrefabManager.LoadAsset(cookingUtensilStub.modelSOs[i]);
                        else
                        {
                            int index = oldLookupArray.FindIndex_Predicate(x => x.m_content.Equals(orderDefinitionNode));
                            if (index >= 0)
                                prefab = oldLookupArray[index].m_prefab;
                            else
                                prefab = m_prefab_default;
                        }
                        allowedIngredients.Add(new OrderToPrefabLookup.ContentPrefabLookup()
                        {
                            m_content = orderDefinitionNode,
                            m_prefab = prefab,
                        });
                    }
                    else
                    {
                        GameObject ingredient = PseudoPrefabManager.LoadAsset<GameObject>(ingredientSO);
                        while (ingredient.GetComponent<WorkableItem>() != null)
                            ingredient = ingredient.GetComponent<WorkableItem>().m_nextPrefab;
                        IngredientPropertiesComponent ingredientPropertiesComponent = ingredient.GetComponent<IngredientPropertiesComponent>();
                        IngredientOrderNode ingredientOrderNode = (IngredientOrderNode)ingredientPropertiesComponent.GetType()
                            .GetField("m_ingredientOrderNode", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                            .GetValue(ingredientPropertiesComponent);
                        // find item in old lookup to use its model prefab
                        var allLookup = oldLookupArray.Where(y =>
                        {
                            if (y.m_content.Equals(ingredientOrderNode)) return true;
                            if (y.m_content is CookedCompositeOrderNode)
                            {
                                CookedCompositeOrderNode cookedCompositeOrderNode = (CookedCompositeOrderNode)y.m_content;
                                return cookedCompositeOrderNode.m_composition.Length == 1 && cookedCompositeOrderNode.m_composition[0].Equals(ingredientOrderNode);
                            }
                            return false;
                        });
                        if (allLookup.Any())
                        {
                            allowedIngredients.AddRange(allLookup);
                        }
                        else
                        {
                            allowedIngredients.Add(new OrderToPrefabLookup.ContentPrefabLookup()
                            {
                                m_content = ingredientOrderNode,
                                m_prefab = m_prefab_default,
                            });
                        }
                    }
                }

                newLookup.GetType()
                    .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                    .SetValue(newLookup, allowedIngredients.ToArray());
                cookableContainer.m_approvedContentsList = newLookup;
                if (cookableContainer.m_cosmeticsPrefab != null)
                {
                    if (cookableContainer.m_cosmeticsPrefab.GetComponent<OverlapModelsMealDecisions>() != null)
                    {
                        GameObject cosmeticsPrefab = RuntimePrefabManager.CloneAsInactivePrefab(cookableContainer.m_cosmeticsPrefab);
                        typeof(OverlapModelsMealDecisions)
                            .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                            .SetValue(cosmeticsPrefab.GetComponent<OverlapModelsMealDecisions>(), newLookup);
                        cookableContainer.m_cosmeticsPrefab = cosmeticsPrefab;
                    }
                }
            }
        }
    }
}