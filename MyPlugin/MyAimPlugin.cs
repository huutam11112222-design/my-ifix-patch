using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace MyAimPlugin
{
    [BepInPlugin("com.huutam.aimplugin", "Aim Neck Plugin", "1.0.0")]
    public class AimPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static AimPlugin Instance;

        private ConfigEntry<bool>    cfgEnabled;
        private ConfigEntry<float>   cfgFov;
        private ConfigEntry<float>   cfgSmooth;
        private ConfigEntry<float>   cfgMaxDist;
        private ConfigEntry<KeyCode> cfgToggleKey;

        private bool aimActive = false;

        private MethodInfo getNeckBoneMethod;
        private MethodInfo getBoneRootMethod;
        private Type       playerType;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            cfgEnabled   = Config.Bind("Aim", "Enabled",   true,  "Bat/tat aim");
            cfgFov       = Config.Bind("Aim", "FOV",       12f,   "Gioi han goc aim");
            cfgSmooth    = Config.Bind("Aim", "Smooth",    0.35f, "Do muot 0-1");
            cfgMaxDist   = Config.Bind("Aim", "MaxDist",   250f,  "Khoang cach toi da");
            cfgToggleKey = Config.Bind("Aim", "ToggleKey", KeyCode.X, "Phim bat/tat");

            TryResolvePlayerMethods();
            Log.LogInfo("[AimPlugin] Loaded. Toggle key: " + cfgToggleKey.Value);
        }

        private void TryResolvePlayerMethods()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    playerType = asm.GetType("COW.GamePlay.Player", false);
                    if (playerType != null)
                    {
                        Log.LogInfo("[AimPlugin] Found Player in: " + asm.GetName().Name);
                        break;
                    }
                }
                catch { }
            }

            if (playerType == null)
            {
                Log.LogError("[AimPlugin] KHONG TIM THAY class COW.GamePlay.Player");
                return;
            }

            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.Static;

            getNeckBoneMethod = playerType.GetMethod("get_NeckBone", F);
            getBoneRootMethod = playerType.GetMethod("get_BoneRootTransform", F);

            Log.LogInfo("[AimPlugin] get_NeckBone           = " + (getNeckBoneMethod != null));
            Log.LogInfo("[AimPlugin] get_BoneRootTransform = " + (getBoneRootMethod != null));
        }

        private void Update()
        {
            if (Input.GetKeyDown(cfgToggleKey.Value))
            {
                aimActive = !aimActive;
                Log.LogInfo("[AimPlugin] Aim " + (aimActive ? "ON" : "OFF"));
            }

            if (!cfgEnabled.Value || !aimActive) return;
            if (playerType == null) return;
            if (Camera.main == null) return;

            object localPlayer = GetLocalPlayer();
            if (localPlayer == null) return;

            object targetPlayer = FindBestTarget(localPlayer);
            if (targetPlayer == null) return;

            Vector3? targetPos = GetBonePosition(targetPlayer, getNeckBoneMethod);
            if (targetPos == null) targetPos = GetBonePosition(targetPlayer, getBoneRootMethod);
            if (targetPos == null) return;

            Camera cam = Camera.main;
            Vector3 from = cam.transform.position;
            Vector3 to   = targetPos.Value;
            Vector3 dir  = (to - from).normalized;
            float dist   = Vector3.Distance(from, to);

            if (dist > cfgMaxDist.Value) return;

            float angle = Vector3.Angle(cam.transform.forward, dir);
            if (angle > cfgFov.Value) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation, targetRot, cfgSmooth.Value);
        }

        private Vector3? GetBonePosition(object playerInstance, MethodInfo method)
        {
            if (method == null || playerInstance == null) return null;
            try
            {
                object result = method.Invoke(playerInstance, null);
                if (result == null) return null;
                Transform t = result as Transform;
                if (t == null) return null;
                return t.position;
            }
            catch (Exception ex)
            {
                Log.LogWarning("[AimPlugin] GetBonePosition error: " + ex.Message);
                return null;
            }
        }

        // ============================================================
        //  BAN PHAI SUA 2 HAM DUOI DAY
        //  Dung dnSpy mo Assembly-CSharp.dll, tim class quan ly player
        // ============================================================
        private object GetLocalPlayer()
        {
            // TODO: Sua ham nay
            return null;
        }

        private object FindBestTarget(object localPlayer)
        {
            // TODO: Sua ham nay
            return null;
        }
    }
}
