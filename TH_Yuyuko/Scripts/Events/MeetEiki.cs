using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Yuyuko.Scrpits.Events;

public sealed class MeetEiki : CustomEventModel
{
	 public override string? CustomInitialPortraitPath => "res://TH_Yuyuko/Artworks/Events/meeteiki.png";
	private EventOption CreateOption(Func<Task>? onChosen, string optionKey, IEnumerable<IHoverTip>? hoverTips = null)
	{
		LocString title = new LocString(LocTable, optionKey + ".title");
		LocString description = new LocString(LocTable, optionKey + ".description");
		return new EventOption(this, onChosen, title, description, optionKey, hoverTips ?? Enumerable.Empty<IHoverTip>());
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (runState.CurrentActIndex is not (0 or 1))
		{
			return false;
		}
		return runState.Players.All(p => p.Deck.Cards.Count(c => c.IsRemovable) >= 3);
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
			CreateOption(Run, $"{Id.Entry}.pages.INITIAL.options.RUN"),
			CreateOption(Listen, $"{Id.Entry}.pages.INITIAL.options.LISTEN", HoverTipFactory.FromCardWithCardHoverTips<Writhe>()),
		};
	}

	private async Task Run()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, 10, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		SetEventFinished(PageDescription("RUN"));
	}

	private async Task Listen()
	{
		Player owner = Owner!;

		List<CardModel> selected = (await CardSelectCmd.FromDeckForRemoval(
			player: owner,
			prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 3)
		)).ToList();
		if (selected.Count != 0)
		{
			foreach (CardModel card in selected)
			{
				await CardPileCmd.RemoveFromDeck(card);
			}
		}

		await CardPileCmd.AddCursesToDeck(Enumerable.Repeat(ModelDb.Card<Writhe>(), 1), owner);
		SetEventFinished(PageDescription("LISTEN"));
	}
}
