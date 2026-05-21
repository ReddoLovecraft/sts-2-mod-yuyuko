using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using TH_Yuyuko.Scripts.VFX;

namespace TH_Yuyuko.Scripts.Patches;

[HarmonyPatch(typeof(SceneHelper))]
public static class YuyukoVfxScenePathPatches
{
	[HarmonyPatch(nameof(SceneHelper.GetScenePath))]
	[HarmonyPrefix]
	public static bool GetScenePath_Prefix(string innerPath, ref string __result)
	{
		if (innerPath == null || !innerPath.StartsWith(YuyukoVfxManager.ModVfxPrefix))
		{
			return true;
		}

		string local = innerPath.Substring(YuyukoVfxManager.ModVfxPrefix.Length);
		if (local.StartsWith('/'))
		{
			local = local.Substring(1);
		}
		if (local.EndsWith(".tscn"))
		{
			local = local.Substring(0, local.Length - ".tscn".Length);
		}

		__result = YuyukoVfxManager.ModVfxBaseResPath + local + ".tscn";
		return false;
	}
}

