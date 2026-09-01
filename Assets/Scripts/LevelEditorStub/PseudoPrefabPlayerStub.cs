using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LevelEditorStub
{
    public class PseudoPrefabPlayerStub : PseudoPrefabStub {

        [SerializeField] public Player playerID = Player.Count;
        [SerializeField] public HatVisState hatVisState = HatVisState.None;
        [SerializeField] public KnifeVisState knifeVisState = KnifeVisState.None;

        public enum Player
        {
            One = 0,
            Two = 1,
            Three = 2,
            Four = 3,
            Five = 4,
            Six = 5,
            Seven = 6,
            Eight = 7,
            Nine = 8,
            Ten = 9,
            Eleven = 10,
            Count = 11
        }

        public enum KnifeVisState
        {
            None,
            Cleaver,
            Knife,
            Hatchet,
            Hammer,
        }

        public enum HatVisState
        {
            None,
            Deprecated‌_Cap,
            Deprecated‌_Tall,
            Fancy,
            Festive,
            Baseball
        }
    }
}
