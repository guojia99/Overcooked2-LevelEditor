using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditor
{
    public class PseudoPrefabTerminal : PseudoPrefab
    {
        public override void Setup()
        {
            PseudoPrefabTerminalStub terminalStub = (PseudoPrefabTerminalStub)stub;
            childGameObject.GetComponent<Terminal>().m_pilotableObject = terminalStub.pilotableObject.GetComponent<PilotMovement>();
            foreach (var component in childGameObject.GetComponents<ForwardTriggerToTarget>())
            {
                DestroyImmediate(component);
            }

            if (childGameObject.GetComponent<AnticipateInteractionHighlight>() == null)
                childGameObject.AddComponent<AnticipateInteractionHighlight>();

            if (terminalStub.joystickMatSO != null)
            {
                Material material = PseudoPrefabManager.LoadAsset<Material>(terminalStub.joystickMatSO);
                TerminalCosmeticDecisions terminalCosmeticDecisions = childGameObject.GetComponent<TerminalCosmeticDecisions>();
                if (terminalCosmeticDecisions != null)
                {
                    terminalCosmeticDecisions.ActiveMaterial = material;
                    terminalCosmeticDecisions.InUseMaterial = material;
                }
                foreach (Renderer renderer in childGameObject.RequestComponentsRecursive<Renderer>())
                    renderer.sharedMaterial = material;
            }
        }
    }
}