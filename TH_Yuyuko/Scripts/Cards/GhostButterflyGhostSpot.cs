using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GhostButterflyGhostSpot : YuyukoCardModel
	{
		public override bool GainsBlock => true;
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new BlockVar(2m, ValueProp.Move),
			new CardsVar(3)
		];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("SummonButterfly"),
			Tools.GetStaticKeyword("Butterfly"),
		];

		public GhostButterflyGhostSpot() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int butterflies = ToolBox.GetTotalButterflies(base.Owner.Creature);
			decimal blockPerHit = base.DynamicVars.Block.BaseValue + butterflies;
			for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
			{
				await CreatureCmd.GainBlock(base.Owner.Creature, blockPerHit, base.DynamicVars.Block.Props, cardPlay, fast: true);
			}
			await ToolBox.SummonButterfliesRandomly(choiceContext, base.Owner.Creature, base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatOrbGeneration);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
