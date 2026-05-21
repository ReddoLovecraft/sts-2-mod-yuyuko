using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Patches;

[HarmonyPatch(typeof(SlowPower), "AfterSideTurnStart")]
[HarmonyPatch(new Type[] { typeof(CombatSide), typeof(CombatState) })]
public static class YuyukoSlowPowerNoNaturalDecreasePatch
{
	[HarmonyPrefix]
	public static bool AfterSideTurnStart_Prefix(SlowPower __instance, CombatSide side, CombatState combatState, ref Task __result)
	{
		bool enabled = combatState?.Players.Any(p => p?.Creature != null && p.Creature.GetPowerAmount<SlowGhostPower>() > 0) ?? false;
		if (!enabled)
		{
			return true;
		}

		__result = Task.CompletedTask;
		return false;
	}
}

[HarmonyPatch(typeof(SlowPower), nameof(SlowPower.AfterCardPlayed))]
public static class YuyukoSlowPowerReduceOnNonAttackPatch
{
	private const string _slowAmountKey = "SlowAmount";
	private static readonly MethodInfo? _invokeDisplayAmountChanged = AccessTools.Method(typeof(PowerModel), "InvokeDisplayAmountChanged");

	[HarmonyPostfix]
	public static void AfterCardPlayed_Postfix(SlowPower __instance, PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (__instance?.Owner == null || __instance.Owner.Player == null)
		{
			return;
		}

		if (cardPlay.Card.Owner != __instance.Owner.Player)
		{
			return;
		}
		if (cardPlay.Card.Type == CardType.Attack)
		{
			return;
		}

		int reduce = __instance.Owner.GetPowerAmount<SlowGhostPower>();
		if (reduce <= 0)
		{
			return;
		}

		DynamicVar slowAmountVar = __instance.DynamicVars[_slowAmountKey];
		int current = slowAmountVar.IntValue;
		int next = current - reduce;
		slowAmountVar.BaseValue = next <= 0 ? 0m : next;

		try
		{
			_invokeDisplayAmountChanged?.Invoke(__instance, null);
		}
		catch
		{
		}
	}
}
