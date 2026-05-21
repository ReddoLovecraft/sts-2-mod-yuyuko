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
	public sealed class FoodInstantDisappearance : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move), new CardsVar(3)];

		public FoodInstantDisappearance() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			decimal damage = base.DynamicVars.Damage.BaseValue;
			if (cardPlay.Target.MaxHp < base.Owner.Creature.MaxHp)
			{
				damage *= base.DynamicVars.Cards.IntValue;
			}

			await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target).WithHitFx("vfx/vfx_bite", null, "heavy_attack.mp3").Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
			base.DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
