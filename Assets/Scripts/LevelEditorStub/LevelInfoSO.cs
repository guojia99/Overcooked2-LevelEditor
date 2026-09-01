using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace LevelEditorStub
{
    [CreateAssetMenu(menuName = "LevelEditor/LevelInfoSO", order = -1000)]
    public class LevelInfoSO : ScriptableObject
	{
        [SerializeField] public string levelName;
        [SerializeField] public string levelNameZH;
        [SerializeField] public Sprite screenshot;
        [SerializeField] public string sceneName;

        [Header("Recipe Settings")]
        [SerializeField] public ScriptableObject[] recipes;
        [SerializeField] public int debugRecipeCount;
        [Range(1, 10)]
        [SerializeField] public int minOrderCount = 2;
        [Range(1, 10)]
        [SerializeField] public int maxOrderCount = 5;

        [Header("Recipe Match List Settings")]
        [SerializeField] public bool excludeStoryRecipeMatchList;
        [SerializeField] public PseudoPrefabSO[] includeRecipeMatchLists;
        [SerializeField] public PseudoPrefabSO[] allIngredients;
        [SerializeField] public ScriptableObject[] optionalRecipeMatchListItems;
        [SerializeField] public PseudoPrefabSO[] allCookingSteps;

        [Header("Audio Settings")]
        [SerializeField] public PseudoPrefabSO inLevelMusicSO;
        [SerializeField] public AudioClip inLevelMusic;
        [SerializeField] public GameLoopingAudioTag[] inLevelAmbiences = new GameLoopingAudioTag[0];
        [SerializeField] public PseudoPrefabSO[] audioDirectorySOs = new PseudoPrefabSO[0];

        [Header("Other Settings")]
        [SerializeField] public bool disableDynamicParenting = true;
        
        [SerializeField] public PseudoPrefabSO onDeathEffectSO;

        [SerializeField] public LevelConfigSetupPerPlayerCountSO config_1p;
        [SerializeField] public LevelConfigSetupPerPlayerCountSO config_2p;
        [SerializeField] public LevelConfigSetupPerPlayerCountSO config_3p;
        [SerializeField] public LevelConfigSetupPerPlayerCountSO config_4p;

        [SerializeField] public string[] dependencies;

        public enum GameLoopingAudioTag
        {
            RestaurantAmbience,
            CityAmbience,
            CityAmbience2,
            WashingUp,
            Sizzling,
            PanSizzle,
            FryingPanSizzle,
            Flames,
            ExtinguisherSpray,
            VanEngine,
            KingVOLoop,
            PeckishDeathRumble,
            Helicopter,
            Flamethrower,
            DynamicStage01AmbInside,
            HotAirBalloonAmb,
            HotAirBalloonMotor,
            HotAirBallonCrowd,
            SushiAmbience,
            SushiTrafficIdle,
            RapidsAmbience,
            RapidsIntro,
            WizardAmbience,
            WizardCrowd,
            SushiIntroAmbience,
            FrontendAmbience,
            TutorialAmbience,
            TutorialIntroAmbience,
            SushiNightAmbience,
            DeepFatFryLoop,
            OvenLoop,
            SteamerLoop,
            SpaceAmb,
            SpaceCameraShake,
            DynamicStage02RiverDrums,
            DynamicStage02MineAmb,
            DynamicStage02Crowd,
            DynamicStage02RapidsAmb,
            TutorialBreadMoans,
            CutsceneAmbInside,
            CutsceneAmbOutside,
            CutsceneAmbCrypt,
            DynamicStage04Amb01,
            DynamicStage04Amb02,
            DynamicStage04AmbFlames,
            DynamicStage04AmbZombies,
            SwampAmbience,
            MovingPlatform,
            OnionKingLoop_01,
            OnionKingLoop_Pooch,
            Tutorial_Amb_No_Rain,
            Zombie_Wind,
            UI_Text_Loop,
            OnionKingRadioLoop,
            WorldMapBoatEngine,
            WorldMapPlaneEngine,
            HotAirPropFast,
            WaterJet,
            Space_Ship_Launch_Loop,
            DLC_02_Amb,
            DLC_02_Amb_Sweetner,
            DLC_02_Crowd,
            DLC_02_SmoothieMaker,
            DLC_02_WaterFlow,
            DLC_02_BBQ_Idle,
            DLC_02_Seagulls,
            DLC_02_Bellow_Blank,
            DLC_02_Resort_Amb,
            DLC_02_BBQ_Smoke,
            DLC_02_ThroneRoomAmb,
            DLC_03_Amb,
            DLC_03_Market_Amb,
            DLC_Cream_Gun,
            DLC_04_Amb_City,
            DLC_04_Amb_Garden,
            DLC_04_Cooking_Pot_Drag,
            DLC_04_HotPotBubble,
            DLC_04_Flames,
            DLC_04_Embers,
            DLC_04_Crowd,
            DLC_05_Amb_Camp,
            DLC_05_Amb_Tree,
            DLC_05_Amb_Special,
            DLC_05_Amb_River,
            DLC_05_Amb_Crowd,
            DLC_05_Amb_Camp_Alt,
            DLC_05_Amb_Tree_Alt,
            DLC_05_Fire_Sml,
            DLC_05_Fire_Med,
            DLC_05_Fire_Lrg,
            DLC_05_LakeLoop,
            DLC_05_Campfire,
            DLC_05_Owls,
            DLC_07_Furnace_Burn_Lrg,
            DLC_07_Furnace_Burn_Med,
            DLC_07_Furnace_Burn_Sml,
            DLC_07_Battlements_Amb,
            DLC_07_Keep_Amb,
            DLC_07_Courtyard_Amb,
            DLC_07_Thunder_Loop,
            DLC_07_Rain_Loop,
            DLC_07_Water_Lap,
            DLC_07_Keep_Amb_Flames,
            DLC_07_Generic_Crowd,
            DLC_07_City_Amb,
            MixerLoop,
            DLC_07_Drone,
            DLC_07_Drips,
            DLC_07_Wind,
            DLC_07_Birds,
            DLC_08_Cannon_Fuse,
            DLC_08_Cannon_Rotate,
            DLC_08_FireBreather_Torch,
            DLC_08_Popcorn_Machine,
            DLC_08_Fairground_Day_Amb,
            DLC_08_Fairground_Night_Amb,
            DLC_08_BigTop_Amb,
            DLC_08_ThroneRoom_Amb_01,
            DLC_08_ThroneRoom_Amb_02,
            DLC_08_ThroneRoom_Amb_03,
            DLC_08_ThroneRoom_Amb_04,
            DLC_09_Water_Lap_Loop,
            DLC_09_Water_Move_Loop,
            DLC_11_Amb_Loop_1_1,
            DLC_11_Amb_Loop_1_2,
            DLC_11_Amb_Loop_1_3,
            DLC_11_Amb_Loop_1_4,
            DLC_11_Amb_Loop_1_5,
            DLC_11_Firework_Rattle_Loop,
            DLC_11_Firework_Sparkle_Loop,
            DLC_11_Firework_Rattle_Loop_FadeIn,
            DLC_11_Firework_Fuse_Loop,
            DLC_13_Amb_Base_Loop,
            DLC_13_Amb_Crowd_Loop,
            COUNT
        }
    }
}