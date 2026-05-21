using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace TH_Yuyuko.Scripts.Patches;

[HarmonyPatch]
public static class YuyukoDamageBlockedVfxPatches
{
	private static MethodBase? TargetMethod()
	{
		Type? t = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Vfx.NDamageBlockedVfx");
		if (t == null)
		{
			return null;
		}

		return AccessTools.Method(t, "Create", [typeof(Creature)]);
	}

	[HarmonyPrefix]
	public static bool Prefix(Creature target)
	{
		if (target == null)
		{
			return false;
		}

		NCombatRoom? room = NCombatRoom.Instance;
		if (room == null)
		{
			return false;
		}

		if (room.GetCreatureNode(target) == null)
		{
			return false;
		}

		return true;
	}
}

