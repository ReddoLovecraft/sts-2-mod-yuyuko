using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SakuraSigilDeathSelf : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new EnergyVar(2), new HpLossVar(3)];
		protected override bool ShouldGlowGoldInternal => PileType.Hand.GetPile(base.Owner).Cards.Any(c => c is Sakura);
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			base.EnergyHoverTip,
			HoverTipFactory.FromCard<Sakura>()
		];

		public SakuraSigilDeathSelf() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await YuyukoVfxManager.PlayDoomKillVfxOnCreature(base.Owner.Creature);

			await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.IntValue, base.Owner);
			await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);

			CardModel? sakuraInHand = PileType.Hand.GetPile(base.Owner).Cards.FirstOrDefault(c => c is Sakura);
			if (sakuraInHand != null)
			{
				await CardCmd.Exhaust(choiceContext, sakuraInHand);
			}
			else
			{
				await CreatureCmd.LoseMaxHp(choiceContext, base.Owner.Creature, base.DynamicVars.HpLoss.BaseValue, isFromCard: true);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1);
			base.DynamicVars.Energy.UpgradeValueBy(1);
		}
	}
}
