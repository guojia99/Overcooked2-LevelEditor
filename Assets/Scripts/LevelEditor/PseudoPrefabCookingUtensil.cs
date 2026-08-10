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

            if (!cookingUtensilStub.allowedCookingStationTypes.IsEmpty() &&
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

            // griddlepan's m_approvedContentsList is originally null
            CookableContainer cookableContainer = childGameObject.GetComponent<CookableContainer>();
            MixableContainer mixableContainer = childGameObject.GetComponent<MixableContainer>();
            if (cookableContainer != null && cookableContainer.m_cosmeticsPrefab != null)
            {
                GriddlePanCosmeticDecisions griddlePanCosmeticDecisions = cookableContainer.m_cosmeticsPrefab.GetComponent<GriddlePanCosmeticDecisions>();
                if (griddlePanCosmeticDecisions != null)
                {
                    cookableContainer.m_approvedContentsList = (OrderToPrefabLookup)typeof(OverlapModelsMealDecisions)
                        .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(griddlePanCosmeticDecisions);
                }
            }
            
            if (!cookingUtensilStub.allowedIngredientSOs.IsEmpty())
            {
                OrderDefinitionNode[] orderDefinitionNodes = cookingUtensilStub.allowedIngredientSOs.Select(x =>
                {
                    if (x is PseudoPrefabSORecipe)
                        return PseudoPrefabManager.LoadAsset<OrderDefinitionNode>(x as PseudoPrefabSORecipe);
                    else if (x is CustomRecipeSO)
                        return RecipeHelper.GetOrderDefinitionNodeCustomRecipe(x as CustomRecipeSO);
                    else if (x is PseudoPrefabSO)
                        return RecipeHelper.GetIngredientOrderNode(x as PseudoPrefabSO);
                    else return null;
                }).ToArray();
                GameObject[] models = cookingUtensilStub.models;
                PseudoPrefabSO[] modelSOs = cookingUtensilStub.modelSOs;

                if (mixableContainer != null)
                {
                    CookingHandler cookingHandler = childGameObject.GetComponent<CookingHandler>();
                    List<int> ingredientIndexList = new List<int>();
                    List<int> cookedIndexList = new List<int>();
                    for (int i = 0; i < orderDefinitionNodes.Length; i++)
                    {
                        if (cookingHandler != null &&
                            orderDefinitionNodes[i] is CookedCompositeOrderNode &&
                            (orderDefinitionNodes[i] as CookedCompositeOrderNode).m_cookingStep.m_uID == cookingHandler.m_cookingType.m_uID)
                        {
                            cookedIndexList.Add(i);
                        }
                        else
                        {
                            ingredientIndexList.Add(i);
                        }
                    }
                    if (ingredientIndexList.Count > 0)
                    {
                        mixableContainer.m_ApprovedIngredients = ingredientIndexList.Select(i => orderDefinitionNodes[i]).ToArray();
                    }
                    orderDefinitionNodes = cookedIndexList.Select(i => orderDefinitionNodes[i]).ToArray();
                    models = cookedIndexList.Select(i => i < models.Length ? models[i] : null).ToArray();
                    modelSOs = cookedIndexList.Select(i => i < modelSOs.Length ? modelSOs[i] : null).ToArray();
                }

                if (cookableContainer != null && !orderDefinitionNodes.IsEmpty())
                {
                    OrderToPrefabLookup oldLookup = cookableContainer.m_approvedContentsList;
                    OrderToPrefabLookup newLookup = ScriptableObject.CreateInstance<OrderToPrefabLookup>();
                    newLookup.name = "Lookup_" + gameObject.name;
                    OrderToPrefabLookup.ContentPrefabLookup[] oldLookupArray = (OrderToPrefabLookup.ContentPrefabLookup[])oldLookup.GetType()
                        .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                        .GetValue(oldLookup);
                    GameObject m_prefab_default = oldLookupArray[0].m_prefab;
                    List<OrderToPrefabLookup.ContentPrefabLookup> allowedIngredients = new List<OrderToPrefabLookup.ContentPrefabLookup>();

                    for (int i = 0; i < orderDefinitionNodes.Length; i++)
                    {
                        GameObject prefab;
                        if (i < models.Length && models[i] != null)
                            prefab = models[i];
                        else if (i < modelSOs.Length && modelSOs[i] != null)
                            prefab = PseudoPrefabManager.LoadAsset(modelSOs[i]);
                        else
                        {
                            int index = oldLookupArray.FindIndex_Predicate(x => x.m_content.Equals(orderDefinitionNodes[i]));
                            if (index >= 0)
                                prefab = oldLookupArray[index].m_prefab;
                            else
                                prefab = m_prefab_default;
                        }
                        allowedIngredients.Add(new OrderToPrefabLookup.ContentPrefabLookup()
                        {
                            m_content = orderDefinitionNodes[i],
                            m_prefab = prefab,
                        });
                    }

                    newLookup.GetType()
                        .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                        .SetValue(newLookup, allowedIngredients.ToArray());
                    cookableContainer.m_approvedContentsList = newLookup;

                    if (cookableContainer.m_cosmeticsPrefab != null)
                    {
                        if (cookableContainer.m_cosmeticsPrefab.RequestComponentRecursive<MixerComboCosmeticDecisions>() != null)
                        {
                            GameObject cosmeticsPrefab = RuntimePrefabManager.CloneAsInactivePrefab(cookableContainer.m_cosmeticsPrefab);
                            MixerComboCosmeticDecisions mixerComboCosmeticDecisions = cosmeticsPrefab.RequestComponentRecursive<MixerComboCosmeticDecisions>();
                            ComboOrderToPrefabLookup comboOrderToPrefabLookup = ScriptableObject.CreateInstance<ComboOrderToPrefabLookup>();
                            comboOrderToPrefabLookup.name = newLookup.name;
                            ComboOrderToPrefabLookup.ContentPrefabLookup[] contentPrefabLookups = allowedIngredients
                                .Select(x => new ComboOrderToPrefabLookup.ContentPrefabLookup()
                                {
                                    m_content = new OrderDefinitionNode[] { x.m_content },
                                    m_prefab = x.m_prefab,
                                }).ToArray();
                            comboOrderToPrefabLookup.GetType()
                                .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.NonPublic)
                                .SetValue(comboOrderToPrefabLookup, contentPrefabLookups);
                            typeof(ComboCosmeticDecisions)
                                .GetField("m_comboPrefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                                .SetValue(mixerComboCosmeticDecisions, comboOrderToPrefabLookup);
                            cookableContainer.m_cosmeticsPrefab = cosmeticsPrefab;
                        }
                        else if (cookableContainer.m_cosmeticsPrefab.GetComponent<OverlapModelsMealDecisions>() != null)
                        {
                            GameObject cosmeticsPrefab = RuntimePrefabManager.CloneAsInactivePrefab(cookableContainer.m_cosmeticsPrefab);
                            typeof(OverlapModelsMealDecisions)
                                .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                                .SetValue(cosmeticsPrefab.GetComponent<OverlapModelsMealDecisions>(), newLookup);
                            cookableContainer.m_cosmeticsPrefab = cosmeticsPrefab;
                        }
                        else if (cookableContainer.m_cosmeticsPrefab.GetComponent<SkewerCosmeticDecisions>() != null)
                        {
                            List<OrderToPrefabLookup.ContentPrefabLookup> uncookedIngredients = new List<OrderToPrefabLookup.ContentPrefabLookup>();
                            List<OrderToPrefabLookup.ContentPrefabLookup> cookedIngredients = new List<OrderToPrefabLookup.ContentPrefabLookup>();
                            CookingStepData cookingStepData = childGameObject.GetComponent<CookingHandler>().m_cookingType;
                            foreach (var ingredient in allowedIngredients)
                            {
                                CookedCompositeOrderNode cookedCompositeOrderNode = ingredient.m_content as CookedCompositeOrderNode;
                                if (cookedCompositeOrderNode != null &&
                                    cookedCompositeOrderNode.m_cookingStep.m_uID == cookingStepData.m_uID &&
                                    cookedCompositeOrderNode.m_composition != null &&
                                    cookedCompositeOrderNode.m_composition.Length == 1)
                                {
                                    cookedIngredients.Add(new OrderToPrefabLookup.ContentPrefabLookup()
                                    {
                                        m_content = cookedCompositeOrderNode.m_composition[0],
                                        m_prefab = ingredient.m_prefab,
                                    });
                                }
                                else
                                {
                                    uncookedIngredients.Add(ingredient);
                                }
                            }
                            OrderToPrefabLookup uncookedLookup = ScriptableObject.CreateInstance<OrderToPrefabLookup>();
                            uncookedLookup.name = "Lookup_Uncooked_" + gameObject.name;
                            uncookedLookup.GetType()
                                .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                                .SetValue(uncookedLookup, uncookedIngredients.ToArray());
                            OrderToPrefabLookup cookedLookup = ScriptableObject.CreateInstance<OrderToPrefabLookup>();
                            cookedLookup.name = "Lookup_Cooked_" + gameObject.name;
                            cookedLookup.GetType()
                                .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                                .SetValue(cookedLookup, cookedIngredients.ToArray());
                            GameObject cosmeticsPrefab = RuntimePrefabManager.CloneAsInactivePrefab(cookableContainer.m_cosmeticsPrefab);
                            typeof(SkewerCosmeticDecisions)
                                .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                                .SetValue(cosmeticsPrefab.GetComponent<SkewerCosmeticDecisions>(), uncookedLookup);
                            typeof(SkewerCosmeticDecisions)
                                .GetField("m_cookedPrefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                                .SetValue(cosmeticsPrefab.GetComponent<SkewerCosmeticDecisions>(), cookedLookup);
                            cookableContainer.m_cosmeticsPrefab = cosmeticsPrefab;
                        }
                    }
                }
            }
        }
    }
}