using System;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
public sealed class GoodDeathSpirit : YuyukoCardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move), new CardsVar(2)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathDesirePower>()];

	public GoodDeathSpirit() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}
	protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<DeathDesirePower>()) ?? false;
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		int desire = cardPlay.Target.GetPowerAmount<DeathDesirePower>();
		int hits = 1 + Math.Max(0, desire);
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitFx(null, null, "blunt_attack.mp3")
		.WithHitVfxNode(target => YuyukoVfxManager.CreateProjectileToTarget("spirit", Owner.Creature, target, new Vector2(0f, -180f),  new Vector2(0f, -40f)))
		.WithHitCount(hits).Execute(choiceContext);
		if (cardPlay.Target.IsAlive)
		{
			await PowerCmd.Apply<DeathDesirePower>(cardPlay.Target, base.DynamicVars.Cards.IntValue, base.Owner.Creature, this);
		}
	}
	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2);
		base.DynamicVars.Cards.UpgradeValueBy(1);
	}
}

}
