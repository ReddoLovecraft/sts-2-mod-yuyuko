using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SakuraFlowerSakuraFubukiKomachi : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move), new CardsVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];

		public SakuraFlowerSakuraFubukiKomachi() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

			int count = base.DynamicVars.Cards.IntValue;
			if (count > 0)
			{
				await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
					.FromCard(this)
					.Targeting(cardPlay.Target)
					.WithHitCount(count)
					.OnlyPlayAnimOnce()
					.Execute(choiceContext);

				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, count, base.CombatState), PileType.Draw, addedByPlayer: true, position: CardPilePosition.Random));
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1m);
		}
	}
}

