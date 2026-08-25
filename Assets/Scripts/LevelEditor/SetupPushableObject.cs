using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


namespace LevelEditor
{
    public class SetupPushableObject : SetupCustomPrefab
    {
        public override void Setup()
        {
            base.Setup();
            GameObject prefab = PseudoPrefabManager.LoadAsset(stub.pseudoPrefabSOArray.pseudoPrefabSOs[0]);
            ContextualInteractHoverIcon icon = gameObject.GetComponent<ContextualInteractHoverIcon>();
            typeof(ButtonHoverIcon).GetField("m_iconPrefab", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(icon, prefab);
        }

        public override void Clear()
        {
            base.Clear();
            ContextualInteractHoverIcon icon = gameObject.GetComponent<ContextualInteractHoverIcon>();
            typeof(ButtonHoverIcon).GetField("m_iconPrefab", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(icon, null);
        }
    }
}