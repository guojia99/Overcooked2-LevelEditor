using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditor
{
	public class MultiCookingStationTypes : MonoBehaviour
	{
		public CookingStationType[] cookingStationTypes;

		public bool MatchType(CookingStationType cookingStationType)
		{
			return cookingStationTypes != null && cookingStationTypes.Contains(cookingStationType);
		}
	}
}