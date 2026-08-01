using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace LevelEditor
{
    public class PseudoPrefabReplaceMaterial : PseudoPrefab
    {
        public override void ResetChild()
        {
            if (stub == null)
                stub = GetComponent<PseudoPrefabStub>();

            Material mat = PseudoPrefabManager.LoadAsset<Material>(stub.pseudoPrefabSO);
            ClearChild();
            if (gameObject.GetComponent<MeshRenderer>() != null)
                gameObject.GetComponent<MeshRenderer>().sharedMaterials = new Material[] { mat };
        }

        public override void Cleanup()
        {
            if (gameObject.GetComponent<MeshRenderer>() != null)
                gameObject.GetComponent<MeshRenderer>().sharedMaterials = new Material[0];
        }
    }
}