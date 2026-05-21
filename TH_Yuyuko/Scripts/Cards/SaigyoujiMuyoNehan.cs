using System.Collections.Generic;
using System.Linq;
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

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SaigyoujiMuyoNehan : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<DeathPower>(),
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
		];

		public SaigyoujiMuyoNehan() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

			int mult = base.DynamicVars["Power"].IntValue;
			foreach (var enemy in base.CombatState.HittableEnemies.ToList())
			{
				var death = enemy.GetPower<DeathPower>();
				if (death == null || death.Amount <= 0)
				{
					continue;
				}
				await YuyukoVfxManager.PlayDoomKillVfxOnCreature(enemy);
				await PowerCmd.ModifyAmount(death,death.Amount * (mult-1), base.Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars["Power"].UpgradeValueBy(1);
		}
	}
}
