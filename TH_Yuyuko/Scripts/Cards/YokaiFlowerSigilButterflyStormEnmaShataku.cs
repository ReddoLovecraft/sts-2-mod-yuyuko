using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class YokaiFlowerSigilButterflyStormEnmaShataku : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Butterfly")];

		protected override bool ShouldGlowGoldInternal =>
			ToolBox.GetTotalButterflies(base.Owner.Creature) > 0 ||
			(base.CombatState?.HittableEnemies.Any(e => e.Monster.IntendsToAttack) ?? false);

		public YokaiFlowerSigilButterflyStormEnmaShataku() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int butterflies = ToolBox.GetTotalButterflies(base.Owner.Creature);
			int hitCount = 6 + butterflies;
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this)
				.TargetingRandomOpponents(base.CombatState)
				.WithHitCount(hitCount)
				.WithHitFx("vfx/vfx_starry_impact")
				.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
