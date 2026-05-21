using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class FlowerSpiritDeepRootedButterfly : YuyukoCardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		Tools.GetStaticKeyword("SummonButterfly"),
		Tools.GetStaticKeyword("Butterfly"),
		HoverTipFactory.FromPower<ButterflyEnergyPower>()
	];

	public FlowerSpiritDeepRootedButterfly() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int discardCount = PileType.Discard.GetPile(base.Owner).Cards.Count;
		if (discardCount > 0)
		{
			await ToolBox.SummonButterflies<ButterflyEnergyPower>(choiceContext, base.Owner.Creature, discardCount);
			await CardPileCmd.Shuffle(choiceContext, base.Owner);
		}
	}
	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}

}
