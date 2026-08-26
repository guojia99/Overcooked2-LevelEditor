using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// 标记可移动火锅载具（由 LayoutRuntimePushablePot.ResetChild 挂到 childGameObject）。
    /// LayoutRuntimePushableVoidFall 只处理带此标记的对象，避免误伤其他 PushableObject。
    /// </summary>
    public class LayoutPushableVoidFallTarget : MonoBehaviour
    {
    }

    /// <summary>
    /// 可移动火锅在 walkable 空洞/水面上方时的坠落补丁。
    ///
    /// 背景：拖锅时宿主 ServerPilotMovement 在 XZ 平面平移且锁 Y，载具 MeshCollider
    /// 还给玩家当地板，导致离开 Col_Floor 区域仍不会落水。松手后同理。
    ///
    /// 策略（server 侧 ticker）：
    ///  - 拖锅且玩家脚点下无 Ground 层 Col_Floor → EndInteraction + 清 GroundCast，走正常落水；
    ///  - 松手且锅中心下无 Ground 支撑 → 关 pilot、开 Rigidbody 重力，落入 KillPlane。
    /// </summary>
    public static class LayoutRuntimePushableVoidFall
    {
        private const float RayStartY = 1.5f;
        private const float RayDistance = 3f;
        private const float PushPlayerProximity = 2.5f;
        private const float PotFallVelocityY = -3f;

        private static int s_groundMask;
        private static bool s_groundMaskReady;
        private static FieldInfo s_pilotGridTargetField;
        private static FieldInfo s_playerGroundCastField;
        private static FieldInfo s_playerApplyGravityField;
        private static MethodInfo s_endInteractionMethod;

        private static readonly HashSet<Rigidbody> s_fallingPotBodies = new HashSet<Rigidbody>();
        private static readonly HashSet<PlayerControls> s_fallingPlayers = new HashSet<PlayerControls>();

        private static VoidFallTicker s_ticker;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (s_ticker != null)
                return;
            var go = new GameObject("LayoutRuntimePushableVoidFall");
            Object.DontDestroyOnLoad(go);
            s_ticker = go.AddComponent<VoidFallTicker>();
        }

        private class VoidFallTicker : MonoBehaviour
        {
            private void Update()
            {
                if (Time.frameCount % 3 != 0)
                    return;
                try
                {
                    LayoutRuntimePushableVoidFall.Tick();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[LayoutRuntimePushableVoidFall] tick skipped: " + ex.Message);
                }
            }
        }

        private static void Tick()
        {
            EnsureGroundMask();
            if (s_groundMask == 0)
                return;

            s_fallingPotBodies.RemoveWhere(rb => rb == null);
            s_fallingPlayers.RemoveWhere(pc => pc == null);

            foreach (var target in Object.FindObjectsOfType<LayoutPushableVoidFallTarget>())
            {
                if (target == null)
                    continue;

                var pushable = target.GetComponentInParent<PushableObject>();
                if (pushable == null)
                    continue;

                var pilot = target.GetComponentInChildren<ServerPilotMovement>();
                var rb = target.GetComponentInChildren<Rigidbody>();
                var col = target.GetComponentInChildren<Collider>();
                var center = col != null ? col.bounds.center : target.transform.position;

                bool attached = IsAnyoneAttached(pushable);
                bool potSupported = HasGroundSupport(center.x, center.z);

                if (!attached && !potSupported && rb != null && !s_fallingPotBodies.Contains(rb))
                    BeginPotFall(pilot, rb);

                if (!attached)
                    continue;

                foreach (var pc in Object.FindObjectsOfType<PlayerControls>())
                {
                    if (pc == null || s_fallingPlayers.Contains(pc))
                        continue;
                    if (!IsPlayerPushing(pc, pushable))
                        continue;

                    var pos = pc.transform.position;
                    if (HasGroundSupport(pos.x, pos.z))
                        continue;

                    BeginPlayerVoidFall(pc, pilot);
                }
            }
        }

        private static void EnsureGroundMask()
        {
            if (s_groundMaskReady)
                return;
            s_groundMaskReady = true;
            int layer = LayerMask.NameToLayer("Ground");
            s_groundMask = layer >= 0 ? (1 << layer) : (1 << 9);
        }

        /// <summary>仅 Ground 层（Col_Floor / Col_AirFloor / 冰面），不含 pushable 碰撞体。</summary>
        private static bool HasGroundSupport(float x, float z)
        {
            return Physics.Raycast(
                new Vector3(x, RayStartY, z),
                Vector3.down,
                RayDistance,
                s_groundMask,
                QueryTriggerInteraction.Ignore);
        }

        private static bool IsPlayerPushing(PlayerControls pc, PushableObject pushable)
        {
            if (pc == null || pushable == null || !pushable.IsAttached(pc.transform))
                return false;

            var current = pc.GetCurrentlyInteracting();
            Transform root;
            if (TryGetTransformRoot(current, out root))
            {
                var pt = pushable.transform;
                if (root == pt || root.IsChildOf(pt) || pt.IsChildOf(root))
                    return true;
            }

            if (pushable.m_AttachPoints == null || pushable.m_AttachPoints.Length == 0)
                return false;

            var playerPos = pc.transform.position;
            float maxDist = PushPlayerProximity * PushPlayerProximity;
            for (int i = 0; i < pushable.m_AttachPoints.Length; i++)
            {
                var ap = pushable.m_AttachPoints[i];
                if (ap == null)
                    continue;
                var delta = ap.transform.position - playerPos;
                delta.y = 0f;
                if (delta.sqrMagnitude <= maxDist)
                    return true;
            }

            return false;
        }

        private static bool TryGetTransformRoot(object interactTarget, out Transform root)
        {
            root = null;
            if (interactTarget == null)
                return false;

            var comp = interactTarget as Component;
            if (comp != null)
            {
                root = comp.transform;
                return true;
            }

            var go = interactTarget as GameObject;
            if (go != null)
            {
                root = go.transform;
                return true;
            }

            return false;
        }

        private static bool IsAnyoneAttached(PushableObject pushable)
        {
            if (pushable == null)
                return false;
            if (pushable.m_UseAttachPoints && pushable.m_AttachPoints != null)
            {
                for (int i = 0; i < pushable.m_AttachPoints.Length; i++)
                {
                    var ap = pushable.m_AttachPoints[i];
                    if (ap == null)
                        continue;
                    var parentable = ap.RequestInterface<IParentable>();
                    if (parentable == null)
                        continue;
                    var attachPoint = parentable.GetAttachPoint(ap.gameObject);
                    if (attachPoint != null && attachPoint.childCount > 0)
                        return true;
                }
                return false;
            }
            if (pushable.m_CentrePoint != null)
                return pushable.m_CentrePoint.transform.childCount > 0;
            return false;
        }

        private static void EndPlayerInteraction(PlayerControls pc)
        {
            if (pc == null)
                return;
            var serverImpl = pc.GetComponent<ServerPlayerControlsImpl_Default>();
            if (serverImpl == null)
                return;
            EnsureMethod(ref s_endInteractionMethod, typeof(ServerPlayerControlsImpl_Default), "EndInteraction");
            if (s_endInteractionMethod == null)
                return;
            try
            {
                s_endInteractionMethod.Invoke(serverImpl, null);
            }
            catch
            {
            }
        }

        private static void BeginPlayerVoidFall(PlayerControls pc, ServerPilotMovement pilot)
        {
            if (pc == null)
                return;

            s_fallingPlayers.Add(pc);

            EndPlayerInteraction(pc);

            if (pilot != null)
                ClearPilotGridTarget(pilot);

            EnsureField(ref s_playerGroundCastField, typeof(PlayerControls), "m_groundCast");
            var groundCast = s_playerGroundCastField != null
                ? s_playerGroundCastField.GetValue(pc) as GroundCast
                : null;
            if (groundCast != null)
            {
                try
                {
                    groundCast.ClearGround();
                    groundCast.ForceUpdateNow();
                }
                catch
                {
                }
            }

            EnsureField(ref s_playerApplyGravityField, typeof(PlayerControls), "m_bApplyGravity");
            if (s_playerApplyGravityField != null)
            {
                try
                {
                    s_playerApplyGravityField.SetValue(pc, true);
                }
                catch
                {
                }
            }
        }

        private static void BeginPotFall(ServerPilotMovement pilot, Rigidbody rb)
        {
            if (rb == null)
                return;

            s_fallingPotBodies.Add(rb);

            if (pilot != null)
            {
                ClearPilotGridTarget(pilot);
                pilot.enabled = false;
            }

            rb.isKinematic = false;
            rb.useGravity = true;
            var v = rb.velocity;
            if (v.y > PotFallVelocityY)
                rb.velocity = new Vector3(v.x, PotFallVelocityY, v.z);
        }

        private static void ClearPilotGridTarget(ServerPilotMovement pilot)
        {
            if (pilot == null)
                return;
            EnsureField(ref s_pilotGridTargetField, typeof(ServerPilotMovement), "m_gridTarget");
            if (s_pilotGridTargetField == null)
                return;
            try
            {
                s_pilotGridTargetField.SetValue(pilot, null);
            }
            catch
            {
            }
        }

        private static void EnsureField(ref FieldInfo field, System.Type type, string name)
        {
            if (field != null)
                return;
            field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void EnsureMethod(ref MethodInfo method, System.Type type, string name)
        {
            if (method != null)
                return;
            method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
