using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class FlowerDancingGhost : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal,CardKeyword.Innate];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new EnergyVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip, HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		public FlowerDancingGhost() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}
		protected override void OnUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
		}
		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<FlowerDancingGhostPower>(base.Owner.Creature, base.DynamicVars.Cards.IntValue, base.Owner.Creature, this);
		}
	}
}
