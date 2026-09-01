using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditor
{
    public class PseudoPrefabHeatedOven : PseudoPrefab
    {
        public override void LateSetup()
        {
            PseudoPrefabHeatedOvenStub ovenStub = (PseudoPrefabHeatedOvenStub)stub;
            childGameObject.GetComponent<HeatedCookingStation>().m_heatSource = 
                ovenStub.heatedStation.GetComponent<PseudoPrefab>().childGameObject.GetComponent<HeatedStation>();
        }
    }
}