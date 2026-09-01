using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditor
{
    public class PseudoPrefabToggleSwitch : PseudoPrefab
    {
        public override void Setup()
        {
            PseudoPrefabToggleSwitchStub switchStub = (PseudoPrefabToggleSwitchStub)stub;

            foreach (var componet in gameObject.GetComponents<TriggerOnObject>())
            {
                componet.m_targetObject = childGameObject;
            }

            if (!string.IsNullOrEmpty(switchStub.toggleBoolOnAnimator))
            {
                var triggerToggleOnAnimator = childGameObject.GetComponent<TriggerToggleOnAnimator>();
                triggerToggleOnAnimator.m_targetParameter = switchStub.toggleBoolOnAnimator;
                triggerToggleOnAnimator.m_targetAnimator = switchStub.animatorToTrigger;
                triggerToggleOnAnimator.m_targetParameterHash = Animator.StringToHash(triggerToggleOnAnimator.m_triggerToReceive);
            }

            if (!string.IsNullOrEmpty(switchStub.triggerOnAnimator))
            {
                TriggerOnAnimator triggerOnAnimator = childGameObject.AddComponent<TriggerOnAnimator>();
                triggerOnAnimator.m_targetAnimator = switchStub.animatorToTrigger;
                triggerOnAnimator.m_triggerToReceive = "Switch";
                triggerOnAnimator.m_triggerToFire = switchStub.triggerOnAnimator;
                triggerOnAnimator.m_triggerToFireHash = Animator.StringToHash(triggerOnAnimator.m_triggerToFire);
            }

            if (switchStub.objectToTrigger != null && switchStub.objectToTrigger.Length > 0)
            {
                var triggerOnObject = childGameObject.AddComponent<TriggerOnObject>();
                triggerOnObject.m_trigger = "Switch";
                triggerOnObject.m_triggerToFire = switchStub.triggerOnObject;
                if (switchStub.objectToTrigger.Length == 1)
                {
                    triggerOnObject.m_targetObject = switchStub.objectToTrigger[0];
                    triggerOnObject.m_targetObjects = new GameObject[0];
                }
                else
                {
                    triggerOnObject.m_targetObject = null;
                    triggerOnObject.m_targetObjects = switchStub.objectToTrigger;
                }
            }
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