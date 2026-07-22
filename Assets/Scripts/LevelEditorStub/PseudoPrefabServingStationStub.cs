using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditorStub
{
	public class PseudoPrefabServingStationStub : PseudoPrefabStub
	{
        [SerializeField] public PseudoPrefabPlateReturnStub plateReturn;
		[SerializeField] public PseudoPrefabPlateReturnStub[] plateReturns;
    }
}