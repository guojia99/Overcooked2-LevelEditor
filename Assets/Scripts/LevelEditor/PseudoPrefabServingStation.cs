using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelEditorStub;
using System.Linq;


namespace LevelEditor
{
    public class PseudoPrefabServingStation : PseudoPrefab
    {
        public override void LateSetup()
        {
            PseudoPrefabServingStationStub servingStationStub = (PseudoPrefabServingStationStub)stub;
            List<PlateReturnStation> allReturns = new List<PlateReturnStation>();
            if (servingStationStub.plateReturn != null)
            {
                allReturns.Add(servingStationStub.plateReturn.GetComponent<PseudoPrefab>().childGameObject.GetComponent<PlateReturnStation>());
            }
            if (servingStationStub.plateReturns != null)
            {
                allReturns.AddRange(servingStationStub.plateReturns.Select(x => x.GetComponent<PseudoPrefab>().childGameObject.GetComponent<PlateReturnStation>()));
            }
            childGameObject.GetComponent<PlateStation>().m_returnStations = allReturns.ToArray();
        }
    }
}