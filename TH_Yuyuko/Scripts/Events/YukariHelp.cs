using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Ui;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Events;

public sealed class YukariHelp : CustomEventModel
{
	 public override string? CustomInitialPortraitPath => "res://TH_Yuyuko/Artworks/Events/yukarihelp.png";
	private RelicModel? _randomRelicToLose;
	private CardModel? _randomCardToLose;
	private PotionModel? _randomPotionToLose;

	private EventOption CreateOption(Func<Task>? onChosen, string optionKey, IEnumerable<IHoverTip>? hoverTips = null)
	{
		LocString title = new LocString(LocTable, optionKey + ".title");
		LocString description = new LocString(LocTable, optionKey + ".description");
		return new EventOption(this, onChosen, title, description, optionKey, hoverTips ?? Enumerable.Empty<IHoverTip>());
	}

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new StringVar("RandomRelic"),
		new StringVar("RandomCard"),
		new StringVar("RandomPotion"),
	];

	public override bool IsAllowed(IRunState runState)
	{
		if (runState.CurrentActIndex is not (1 or 2))
		{
			return false;
		}
		return runState.Players.All(p => p.Character is YuyukoCharacter);
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

		EventOption relicOption = CreateRelicOption(owner);
		EventOption cardOption = CreateCardOption(owner);
		EventOption potionOption = CreatePotionOption(owner);

		return new EventOption[] { relicOption, cardOption, potionOption };
	}

	private EventOption CreateRelicOption(Player owner)
	{
		List<RelicModel> candidates = owner.Relics
			.Where(r => r.IsTradable && !r.IsMelted && r.Status != RelicStatus.Disabled)
			.ToList();

		if (candidates.Count == 0)
		{
			return CreateOption(null, $"{Id.Entry}.pages.INITIAL.options.SELECT_RELIC_LOCKED");
		}

		_randomRelicToLose = Rng.NextItem(candidates);
		((StringVar)DynamicVars["RandomRelic"]).StringValue = _randomRelicToLose.Title.GetFormattedText();

		return CreateOption(SelectRelicFlow, $"{Id.Entry}.pages.INITIAL.options.SELECT_RELIC", [_randomRelicToLose.HoverTip]);
	}

	private async Task SelectRelicFlow()
	{
		Player owner = Owner!;
		if (_randomRelicToLose != null)
		{
			await RelicCmd.Remove(_randomRelicToLose);
		}

		List<RelicModel> relicOptions = GetSelectableRelics(owner);
		RelicModel? selected = relicOptions.Count == 0
			? null
			: await RelicSelectCmd.FromChooseARelicScreen(owner, relicOptions);

		if (selected != null)
		{
			RelicModel relic = ModelDb.GetById<RelicModel>(selected.Id).ToMutable();
			await RelicCmd.Obtain(relic, owner);
		}

		SetEventFinished(PageDescription("SELECT_RELIC"));
	}

	private static List<RelicModel> GetSelectableRelics(Player owner)
	{
		var owned = owner.Relics.Select(r => r.Id).ToHashSet();

		IEnumerable<RelicPoolModel> pools = GetAllRelicPools(owner);
		IEnumerable<RelicModel> options = pools.SelectMany(p => p.GetUnlockedRelics(owner.UnlockState));

		return options
			.GroupBy(r => r.Id)
			.Select(g => g.First())
			.Where(r => !owned.Contains(r.Id))
			.Select(r => r.ToMutable())
			.ToList();
	}

	private static IEnumerable<RelicPoolModel> GetAllRelicPools(Player owner)
	{
		yield return owner.Character.RelicPool;
		yield return ModelDb.RelicPool<SharedRelicPool>();

		foreach (var pool in GetPoolsFromUnlockState<RelicPoolModel>(owner))
		{
			yield return pool;
		}
	}

	private EventOption CreateCardOption(Player owner)
	{
		List<CardModel> candidates = owner.Deck.Cards.Where(c => c.IsRemovable).ToList();
		if (candidates.Count == 0)
		{
			return CreateOption(null, $"{Id.Entry}.pages.INITIAL.options.SELECT_CARD_LOCKED");
		}

		_randomCardToLose = Rng.NextItem(candidates);
		((StringVar)DynamicVars["RandomCard"]).StringValue = _randomCardToLose.Title;

		return CreateOption(SelectCardFlow, $"{Id.Entry}.pages.INITIAL.options.SELECT_CARD", [HoverTipFactory.FromCard(_randomCardToLose)]);
	}

	private async Task SelectCardFlow()
	{
		Player owner = Owner!;
		if (_randomCardToLose != null)
		{
			await CardPileCmd.RemoveFromDeck(_randomCardToLose);
		}

		List<CardModel> libraryCards = GetAllUnlockedCards(owner)
			.Where(c => c.ShouldShowInCardLibrary && c.Type != CardType.Quest && c.Rarity != CardRarity.Curse && c.Type != CardType.Status)
			.ToList();

		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1, 1)
		{
			RequireManualConfirmation = true
		};

		CardModel? selected = (await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), libraryCards, owner, prefs)).FirstOrDefault();
		if (selected != null)
		{
			CardModel card = owner.RunState.CreateCard(selected, owner);
			CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
			CardCmd.PreviewCardPileAdd(result, 2f);
		}

		SetEventFinished(PageDescription("SELECT_CARD"));
	}

	private static IEnumerable<CardModel> GetAllUnlockedCards(Player owner)
	{
		return owner.UnlockState.CharacterCardPools
			.SelectMany(pool => pool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint))
			.GroupBy(c => c.Id)
			.Select(g => g.First());
	}

	private EventOption CreatePotionOption(Player owner)
	{
		List<PotionModel> candidates = owner.Potions.ToList();
		if (candidates.Count == 0)
		{
			return CreateOption(null, $"{Id.Entry}.pages.INITIAL.options.SELECT_POTION_LOCKED");
		}

		_randomPotionToLose = Rng.NextItem(candidates);
		((StringVar)DynamicVars["RandomPotion"]).StringValue = _randomPotionToLose.Title.GetFormattedText();

		return CreateOption(SelectPotionFlow, $"{Id.Entry}.pages.INITIAL.options.SELECT_POTION", [HoverTipFactory.FromPotion(_randomPotionToLose)]);
	}

	private async Task SelectPotionFlow()
	{
		Player owner = Owner!;
		if (_randomPotionToLose != null)
		{
			await PotionCmd.Discard(_randomPotionToLose);
		}

		List<PotionModel> potions = GetSelectablePotions(owner);
		PotionModel? selected = potions.Count == 0 ? null : await NChooseAPotionSelection.ChooseOne(owner, potions);
		if (selected != null)
		{
			await PotionCmd.TryToProcure(selected.ToMutable(), owner);
		}

		SetEventFinished(PageDescription("SELECT_POTION"));
	}

	private static List<PotionModel> GetSelectablePotions(Player owner)
	{
		IEnumerable<PotionPoolModel> pools = GetAllPotionPools(owner);
		IEnumerable<PotionModel> options = pools.SelectMany(p => p.GetUnlockedPotions(owner.UnlockState));

		return options
			.GroupBy(p => p.Id)
			.Select(g => g.First())
			.ToList();
	}

	private static IEnumerable<PotionPoolModel> GetAllPotionPools(Player owner)
	{
		yield return owner.Character.PotionPool;
		yield return ModelDb.PotionPool<SharedPotionPool>();

		foreach (var pool in GetPoolsFromUnlockState<PotionPoolModel>(owner))
		{
			yield return pool;
		}
	}

	private static IEnumerable<TPool> GetPoolsFromUnlockState<TPool>(Player owner) where TPool : class
	{
		object unlockState = owner.UnlockState;
		foreach (var prop in unlockState.GetType().GetProperties())
		{
			if (!prop.CanRead)
			{
				continue;
			}
			if (prop.GetIndexParameters().Length != 0)
			{
				continue;
			}

			object? value;
			try
			{
				value = prop.GetValue(unlockState);
			}
			catch
			{
				continue;
			}

			if (value is System.Collections.IEnumerable enumerable)
			{
				foreach (object? item in enumerable)
				{
					if (item is TPool pool)
					{
						yield return pool;
					}
				}
			}
		}
	}
}
