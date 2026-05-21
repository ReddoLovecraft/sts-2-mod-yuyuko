using System;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class FlowerSpiritSwallowtailButterfly : YuyukoCardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		Tools.GetStaticKeyword("Butterfly")
	];
	protected override bool ShouldGlowGoldInternal =>Owner.HasPower<ButterflyEnergyPower>()||Owner.HasPower<ButterflyDeathPower>()||Owner.HasPower<ButterflySoulPower>();
	public FlowerSpiritSwallowtailButterfly() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int butterflyTypes =0+
			(ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Death) > 0 ? 1 : 0)
			+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Soul) > 0 ? 1 : 0)
			+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Energy) > 0 ? 1 : 0);
		await CardPileCmd.Draw(choiceContext, 1 + butterflyTypes, base.Owner);
		await PowerCmd.Apply<DrawCardsNextTurnPower>(base.Owner.Creature, 1+butterflyTypes, base.Owner.Creature, this);
	}
	protected override void OnUpgrade()
	{
		this.EnergyCost.UpgradeBy(-1);
	}
}

}
