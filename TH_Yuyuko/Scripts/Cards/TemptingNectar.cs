using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class TemptingNectar : YuyukoCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip, HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<FrailPower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(2)];

		public TemptingNectar() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<WeakPower>(base.Owner.Creature, base.DynamicVars.Cards.IntValue, base.Owner.Creature, this);
			await PowerCmd.Apply<FrailPower>(base.Owner.Creature, base.DynamicVars.Cards.IntValue, base.Owner.Creature, this);
			await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Energy.UpgradeValueBy(1);
		}
	}
}
