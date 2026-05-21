using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class EleganceYomotsuEgregoire : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(11), new DynamicVar("Power", 1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<DeathPower>(),
			HoverTipFactory.FromPower<DeathDesirePower>()
		];

		public EleganceYomotsuEgregoire() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			YuyukoVfxManager.PlaySimple("Soul",cardPlay.Target,VfxTargetPositionType.TargetCreature);
			int death = base.DynamicVars.Cards.IntValue;
			await PowerCmd.Apply<DeathPower>(cardPlay.Target, death, base.Owner.Creature, this);
			if (cardPlay.Target.HasPower<DeathDesirePower>())
			{
				await PowerCmd.Apply<DeathPower>(cardPlay.Target, death, base.Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Apply<DeathDesirePower>(cardPlay.Target, base.DynamicVars["Power"].IntValue, base.Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(3);
			base.DynamicVars["Power"].UpgradeValueBy(1);
		}
	}
}
