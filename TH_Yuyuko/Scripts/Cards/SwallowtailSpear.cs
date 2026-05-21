using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SwallowtailSpear : YuyukoCardModel
	{
		protected override bool ShouldGlowGoldInternal =>ToolBox.GetTotalButterflies(base.Owner.Creature) > 0;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Butterfly")];

		public SwallowtailSpear() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
			int butterflyTotal = ToolBox.GetTotalButterflies(base.Owner.Creature);
			int butterflyTypes = 0
				+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Death) > 0 ? 1 : 0)
				+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Soul) > 0 ? 1 : 0)
				+ (ToolBox.GetButterfliesByKind(base.Owner.Creature, ToolBox.ButterflyKind.Energy) > 0 ? 1 : 0);

			decimal damage = base.DynamicVars.Damage.BaseValue + butterflyTotal;
			int hits = 1 + butterflyTypes;

			await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target)
				.WithHitFx(null, null, "dagger_throw.mp3")
				.WithHitVfxNode(target => YuyukoVfxManager.CreateProjectileToTarget("swallowtail_spear", Owner.Creature, target, new Vector2(0f, -180f), new Vector2(0f, -40f)))
				.WithHitCount(hits)
				.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
