using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Events;

public sealed class Hermit : CustomEventModel
{
	 public override string? CustomInitialPortraitPath => "res://TH_Yuyuko/Artworks/Events/hermit.png";
	private EventOption CreateOption(Func<Task>? onChosen, string optionKey, IEnumerable<IHoverTip>? hoverTips = null)
	{
		LocString title = new LocString(LocTable, optionKey + ".title");
		LocString description = new LocString(LocTable, optionKey + ".description");
		return new EventOption(this, onChosen, title, description, optionKey, hoverTips ?? Enumerable.Empty<IHoverTip>());
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (runState.CurrentActIndex is not (1 or 2))
		{
			return false;
		}
		return runState.Players.Any(p => p.Character is YuyukoCharacter);
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
			CreateOption(Review, $"{Id.Entry}.pages.INITIAL.options.REVIEW"),
			CreateOption(Transform, $"{Id.Entry}.pages.INITIAL.options.TRASFORM", [HoverTipFactory.Static(StaticHoverTip.Transform)]),
			CreateOption(Remove, $"{Id.Entry}.pages.INITIAL.options.REMOVE"),
		};
	}

	private Task Review()
	{
		Player owner = Owner!;
		IEnumerable<CardModel> upgradable = PileType.Deck.GetPile(owner).Cards.Where(c => c?.IsUpgradable ?? false).ToList().StableShuffle(owner.RunState.Rng.Niche).Take(3);
		foreach (CardModel card in upgradable)
		{
			CardCmd.Upgrade(card);
		}

		SetEventFinished(PageDescription("REVIEW"));
		return Task.CompletedTask;
	}

	private async Task Transform()
	{
		Player owner = Owner!;
		List<CardModel> selected = (await CardSelectCmd.FromDeckForTransformation(
			player: owner,
			prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 2)
		)).ToList();

		foreach (CardModel item in selected)
		{
			CardModel transformed = CardFactory.CreateRandomCardForTransform(item, isInCombat: false, owner.RunState.Rng.Niche);
			await CardCmd.Transform(item, transformed);
		}

		SetEventFinished(PageDescription("TRASFORM"));
	}

	private async Task Remove()
	{
		Player owner = Owner!;
		List<CardModel> selected = (await CardSelectCmd.FromDeckForRemoval(
			player: owner,
			prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1)
		)).ToList();
		if (selected.Count != 0)
		{
			foreach (CardModel card in selected)
			{
				await CardPileCmd.RemoveFromDeck(card);
			}
		}
		SetEventFinished(PageDescription("REMOVE"));
	}
}
