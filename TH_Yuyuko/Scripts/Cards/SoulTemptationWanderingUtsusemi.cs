using System;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class SoulTemptationWanderingUtsusemi : YuyukoCardModel
{
	 public override bool GainsBlock => true;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move),new CardsVar(1)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => 
		[
			HoverTipFactory.FromPower<WeakPower>(),
		];

	public SoulTemptationWanderingUtsusemi() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<WeakPower>(CombatState.HittableEnemies, base.DynamicVars.Cards.IntValue, base.Owner.Creature, this);
	}
	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1);
		base.DynamicVars.Block.UpgradeValueBy(2);
	}
}

}
