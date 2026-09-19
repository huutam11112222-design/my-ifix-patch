using System;
using UnityEngine;
using IFix;

namespace COW.GamePlay
{
    [IFixPatch]
    public static class Player_AimPatch
    {
        [Patch]
        public static Transform get_NeckBone(Player __instance)
        {
            Transform t = Original.get_NeckBone(__instance);
            if (t == null)
                t = Original.get_BoneRootTransform(__instance);
            return t;
        }
    }
}
