using LevelEditorStub;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LevelEditor
{
    public static class RecipeHelper
    {
        public static RecipeList.Entry GetRecipe(ScriptableObject recipeSO)
        {
            if (recipeSO is PseudoPrefabSORecipe)
            {
                PseudoPrefabSORecipe x = (PseudoPrefabSORecipe)recipeSO;
                var entry = new RecipeList.Entry();
                entry.m_weight = 1f; // this is unused in the game
                entry.m_scoreForMeal = x.score;
                entry.m_order = PseudoPrefabManager.LoadAsset<OrderDefinitionNode>(x);
                return entry;
            }
            else if (recipeSO is CustomRecipeSO)
            {
                CustomRecipeSO customRecipeSO = (CustomRecipeSO)recipeSO;
                var entry = new RecipeList.Entry();
                entry.m_weight = 1f; // this is unused in the game
                entry.m_scoreForMeal = customRecipeSO.score;
                entry.m_order = GetCustomRecipeOrderDefinitionNode(customRecipeSO);
                entry.m_order.m_orderGuiDescription = GetCustomRecipeGuiDescription(customRecipeSO, entry.m_order);
                return entry;
            }
            else return new RecipeList.Entry();
        }

        public static OrderDefinitionNode GetCustomRecipeOrderDefinitionNode(CustomRecipeSO customRecipeSO)
        {
            FixOldCustomRecipeSO(customRecipeSO);

            OrderDefinitionNode recipe;
            switch (customRecipeSO.type)
            {
                case CustomRecipeSO.RecipeType.Composite:
                    recipe = ScriptableObject.CreateInstance<CompositeOrderNode>();
                    break;
                case CustomRecipeSO.RecipeType.Cooked:
                    recipe = ScriptableObject.CreateInstance<CookedCompositeOrderNode>();
                    break;
                case CustomRecipeSO.RecipeType.Mixed:
                    recipe = ScriptableObject.CreateInstance<MixedCompositeOrderNode>();
                    break;
                default:
                    recipe = ScriptableObject.CreateInstance<WildcardOrderNode>();
                    break;
            }

            if (customRecipeSO.type == CustomRecipeSO.RecipeType.Composite ||
                customRecipeSO.type == CustomRecipeSO.RecipeType.Cooked ||
                customRecipeSO.type == CustomRecipeSO.RecipeType.Mixed)
            {
                CompositeOrderNode compositeOrderNode = recipe as CompositeOrderNode;
                if (customRecipeSO.compositionSOs != null)
                {
                    compositeOrderNode.m_composition = customRecipeSO.compositionSOs
                        .Select(x => GetCustomRecipeOrIngredientNode(x)).ToArray();
                }
                if (customRecipeSO.optionalSOs != null)
                {
                    compositeOrderNode.m_optional = customRecipeSO.optionalSOs
                        .Select(x => GetCustomRecipeOrIngredientNode(x)).ToArray();
                }
            }

            if (customRecipeSO.type == CustomRecipeSO.RecipeType.Cooked)
            {
                CookedCompositeOrderNode cookedCompositeOrderNode = recipe as CookedCompositeOrderNode;
                cookedCompositeOrderNode.m_cookingStep = PseudoPrefabManager.LoadAsset<CookingStepData>(customRecipeSO.cookingStepSO);
                cookedCompositeOrderNode.m_progress = (CookedCompositeOrderNode.CookingProgress)customRecipeSO.cookingProgress;
            }

            if (customRecipeSO.type == CustomRecipeSO.RecipeType.Mixed)
            {
                MixedCompositeOrderNode mixedCompositeOrderNode = recipe as MixedCompositeOrderNode;
                mixedCompositeOrderNode.m_progress = (MixedCompositeOrderNode.MixingProgress)customRecipeSO.mixingProgress;
            }

            recipe.name = customRecipeSO.recipeName;
            recipe.m_uID = customRecipeSO.uID;
            recipe.m_platingStep = customRecipeSO.platingStepSO != null ? PseudoPrefabManager.LoadAsset<PlatingStepData>(customRecipeSO.platingStepSO) : null;
            recipe.m_platingPrefab = customRecipeSO.GetModel();
            return recipe;
        }

        private static RecipeWidgetUIController.RecipeTileData[] GetCustomRecipeGuiDescription(CustomRecipeSO customRecipeSO, OrderDefinitionNode recipe)
        {
            FixOldCustomRecipeSO(customRecipeSO);

            RecipeWidgetUIController.RecipeTileData gui0 = new RecipeWidgetUIController.RecipeTileData();
            gui0.m_tileDefinition = new RecipeWidgetTile.TileDefinition();
            gui0.m_tileDefinition.m_mainPictures = new List<Sprite> { customRecipeSO.GetIcon() };

            switch (customRecipeSO.type)
            {
                case CustomRecipeSO.RecipeType.Composite:
                    RecipeWidgetUIController.RecipeTileData[] children = new RecipeWidgetUIController.RecipeTileData[customRecipeSO.compositionSOs.Length];
                    CompositeOrderNode compositeOrderNode = recipe as CompositeOrderNode;
                    for (int i = 0; i < customRecipeSO.compositionSOs.Length; i++)
                    {
                        children[i] = new RecipeWidgetUIController.RecipeTileData();
                        children[i].m_tileDefinition = GetCustomRecipeTileDefinition(customRecipeSO.compositionSOs[i], compositeOrderNode.m_composition[i]);
                    }
                    gui0.m_children = Enumerable.Range(1, children.Length).ToList();
                    return new List<RecipeWidgetUIController.RecipeTileData> { gui0 }.Concat(children).ToArray();
                case CustomRecipeSO.RecipeType.Cooked:
                case CustomRecipeSO.RecipeType.Mixed:
                    RecipeWidgetUIController.RecipeTileData gui1 = new RecipeWidgetUIController.RecipeTileData();
                    gui1.m_tileDefinition = GetCustomRecipeTileDefinition(customRecipeSO, recipe);
                    gui0.m_children = new List<int> { 1 };
                    return new RecipeWidgetUIController.RecipeTileData[] { gui0, gui1 };
                default:
                    return new RecipeWidgetUIController.RecipeTileData[] { gui0 };
            }
        }

        private static List<Sprite> GetCustomRecipeSpriteList(CustomRecipeSO customRecipeSO, OrderDefinitionNode recipe)
        {
            List<Sprite> list = new List<Sprite>();
            if (customRecipeSO.type == CustomRecipeSO.RecipeType.Null || !(recipe is CompositeOrderNode)) return list;
            CompositeOrderNode compositeOrderNode = recipe as CompositeOrderNode;
            for (int i = 0; i < customRecipeSO.compositionSOs.Length; i++)
            {
                if (customRecipeSO.compositionSOs[i] is CustomRecipeSO)
                    list.AddRange(GetCustomRecipeSpriteList(customRecipeSO.compositionSOs[i] as CustomRecipeSO, compositeOrderNode.m_composition[i]));
                else if (customRecipeSO.compositionSOs[i] is PseudoPrefabSO && compositeOrderNode.m_composition[i] is IngredientOrderNode)
                    list.Add((compositeOrderNode.m_composition[i] as IngredientOrderNode).m_iconSprite);
            }
            if (customRecipeSO.type == CustomRecipeSO.RecipeType.Cooked)
                list.Add(customRecipeSO.GetCookingStepIcon());
            else if (customRecipeSO.type == CustomRecipeSO.RecipeType.Mixed)
                list.Add(customRecipeSO.GetMixingIcon());
            return list;
        }

        private static RecipeWidgetTile.TileDefinition GetCustomRecipeTileDefinition(ScriptableObject recipeSO, OrderDefinitionNode recipe)
        {
            RecipeWidgetTile.TileDefinition tileDefinition = new RecipeWidgetTile.TileDefinition();
            if (recipeSO is PseudoPrefabSO)
            {
                tileDefinition.m_mainPictures = new List<Sprite> { (recipe as IngredientOrderNode).m_iconSprite };
            }
            else if (recipeSO is CustomRecipeSO)
            {
                CustomRecipeSO customRecipeSO = recipeSO as CustomRecipeSO;
                List<Sprite> sprites = GetCustomRecipeSpriteList(customRecipeSO, recipe);
                int cookingStepCount = 0;
                CustomRecipeSO cur = customRecipeSO;
                while (cur != null && (cur.type == CustomRecipeSO.RecipeType.Cooked || cur.type == CustomRecipeSO.RecipeType.Mixed))
                {
                    cookingStepCount++;
                    if (cur.compositionSOs.Length > 1)
                        break;
                    cur = cur.compositionSOs[0] as CustomRecipeSO;
                }
                tileDefinition.m_mainPictures = sprites.GetRange(0, sprites.Count - cookingStepCount);
                tileDefinition.m_modifierPictures = sprites.GetRange(sprites.Count - cookingStepCount, cookingStepCount);
            }

            return tileDefinition;
        }

        public static OrderDefinitionNode GetOptionalRecipeNode(ScriptableObject recipeSO)
        {
            if (recipeSO is PseudoPrefabSO)
                return PseudoPrefabManager.LoadAsset<OrderDefinitionNode>(recipeSO as PseudoPrefabSO);

            CustomRecipeSO customRecipeSO = recipeSO as CustomRecipeSO;
            if (customRecipeSO == null) return null;
            FixOldCustomRecipeSO(customRecipeSO);
            OrderDefinitionNode recipe;

            if (customRecipeSO is CustomRecipeOptionalBurgerSO)
            {
                CustomRecipeOptionalBurgerSO optionalBurgerSO = (CustomRecipeOptionalBurgerSO)customRecipeSO;
                CompositeOrderNode node = ScriptableObject.CreateInstance<CompositeOrderNode>();
                node.m_composition = new OrderDefinitionNode[0];
                node.m_optional = optionalBurgerSO.optionalSOs
                    .Where(x => x != optionalBurgerSO.bunSO)
                    .Select(x => GetCustomRecipeOrIngredientNode(x))
                    .Concat(new OrderDefinitionNode[] { GetIngredientOrderNode(optionalBurgerSO.bunSO) })
                    .ToArray();

                GameObject platingPrefabAsset = PseudoPrefabManager.LoadAsset(customRecipeSO.modelSO);
                GameObject platingPrefab = RuntimePrefabManager.CloneAsInactivePrefab(platingPrefabAsset);
                BurgerBunCosmeticDecisions burgerCosmetic = platingPrefab.GetComponent<BurgerBunCosmeticDecisions>();
                BurritoCosmeticDecisions burritoCosmetic = platingPrefab.GetComponent<BurritoCosmeticDecisions>();
                if (burgerCosmetic != null)
                {
                    OrderToPrefabLookup oldLookup = (OrderToPrefabLookup)typeof(BurgerBunCosmeticDecisions)
                        .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(burgerCosmetic);
                    OrderToPrefabLookup lookup = GetOrderToPrefabLookupBurger(optionalBurgerSO, oldLookup);
                    typeof(BurgerBunCosmeticDecisions)
                        .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                        .SetValue(burgerCosmetic, lookup);
                }
                else if (burritoCosmetic != null)
                {
                    OrderToPrefabLookup oldLookup = (OrderToPrefabLookup)typeof(OverlapModelsMealDecisions)
                        .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                        .GetValue(burritoCosmetic);
                    OrderToPrefabLookup lookup = GetOrderToPrefabLookupBurger(optionalBurgerSO, oldLookup);
                    typeof(OverlapModelsMealDecisions)
                        .GetField("m_prefabLookup", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                        .SetValue(burritoCosmetic, lookup);
                }

                node.m_platingPrefab = platingPrefab;
                recipe = node;
            }
            else if (customRecipeSO is CustomRecipeOptionalPizzaSO)
            {
                CustomRecipeOptionalPizzaSO optionalPizzaSO = (CustomRecipeOptionalPizzaSO)customRecipeSO;
                CookedCompositeOrderNode node = ScriptableObject.CreateInstance<CookedCompositeOrderNode>();
                node.m_cookingStep = PseudoPrefabManager.LoadAsset<CookingStepData>(optionalPizzaSO.cookingStepSO);
                node.m_progress = optionalPizzaSO.cooked ? CookedCompositeOrderNode.CookingProgress.Cooked : CookedCompositeOrderNode.CookingProgress.Raw;
                node.m_composition = new OrderDefinitionNode[] { GetIngredientOrderNode(optionalPizzaSO.doughSO) };
                node.m_optional = optionalPizzaSO.optionalSOs.Select(x => GetCustomRecipeOrIngredientNode(x)).ToArray();

                GameObject platingPrefabAsset = PseudoPrefabManager.LoadAsset(customRecipeSO.modelSO);
                GameObject platingPrefab = RuntimePrefabManager.CloneAsInactivePrefab(platingPrefabAsset);
                PizzaCosmeticDecisions pizzaCosmetic = platingPrefab.GetComponent<PizzaCosmeticDecisions>();
                OrderToPrefabLookup uncookedLookup = GetOrderToPrefabLookupPizza(optionalPizzaSO, false, pizzaCosmetic);
                OrderToPrefabLookup cookedLookup = GetOrderToPrefabLookupPizza(optionalPizzaSO, true, pizzaCosmetic);
                pizzaCosmetic.GetType()
                    .GetField("m_uncookedPrefabLookup", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                    .SetValue(pizzaCosmetic, uncookedLookup);
                pizzaCosmetic.GetType()
                    .GetField("m_cookedPrefabLookup", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                    .SetValue(pizzaCosmetic, cookedLookup);

                node.m_platingPrefab = platingPrefab;
                recipe = node;
            }
            else
            {
                recipe = GetCustomRecipeOrderDefinitionNode(customRecipeSO);
            }

            recipe.name = customRecipeSO.recipeName;
            recipe.m_uID = customRecipeSO.uID;
            recipe.m_platingStep =
                customRecipeSO.platingStepSO != null ?
                PseudoPrefabManager.LoadAsset<PlatingStepData>(customRecipeSO.platingStepSO) : null;

            return recipe;
        }

        public static OrderToPrefabLookup GetOrderToPrefabLookupBurger(CustomRecipeOptionalBurgerSO optionalBurgerSO, OrderToPrefabLookup oldLookup)
        {
            OrderToPrefabLookup lookup = GetOrderToPrefabLookup(
                optionalBurgerSO.recipeName, 
                optionalBurgerSO.optionalSOs, 
                optionalBurgerSO.ingredientModels, optionalBurgerSO.ingredientModelSOs, oldLookup);
            return lookup;
        }

        public static OrderToPrefabLookup GetOrderToPrefabLookupPizza(CustomRecipeOptionalPizzaSO optionalPizzaSO, bool cooked, PizzaCosmeticDecisions pizzaCosmeticDecisions)
        {
            var models = cooked ? optionalPizzaSO.cookedPizzaIngredientPrefabs : optionalPizzaSO.rawPizzaIngredientPrefabs;
            var modelSOs = cooked ? optionalPizzaSO.cookedPizzaIngredientPrefabSOs : optionalPizzaSO.rawPizzaIngredientPrefabSOs;
            OrderToPrefabLookup oldLookup = (OrderToPrefabLookup)pizzaCosmeticDecisions.GetType()
                .GetField(cooked ? "m_cookedPrefabLookup" : "m_uncookedPrefabLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(pizzaCosmeticDecisions);
            OrderToPrefabLookup lookup = GetOrderToPrefabLookup(
                optionalPizzaSO.recipeName, 
                optionalPizzaSO.optionalSOs, 
                models, modelSOs, oldLookup);
            return lookup;
        }

        public static OrderToPrefabLookup GetOrderToPrefabLookup(string recipeName, ScriptableObject[] optionalSOs, GameObject[] models, PseudoPrefabSO[] modelSOs, OrderToPrefabLookup oldLookup)
        {
            var indices = Enumerable.Range(0, optionalSOs.Length);
            OrderDefinitionNode[] orderDefinitionNodes = indices.Select(
                i => GetCustomRecipeOrIngredientNode(optionalSOs[i])).ToArray();
            OrderToPrefabLookup.ContentPrefabLookup[] oldLookupArray = new OrderToPrefabLookup.ContentPrefabLookup[0];
            if (oldLookup != null)
            {
                oldLookupArray = (OrderToPrefabLookup.ContentPrefabLookup[])oldLookup.GetType()
                    .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(oldLookup);
            }
            GameObject[] realModels = indices.Select(i =>
            {
                if (i < models.Length && models[i] != null)
                    return models[i];
                if (i < modelSOs.Length && modelSOs[i] != null)
                    return PseudoPrefabManager.LoadAsset(modelSOs[i]);
                int index = oldLookupArray.FindIndex_Predicate(x => x.m_content.Equals(orderDefinitionNodes[i]));
                if (index >= 0)
                    return oldLookupArray[index].m_prefab;
                else
                    return null;
            }).ToArray();
            OrderToPrefabLookup lookup = GetOrderToPrefabLookup(orderDefinitionNodes, realModels);
            lookup.name = "Lookup_" + recipeName;
            return lookup;
        }

        private static OrderToPrefabLookup GetOrderToPrefabLookup(OrderDefinitionNode[] orderDefinitionNodes, GameObject[] models)
        {
            OrderToPrefabLookup lookup = ScriptableObject.CreateInstance<OrderToPrefabLookup>();
            Dictionary<OrderDefinitionNode, OrderToPrefabLookup.ContentPrefabLookup> lookupDic = new Dictionary<OrderDefinitionNode, OrderToPrefabLookup.ContentPrefabLookup>();
            for (int i = 0; i < orderDefinitionNodes.Length; i++)
            {
                OrderDefinitionNode ingredientNode = orderDefinitionNodes[i];
                OrderDefinitionNode matchKey = lookupDic.Keys.FirstOrDefault(key => key.Equals(ingredientNode));
                if (matchKey != null)
                {
                    lookupDic[matchKey].m_amountAllowed += 1;
                }
                else
                {
                    GameObject prefab = models[i];
                    lookupDic[ingredientNode] = new OrderToPrefabLookup.ContentPrefabLookup
                    {
                        m_content = ingredientNode,
                        m_prefab = prefab,
                        m_amountAllowed = 1,
                    };
                }
            }
            lookup.GetType()
                .GetField("m_lookupArray", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(lookup, lookupDic.Values.ToArray());
            return lookup;
        }

        public static IngredientOrderNode GetIngredientOrderNode(PseudoPrefabSO pseudoPrefabSO)
        {
            GameObject ingredient = PseudoPrefabManager.LoadAsset<GameObject>(pseudoPrefabSO);
            while (ingredient.GetComponent<WorkableItem>() != null)
                ingredient = ingredient.GetComponent<WorkableItem>().m_nextPrefab;
            IngredientPropertiesComponent ingredientPropertiesComponent = ingredient.GetComponent<IngredientPropertiesComponent>();
            if (ingredientPropertiesComponent != null)
            {
                return (IngredientOrderNode)ingredientPropertiesComponent.GetType()
                    .GetField("m_ingredientOrderNode", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(ingredientPropertiesComponent);
            }
            else
            {
                PreparationContainer preparationContainer = ingredient.GetComponent<PreparationContainer>();
                if (preparationContainer != null)
                {
                    return preparationContainer.m_ingredientOrderNode;
                }
                else
                {
                    return null;
                }
            }
        }

        public static OrderDefinitionNode GetCustomRecipeOrIngredientNode(ScriptableObject recipeSO)
        {
            return recipeSO is CustomRecipeSO ? 
                GetCustomRecipeOrderDefinitionNode(recipeSO as CustomRecipeSO) : 
                GetIngredientOrderNode(recipeSO as PseudoPrefabSO);
        }

        public static ItemOrderNode GetItemOrderNode(PseudoPrefabSO pseudoPrefabSO)
        {
            GameObject item = PseudoPrefabManager.LoadAsset<GameObject>(pseudoPrefabSO);
            while (item.GetComponent<WorkableItem>() != null)
                item = item.GetComponent<WorkableItem>().m_nextPrefab;
            ItemPropertiesComponent itemPropertiesComponent = item.GetComponent<ItemPropertiesComponent>();
            if (itemPropertiesComponent != null)
            {
                return (ItemOrderNode)itemPropertiesComponent.GetType()
                    .GetField("m_itemDefinition", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(itemPropertiesComponent);
            }
            else
            {
                return null;
            }
        }

        public static OrderDefinitionNode GetIngredientOrItemOrderNode(PseudoPrefabSO pseudoPrefabSO)
        {
            OrderDefinitionNode ingredientOrderNode = GetIngredientOrderNode(pseudoPrefabSO);
            return ingredientOrderNode ?? GetItemOrderNode(pseudoPrefabSO);
        }

        public static CookingStepData GetCookingStepData(PseudoPrefabSO pseudoPrefabSO)
        {
            return PseudoPrefabManager.LoadAsset<CookingStepData>(pseudoPrefabSO);
        }

        public static GameObject GetModel(this CustomRecipeSO customRecipeSO)
        {
            return 
                customRecipeSO.model != null ? customRecipeSO.model :
                customRecipeSO.modelSO != null ? PseudoPrefabManager.LoadAsset(customRecipeSO.modelSO) : null;
        }

        public static Sprite GetIcon(this CustomRecipeSO customRecipeSO)
        {
            return 
                customRecipeSO.icon != null ? customRecipeSO.icon :
                customRecipeSO.iconSO != null ? PseudoPrefabManager.LoadSpriteSubAsset(customRecipeSO.iconSO) : null;
        }

        public static Sprite GetCookingStepIcon(this CustomRecipeSO customRecipeSO)
        {
            return 
                customRecipeSO.cookingStepIcon != null ? customRecipeSO.cookingStepIcon :
                customRecipeSO.cookingStepIconSO != null ? PseudoPrefabManager.LoadSpriteSubAsset(customRecipeSO.cookingStepIconSO) : null;
        }

        public static Sprite GetMixingIcon(this CustomRecipeSO customRecipeSO)
        {
            return
                customRecipeSO.mixingIcon != null ? customRecipeSO.mixingIcon :
                customRecipeSO.mixingIconSO != null ? PseudoPrefabManager.LoadSpriteSubAsset(customRecipeSO.mixingIconSO) : null;
        }

        private static void FixOldCustomRecipeSO(CustomRecipeSO customRecipeSO)
        {
            if (customRecipeSO.type == CustomRecipeSO.RecipeType.Null)
            {
                if (customRecipeSO.cookingStepSO != null)
                {
                    customRecipeSO.type = CustomRecipeSO.RecipeType.Cooked;
                    customRecipeSO.cookingProgress = CustomRecipeSO.CookingProgress.Cooked;
                }
                else
                {
                    customRecipeSO.type = CustomRecipeSO.RecipeType.Composite;
                }
                if (customRecipeSO is CustomRecipeOptionalPizzaSO)
                {
                    CustomRecipeOptionalPizzaSO pizzaSO = (CustomRecipeOptionalPizzaSO)customRecipeSO;
                    customRecipeSO.cookingProgress = pizzaSO.cooked ? CustomRecipeSO.CookingProgress.Cooked : CustomRecipeSO.CookingProgress.Raw;
                    int doughIndex = pizzaSO.compositionSOs.FindIndex_Predicate(x => x == pizzaSO.doughSO);
                    int length = pizzaSO.compositionSOs.Length;
                    if (doughIndex >= 0 &&
                        pizzaSO.rawPizzaIngredientPrefabs.Length == length &&
                        pizzaSO.rawPizzaIngredientPrefabSOs.Length == length &&
                        pizzaSO.cookedPizzaIngredientPrefabs.Length == length &&
                        pizzaSO.cookedPizzaIngredientPrefabSOs.Length == length)
                    {
                        var indices = Enumerable.Range(0, length).Where(i => i != doughIndex);
                        pizzaSO.optionalSOs = indices.Select(i => pizzaSO.compositionSOs[i]).ToArray();
                        pizzaSO.rawPizzaIngredientPrefabs = indices.Select(i => pizzaSO.rawPizzaIngredientPrefabs[i]).ToArray();
                        pizzaSO.rawPizzaIngredientPrefabSOs = indices.Select(i => pizzaSO.rawPizzaIngredientPrefabSOs[i]).ToArray();
                        pizzaSO.cookedPizzaIngredientPrefabs = indices.Select(i => pizzaSO.cookedPizzaIngredientPrefabs[i]).ToArray();
                        pizzaSO.cookedPizzaIngredientPrefabSOs = indices.Select(i => pizzaSO.cookedPizzaIngredientPrefabSOs[i]).ToArray();
                        pizzaSO.compositionSOs = new ScriptableObject[] { pizzaSO.doughSO };
                    }
                }
            }
        }

        public static bool IsEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }
    }
}
