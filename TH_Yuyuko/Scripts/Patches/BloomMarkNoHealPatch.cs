using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using TH_Yuyuko.Scrpits.Relics;

namespace TH_Yuyuko.Scripts.Patches;

[HarmonyPatch(typeof(CreatureCmd))]
public static class BloomMarkNoHealPatch
{
	[HarmonyPatch(nameof(CreatureCmd.Heal), [typeof(Creature), typeof(decimal), typeof(bool)])]
	[HarmonyPrefix]
	public static bool Prefix(Creature creature, decimal amount, ref Task __result)
	{
		if (amount <= 0m)
		{
			return true;
		}

		if (creature?.Player == null)
		{
			return true;
		}

		if (!creature.Player.Relics.Any(r => r is BloomMark))
		{
			return true;
		}

		__result = Task.CompletedTask;
		return false;
	}
}
