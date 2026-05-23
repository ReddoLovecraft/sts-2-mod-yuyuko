using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class FlowerSpiritGhostButterfly : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("SummonButterfly"),
			Tools.GetStaticKeyword("Butterfly")
		];

		public FlowerSpiritGhostButterfly() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
			AttackCommand attack = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx(null, null, "blunt_attack.mp3")
		.WithHitVfxNode(target => YuyukoVfxManager.CreateProjectileToTarget("dream", Owner.Creature, target, new Vector2(0f, -180f),  new Vector2(0f, -40f)))
		.Execute(choiceContext);

			int unblockedDamage = attack.Results.Sum(r => r.TotalDamage + r.OverkillDamage);
			int butterflies = unblockedDamage / 4;
			if (butterflies > 0)
			{
				await ToolBox.SummonButterfliesRandomly(choiceContext, base.Owner.Creature, butterflies, base.Owner.RunState.Rng.CombatOrbGeneration);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
