using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditorStub
{
    public class PseudoPrefabToggleSwitchStub : PseudoPrefabStub
    {
        [SerializeField] public string toggleBoolOnAnimator;
        [SerializeField] public string triggerOnAnimator;
        [SerializeField] public Animator animatorToTrigger;
        [SerializeField] public string triggerOnObject;
        [SerializeField] public GameObject[] objectToTrigger;
    }
}