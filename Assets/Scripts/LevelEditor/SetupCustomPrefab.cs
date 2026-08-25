using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditor
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-99)]
    public class SetupCustomPrefab : MonoBehaviour
    {
        protected SetupCustomPrefabStub stub;

        protected virtual void Awake()
        {
            stub = GetComponent<SetupCustomPrefabStub>();
        }

        private void Start()
        {
            if (PseudoPrefabManager.Instance.GameEditState == GameEditState.Edit)
                Setup();
        }

        public virtual void Setup()
        {
            stub = GetComponent<SetupCustomPrefabStub>();
        }

        public virtual void Clear()
        {
        }
    }
}