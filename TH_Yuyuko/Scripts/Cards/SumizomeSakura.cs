using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SumizomeSakura : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>(),HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

		public SumizomeSakura() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<SumizomeSakuraPower>(base.Owner.Creature, 1, base.Owner.Creature, this);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, base.DynamicVars.Cards.IntValue, base.CombatState), PileType.Hand, addedByPlayer: true));
		}

		protected override void OnUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
		}
	}
}
