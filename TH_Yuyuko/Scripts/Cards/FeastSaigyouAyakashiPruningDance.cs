using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class FeastSaigyouAyakashiPruningDance : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move), new CardsVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SpringPower>()];

		public FeastSaigyouAyakashiPruningDance() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int spring = base.Owner.Creature.GetPowerAmount<SpringPower>();
			decimal damage = base.DynamicVars.Damage.BaseValue + spring;
			SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
			AttackCommand cmd = await DamageCmd.Attack(damage)
				.FromCard(this)
				.TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_giant_horizontal_slash")
				.Execute(choiceContext);
			int dealt = cmd.Results.Sum(r => r.TotalDamage + r.OverkillDamage);
			if (dealt <= 0)
			{
				return;
			}
			GhostDanceOukaRanmanPower.SpawnPetalVfx(dealt);
			int missing = base.Owner.Creature.MaxHp - base.Owner.Creature.CurrentHp;
			if (missing > 0)
			{
				int heal = dealt > missing ? missing : dealt;
				await CreatureCmd.Heal(base.Owner.Creature, heal);
				dealt -= heal;
			}
			if (dealt > 0)
			{
				await CreatureCmd.GainMaxHp(base.Owner.Creature, dealt/base.DynamicVars.Cards.IntValue);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(2m);
			base.DynamicVars.Cards.UpgradeValueBy(-1);
		}
	}
}
