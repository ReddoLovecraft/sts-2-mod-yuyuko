using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Events;

public sealed class Dinnerhall : CustomEventModel
{
	 public override string? CustomInitialPortraitPath => "res://TH_Yuyuko/Artworks/Events/dinnerhall.png";
	private EventOption CreateOption(Func<Task>? onChosen, string optionKey, IEnumerable<IHoverTip>? hoverTips = null)
	{
		LocString title = new LocString(LocTable, optionKey + ".title");
		LocString description = new LocString(LocTable, optionKey + ".description");
		return new EventOption(this, onChosen, title, description, optionKey, hoverTips ?? Enumerable.Empty<IHoverTip>());
	}

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new GoldVar(0),
	];

	public override bool IsAllowed(IRunState runState)
	{
		return runState.Players.Any(p => p.Character is YuyukoCharacter);
	}

	public override void CalculateVars()
	{
		base.CalculateVars();

		Player owner = Owner!;
		int goldLost = Math.Max(0, owner.Gold);
		int maxHpGain = goldLost / 15;
		DynamicVars.Gold.BaseValue = maxHpGain;
	}

	protected override Task BeforeEventStarted(bool isPreFinished)
	{
		Owner!.CanRemovePotions = false;
		return Task.CompletedTask;
	}

	protected override void OnEventFinished()
	{
		Owner!.CanRemovePotions = true;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = Owner!;

		EventOption leave = owner.Creature.CurrentHp >= owner.Creature.MaxHp
			? CreateOption(Leave, $"{Id.Entry}.pages.INITIAL.options.LEAVE")
			: CreateOption(null, $"{Id.Entry}.pages.INITIAL.options.LEAVE_LOCKED");

		return new EventOption[]
		{
			CreateOption(Order, $"{Id.Entry}.pages.INITIAL.options.ORDER"),
			CreateOption(EatBird, $"{Id.Entry}.pages.INITIAL.options.EAT_BIRD", HoverTipFactory.FromCardWithCardHoverTips<Shame>()),
			leave
		};
	}

	private async Task Order()
	{
		Player owner = Owner!;
		int goldLost = Math.Max(0, owner.Gold);
		int maxHpGain = goldLost / 15;
		DynamicVars.Gold.BaseValue = maxHpGain;

		if (goldLost != 0)
		{
			await PlayerCmd.LoseGold(goldLost, owner, GoldLossType.Stolen);
		}
		if (maxHpGain != 0)
		{
			await CreatureCmd.GainMaxHp(owner.Creature, maxHpGain);
		}

		SetEventFinished(PageDescription("ORDER"));
	}

	private async Task EatBird()
	{
		Player owner = Owner!;
		await CreatureCmd.GainMaxHp(owner.Creature, 15);
		await CardPileCmd.AddCursesToDeck(Enumerable.Repeat(ModelDb.Card<Shame>(), 1), owner);
		SetEventFinished(PageDescription("EAT_BIRD"));
	}

	private Task Leave()
	{
		SetEventFinished(PageDescription("LEAVE"));
		return Task.CompletedTask;
	}
}
