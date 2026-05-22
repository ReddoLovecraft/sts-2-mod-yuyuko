using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Yuyuko.Scrpits.Events;

public sealed class MeetMoto : CustomEventModel
{
	 public override string? CustomInitialPortraitPath => "res://TH_Yuyuko/Artworks/Events/meetmoto.png";
	private EventOption CreateOption(Func<Task>? onChosen, string optionKey, IEnumerable<IHoverTip>? hoverTips = null)
	{
		LocString title = new LocString(LocTable, optionKey + ".title");
		LocString description = new LocString(LocTable, optionKey + ".description");
		return new EventOption(this, onChosen, title, description, optionKey, hoverTips ?? Enumerable.Empty<IHoverTip>());
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
		return new EventOption[]
		{
			CreateOption(Communicate, $"{Id.Entry}.pages.INITIAL.options.COMMUNICATE"),
			CreateOption(Take, $"{Id.Entry}.pages.INITIAL.options.TAKE"),
		};
	}

	private Task Communicate()
	{
		Player owner = Owner!;
		IEnumerable<CardModel> cards = PileType.Deck.GetPile(owner).Cards
			.Where(c => c?.IsUpgradable ?? false)
			.ToList()
			.StableShuffle(owner.RunState.Rng.Niche)
			.Take(3);

		foreach (CardModel card in cards)
		{
			CardCmd.Upgrade(card);
		}

		SetEventFinished(PageDescription("COMMUNICATE"));
		return Task.CompletedTask;
	}

	private async Task Take()
	{
		SetEventFinished(PageDescription("TAKE"));
		await TeleportToActBoss(Owner!);
	}

	private static Task TeleportToActBoss(Player owner)
	{
		IRunState runState = owner.RunState;
		MapCoord destination = runState.Map.BossMapPoint.coord;
		Player me = LocalContext.GetMe(runState);
		MoveToMapCoordAction action = new MoveToMapCoordAction(me, destination);
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
		return Task.CompletedTask;
	}
}
