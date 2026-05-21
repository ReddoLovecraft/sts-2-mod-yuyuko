using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SlowGhost : YuyukoCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SlowPower>()];

		public SlowGhost() : base(1, CardType.Power, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

			foreach (Player player in base.CombatState.Players)
			{
				if (player.Creature.IsAlive)
				{
					await PowerCmd.Apply<SlowPower>(player.Creature, 1, base.Owner.Creature, this);
				}
			}

			foreach (var enemy in base.CombatState.HittableEnemies)
			{
				if (enemy.IsAlive)
				{
					await PowerCmd.Apply<SlowPower>(enemy, 1, base.Owner.Creature, this);
				}
			}

			await PowerCmd.Apply<SlowGhostPower>(base.Owner.Creature, 1, base.Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
