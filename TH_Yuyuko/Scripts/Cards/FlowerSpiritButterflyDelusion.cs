using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class FlowerSpiritButterflyDelusion : YuyukoCardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		Tools.GetStaticKeyword("Butterfly"),
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
	];

	public FlowerSpiritButterflyDelusion() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int maxSelect = ToolBox.GetTotalButterflies(base.Owner.Creature);
		if (maxSelect <= 0)
		{
			return;
		}
		List<CardModel> cardsIn = (from c in PileType.Draw.GetPile(base.Owner).Cards
			orderby c.Rarity, c.Id
			select c).ToList();
		if (cardsIn.Count == 0)
		{
			return;
		}
		maxSelect = Math.Min(maxSelect, cardsIn.Count);
		CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, maxSelect)
		{
			Cancelable = true
		};
		List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, cardsIn, base.Owner, prefs)).ToList();
		if (selected.Count == 0)
		{
			return;
		}
		foreach (CardModel card in selected)
		{
			if (base.IsUpgraded)
			{
				card.EnergyCost.SetThisTurnOrUntilPlayed(0);
			}
			await CardPileCmd.Add(card, PileType.Hand);
		}
		await ToolBox.ConsumeRandomButterflies(choiceContext, base.Owner.Creature, selected.Count, base.Owner.RunState.Rng.CombatOrbGeneration);
	}
	protected override void OnUpgrade()
	{
	}
}

}
