using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
	public sealed class TalismanOneGhostDreamButterfly : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move), new CardsVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("SummonButterfly"),
			Tools.GetStaticKeyword("Butterfly"),
			HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
		];

		public TalismanOneGhostDreamButterfly() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitFx(null, null, "blunt_attack.mp3")
		.WithHitVfxNode(target => YuyukoVfxManager.CreateProjectileToTarget("dream2", Owner.Creature, target, new Vector2(0f, -180f),  new Vector2(0f, -40f))).Execute(choiceContext);
			await ToolBox.SummonButterfliesRandomly(choiceContext, base.Owner.Creature, base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatOrbGeneration);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(2m);
			base.DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
