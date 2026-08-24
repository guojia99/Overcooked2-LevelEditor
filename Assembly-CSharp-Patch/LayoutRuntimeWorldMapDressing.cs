using LevelEditor;
using UnityEngine;

/// 世界地图装饰强制展开，烘焙到 map_* 装饰伪根上，随场景保存，编辑器与游戏包内都生效）。
///
/// 背景：dlc08 map_* 装饰（bundle 内 dressing assets/map/ 家族，如
/// p_dlc08_map_rope_fence_* 绳栏）的 prefab 带 WorldMapSceneryOptimizer：
/// 其 Awake() 把整个可视 Mesh 子树 SetActive(false)，只有世界地图场景的
/// 展开流程（WorldMapTileFlip → StartUnfoldFlow/StartInstantUnfold → End）
/// 才会重新激活。关卡场景里 m_startFlipped=0 且 m_flipOwnerData.m_levelMapNode=null，
/// 展开永不触发——表现为编辑器里可见，进 Play / 游戏包后整件消失。
///
/// 本组件等伪 prefab child 实例化（其 Awake 已跑、Mesh 已隐藏）后，对每个
/// optimizer 调 End(FlipDirection.Unfold)：Mesh 重新激活、不挂 Animator，
/// 等价于世界地图上展开完成的终态。无 optimizer 的伪根（map 家族里也有纯网格件，
/// 如 p_dlc08_cloud_clump）自然空转，无副作用。
public class LayoutRuntimeWorldMapDressing : MonoBehaviour
{
    private bool m_done;

    private void Update()
    {
        if (m_done)
            return;
        var pseudo = GetComponent<PseudoPrefab>();
        if (pseudo == null)
        {
            m_done = true;
            return;
        }
        var child = pseudo.childGameObject;
        if (child == null)
            return; // child 尚未生成（ResetChild 未执行），下帧再试
        m_done = true;

        var optimizers = child.GetComponentsInChildren<WorldMapSceneryOptimizer>(true);
        if (optimizers == null)
            return;
        for (int i = 0; i < optimizers.Length; i++)
        {
            var opt = optimizers[i];
            if (opt == null)
                continue;
            var mesh = opt.Mesh;
            if (mesh != null && !mesh.activeSelf)
                opt.End(FlipDirection.Unfold);
        }
    }
}
