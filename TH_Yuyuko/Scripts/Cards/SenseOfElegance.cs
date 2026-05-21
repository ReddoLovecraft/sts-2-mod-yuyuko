using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SenseOfElegance : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<DexterityPower>(),
			HoverTipFactory.FromPower<StrengthPower>()
		];

		public SenseOfElegance() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			if (cardPlay.Target.Monster.IntendsToAttack)
			{
				await PowerCmd.Apply<SpeedPotionPower>(base.Owner.Creature, base.DynamicVars["Power"].IntValue, base.Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Apply<FlexPotionPower>(base.Owner.Creature, base.DynamicVars["Power"].IntValue, base.Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars["Power"].UpgradeValueBy(2);
		}
	}
}
