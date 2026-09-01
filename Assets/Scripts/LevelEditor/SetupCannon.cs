using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    public class SetupCannon : SetupCustomPrefab
    {
        [SerializeField] private GameObject model;
        [SerializeField] private GameObject swivel;
        [SerializeField] private GameObject target;

        public override void Setup()
        {
            base.Setup();
            SetupCannonStub cannonStub = (SetupCannonStub)stub;
            GameObject modelPrefab = PseudoPrefabManager.LoadAsset(cannonStub.modelSO);
            Material modelMat = PseudoPrefabManager.LoadAsset<Material>(cannonStub.modelMatSO);
            GameObject targetPrefab = PseudoPrefabManager.LoadAsset(cannonStub.targetSO);
            Material targetMat = PseudoPrefabManager.LoadAsset<Material>(cannonStub.targetMatSO);
            RuntimeAnimatorController targetAnimator = PseudoPrefabManager.LoadAsset<RuntimeAnimatorController>(cannonStub.targetAnimatorSO);
            Mesh runPuff = PseudoPrefabManager.LoadMeshSubAsset(cannonStub.runPuffSO);
            Material matSmoke = PseudoPrefabManager.LoadAsset<Material>(cannonStub.matSmokeSO);
            Material matConfetti = PseudoPrefabManager.LoadAsset<Material>(cannonStub.matConfettiSO);

            CannonCosmeticDecisions cannonCosmeticDecisions = GetComponent<CannonCosmeticDecisions>();

            // Clear() and Setup() may be called again in LoadAsset(), instantiating a child object
            // thus Clear() here (not above), otherwise two child objects are instantiated
            Clear();

            model = Instantiate(modelPrefab, cannonStub.baseModelParent);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = cannonStub.baseModelRotation;
            foreach (RendererInfo rendererInfo in model.RequestComponentsRecursive<RendererInfo>())
            {
                rendererInfo.lightmapIndex = -1;
                rendererInfo.lightmapScaleOffset = Vector4.zero;
            }
            foreach (Renderer renderer in model.RequestComponentsRecursive<Renderer>())
            {
                renderer.sharedMaterial = modelMat;
            }
            swivel = model.transform.GetChild(0).gameObject;
            swivel.transform.SetParent(cannonStub.swivelModelParent);
            swivel.transform.localRotation = cannonStub.swivelModelRotation;
            cannonCosmeticDecisions.m_cannonAnimator = swivel.transform.GetChild(0).GetComponent<Animator>();
            GameObject pfx = cannonCosmeticDecisions.m_fireFX;
            if (pfx != null)
            {
                pfx.transform.SetParent(swivel.transform.GetChild(0));
                pfx.transform.localPosition = Vector3.zero;
                pfx.transform.localRotation = Quaternion.identity;
                ParticleSystemRenderer psr = pfx.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
                psr.mesh = runPuff;
                psr.sharedMaterial = matSmoke;
                psr = pfx.transform.GetChild(2).GetComponent<ParticleSystemRenderer>();
                psr.trailMaterial = matConfetti;
                psr.sharedMaterial = matConfetti;
                psr = pfx.transform.GetChild(3).GetComponent<ParticleSystemRenderer>();
                psr.trailMaterial = matConfetti;
                psr.sharedMaterial = matConfetti;
                psr = pfx.transform.GetChild(4).GetComponent<ParticleSystemRenderer>();
                psr.mesh = runPuff;
                psr.sharedMaterial = matSmoke;
            }
            pfx = cannonCosmeticDecisions.m_fuseFX;
            if (pfx != null)
            {
                pfx.transform.SetParent(swivel.transform.GetChild(0));
                pfx.transform.localPosition = Vector3.zero;
                pfx.transform.localRotation = Quaternion.identity;
            }

            target = Instantiate(targetPrefab, cannonStub.targetParent);
            target.transform.localScale = cannonStub.targetScale;
            foreach (Renderer renderer in target.RequestComponentsRecursive<Renderer>())
            {
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sharedMaterial = targetMat;
            }
            Animator animator = target.AddComponent<Animator>();
            animator.runtimeAnimatorController = targetAnimator;
        }

        public override void LateSetup()
        {
            base.LateSetup();
            SetupCannonStub cannonStub = (SetupCannonStub)stub;
            GameObject button = cannonStub.button;
            if (button != null)
            {
                if (button.GetComponent<Interactable>() == null && button.GetComponent<PseudoPrefab>() != null)
                    button = button.GetComponent<PseudoPrefab>().childGameObject;
                GetComponent<Cannon>().m_button = button;
            }
        }

        public override void Clear()
        {
            base.Clear();
            SetupCannonStub cannonStub = (SetupCannonStub)stub;
            CannonCosmeticDecisions cannonCosmeticDecisions = GetComponent<CannonCosmeticDecisions>();
            cannonCosmeticDecisions.m_cannonAnimator = null;
            GameObject pfx = cannonCosmeticDecisions.m_fireFX;
            if (pfx != null)
            {
                pfx.transform.SetParent(cannonStub.swivelModelParent);
                pfx.transform.localPosition = Vector3.zero;
                pfx.transform.localRotation = Quaternion.identity;
                ParticleSystemRenderer psr = pfx.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
                psr.mesh = null;
                psr.sharedMaterial = null;
                psr = pfx.transform.GetChild(2).GetComponent<ParticleSystemRenderer>();
                psr.trailMaterial = null;
                psr.sharedMaterial = null;
                psr = pfx.transform.GetChild(3).GetComponent<ParticleSystemRenderer>();
                psr.trailMaterial = null;
                psr.sharedMaterial = null;
                psr = pfx.transform.GetChild(4).GetComponent<ParticleSystemRenderer>();
                psr.mesh = null;
                psr.sharedMaterial = null;
            }
            pfx = cannonCosmeticDecisions.m_fuseFX;
            if (pfx != null)
            {
                pfx.transform.SetParent(cannonStub.swivelModelParent);
                pfx.transform.localPosition = Vector3.zero;
                pfx.transform.localRotation = Quaternion.identity;
            }

            DestroyImmediate(model);
            DestroyImmediate(swivel);
            DestroyImmediate(target);
            model = null;
            swivel = null;
            target = null;

            GetComponent<Cannon>().m_button = null;
        }
    }
}