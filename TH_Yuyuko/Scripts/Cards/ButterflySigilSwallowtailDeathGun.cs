using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class ButterflySigilSwallowtailDeathGun : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];
		protected override bool ShouldGlowGoldInternal =>ToolBox.GetTotalButterflies(base.Owner.Creature) > 0;
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("Butterfly"),
			HoverTipFactory.FromPower<DeathPower>()
		];

		public ButterflySigilSwallowtailDeathGun() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Creature? vfxTarget = base.CombatState.HittableEnemies.FirstOrDefault();
			if (vfxTarget != null)
			{
				Node2D? vfx = YuyukoVfxManager.CreateProjectileToTarget(
					"swallowtail_death_gun",
					Owner.Creature,
					vfxTarget,
					new Vector2(0f, -180f),
					new Vector2(0f, -40f)
				);
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
			}

			int butterflyTypes = 0
				+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Death) > 0 ? 1 : 0)
				+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Soul) > 0 ? 1 : 0)
				+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Energy) > 0 ? 1 : 0);

			decimal damage = base.DynamicVars.Damage.BaseValue;
			if (butterflyTypes > 0)
			{
				damage *= (decimal)(1 << butterflyTypes);
			}

			foreach (Creature enemy in base.CombatState.HittableEnemies.ToList())
			{
				AttackCommand attack = await DamageCmd.Attack(damage).FromCard(this).Targeting(enemy)
					.WithHitFx(null, null, "dagger_throw.mp3")
					.Execute(choiceContext);

				if (enemy.IsAlive)
				{
					int dealt = attack.Results.Sum(r => r.TotalDamage + r.OverkillDamage);
					if (dealt > 0)
					{
						await PowerCmd.Apply<DeathPower>(enemy, dealt, base.Owner.Creature, this);
					}
				}
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
