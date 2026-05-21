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
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class DeadButterflySpirit : YuyukoCardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move), new CardsVar(1),new EnergyVar(1)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => 
		[
			base.EnergyHoverTip,
			HoverTipFactory.FromPower<ButterflyDeathPower>(),
			HoverTipFactory.FromPower<ButterflyEnergyPower>(),
		];
	protected override bool ShouldGlowGoldInternal =>Owner.HasPower<ButterflyEnergyPower>()||Owner.HasPower<ButterflyDeathPower>();
	public DeadButterflySpirit() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		bool flag1=Owner.HasPower<ButterflyEnergyPower>();
		bool flag2=Owner.HasPower<ButterflyDeathPower>();
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitFx(null, null, "blunt_attack.mp3")
		.WithHitVfxNode(target => YuyukoVfxManager.CreateProjectileToTarget("spirit", Owner.Creature, target, new Vector2(0f, -180f),  new Vector2(0f, -40f)))
		.Execute(choiceContext);
		if(flag1)
		await CardPileCmd.Draw(choiceContext,base.DynamicVars.Cards.IntValue,Owner);
		if(flag2)
		await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue,Owner);
	}
	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1);
		base.DynamicVars.Cards.UpgradeValueBy(1);
		base.DynamicVars.Energy.UpgradeValueBy(1);
	}
}

}
