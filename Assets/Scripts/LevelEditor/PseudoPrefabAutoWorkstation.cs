using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditor
{
    public class PseudoPrefabAutoWorkstation : PseudoPrefab
    {
        public override void Setup()
        {
            PseudoPrefabAutoWorkstationStub autoWorkstationStub = (PseudoPrefabAutoWorkstationStub)stub;

            foreach (var componet in gameObject.GetComponents<TriggerOnObject>())
            {
                componet.m_targetObject = childGameObject;
            }

            childGameObject.GetComponent<AutoWorkstation>().m_workFinishedTarget = autoWorkstationStub.workFinishedTarget;
        }

        public override void Cleanup()
        {
            foreach (var componet in gameObject.GetComponents<TriggerOnObject>())
            {
                componet.m_targetObject = null;
            }
        }
    }
}