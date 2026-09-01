using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelEditorStub;


namespace LevelEditor
{
    public class PseudoPrefabPlayer : PseudoPrefab
    {
        public override void Setup()
        {
            PseudoPrefabPlayerStub playerStub = (PseudoPrefabPlayerStub)stub;
            PlayerIDProvider playerIDProvider = childGameObject.GetComponent<PlayerIDProvider>();
            playerIDProvider.OverridePlayerId((PlayerInputLookup.Player)playerStub.playerID);

            if (childGameObject.GetComponent<HatMeshVisibility>() != null && playerStub.hatVisState != PseudoPrefabPlayerStub.HatVisState.None)
            {
                childGameObject.GetComponent<HatMeshVisibility>().m_initialVisState = (HatMeshVisibility.VisState)playerStub.hatVisState;
            }

            if (childGameObject.GetComponent<HeldItemsMeshVisibility>() != null && playerStub.knifeVisState != PseudoPrefabPlayerStub.KnifeVisState.None)
            {
                int flag = 24 + new int[] { 1, 1, 2, 128, 256 }[(int)playerStub.knifeVisState];
                childGameObject.GetComponent<HeldItemsMeshVisibility>().m_stateFlags[0] = flag;
            }
        }
    }
}
