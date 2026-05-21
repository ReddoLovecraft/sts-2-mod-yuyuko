using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class YuyukoMilk : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip];

		public YuyukoMilk() : base(0, CardType.Status, CardRarity.Status, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int multiplier = base.Keywords.Contains(CardKeyword.Ethereal) ? 2 : 1;

			int maxHpGain = base.DynamicVars.Cards.IntValue * multiplier;
			if (maxHpGain > 0)
			{
				await CreatureCmd.GainMaxHp(base.Owner.Creature, maxHpGain);
			}

			int energyGain = base.DynamicVars.Energy.IntValue * multiplier;
			if (energyGain > 0)
			{
				await PlayerCmd.GainEnergy(energyGain, base.Owner);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1);
			base.DynamicVars.Energy.UpgradeValueBy(1);
		}
	}
}
