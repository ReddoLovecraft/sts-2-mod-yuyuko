using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class TalismanThreeDistantPast : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move), new CardsVar(3)];

		public TalismanThreeDistantPast() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			int discardCount = PileType.Discard.GetPile(base.Owner).Cards.Count;
			int exhaustCount = PileType.Exhaust.GetPile(base.Owner).Cards.Count;
			decimal damagePerHit = base.DynamicVars.Damage.BaseValue + discardCount * base.DynamicVars.Cards.IntValue;
			int hitCount = 1 + exhaustCount;
			await DamageCmd.Attack(damagePerHit).FromCard(this).Targeting(cardPlay.Target).WithHitCount(hitCount).WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3").Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
			base.DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
