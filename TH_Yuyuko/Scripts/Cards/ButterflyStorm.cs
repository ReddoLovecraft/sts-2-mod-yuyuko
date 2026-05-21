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
public sealed class ButterflyStorm : YuyukoCardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move), new CardsVar(2)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Butterfly")];
	protected override bool HasEnergyCostX => true;
	protected override bool ShouldGlowGoldInternal =>ToolBox.GetTotalButterflies(base.Owner.Creature) > 0;

	public ButterflyStorm() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = ResolveEnergyXValue();
		if (num <= 0)
		{
			return;
		}
		int butterflyTotal = ToolBox.GetTotalButterflies(base.Owner.Creature);
		int butterflyTypes = 0
			+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Death) > 0 ? 1 : 0)
			+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Soul) > 0 ? 1 : 0)
			+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Energy) > 0 ? 1 : 0);
		if (base.Owner.Character is YuyukoCharacter)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Summon", base.Owner.Character.CastAnimDelay);
		}
		float vfxDuration = base.Owner.Character.CastAnimDelay * num;
		if (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Death) > 0)
		{
			YuyukoRadialButterflyBurstVfx.PlayOnCreature(base.Owner.Creature, vfxDuration, "res://TH_Yuyuko/Artworks/VFX/bulletAa000.png", null);
		}
		if (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Soul) > 0)
		{
			YuyukoRadialButterflyBurstVfx.PlayOnCreature(base.Owner.Creature, vfxDuration, "res://TH_Yuyuko/Artworks/VFX/bulletAa001.png", null);
		}
		if (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Energy) > 0)
		{
			YuyukoRadialButterflyBurstVfx.PlayOnCreature(base.Owner.Creature, vfxDuration, "res://TH_Yuyuko/Artworks/VFX/bulletAa003.png", null);
		}
		decimal damage = num + (IsUpgraded ? butterflyTypes : 0);
		int hits = 1 + Math.Max(0, butterflyTotal);
		await DamageCmd.Attack(damage)
			.WithHitCount(hits)
			.FromCard(this)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx(null, null, "blunt_attack.mp3")
			.Execute(choiceContext);
	}
	protected override void OnUpgrade()
	{
		
	}
}

}
