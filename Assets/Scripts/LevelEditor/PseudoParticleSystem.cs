using LevelEditorStub;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LevelEditor
{
    [ExecuteInEditMode]
    public class PseudoParticleSystem : SetupCustomPrefab
    {
        private PseudoParticleSystemStub pseudoParticleSystemStub;
        private ParticleSystem ps;
        private ParticleSystemRenderer psr;

        protected override void Awake()
        {
            pseudoParticleSystemStub = GetComponent<PseudoParticleSystemStub>();
            ps = GetComponent<ParticleSystem>();
            psr = GetComponent<ParticleSystemRenderer>();
        }

        public override void Setup() 
        {
            Clear();

            if (pseudoParticleSystemStub.meshSO != null)
            {
                psr.renderMode = ParticleSystemRenderMode.Mesh;
                Mesh mesh = PseudoPrefabManager.LoadMeshSubAsset(pseudoParticleSystemStub.meshSO);
                psr.SetMeshes(new Mesh[] { mesh });
            }

            if (pseudoParticleSystemStub.materialSO != null)
            {
                Material material = PseudoPrefabManager.LoadAsset<Material>(pseudoParticleSystemStub.materialSO);
                psr.sharedMaterial = material;
            }
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public override void Clear()
        {
            pseudoParticleSystemStub = GetComponent<PseudoParticleSystemStub>();
            ps = GetComponent<ParticleSystem>();
            psr = GetComponent<ParticleSystemRenderer>();

            if (pseudoParticleSystemStub.meshSO != null)
            {
                psr.renderMode = ParticleSystemRenderMode.None;
                psr.mesh = null;
            }

            if (pseudoParticleSystemStub.materialSO != null)
            {
                psr.sharedMaterial = null;
            }
        }
    }
}