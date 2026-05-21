using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class DeadButterflyFloatingMoonPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("SummonButterfly"),
			Tools.GetStaticKeyword("Butterfly"),
			HoverTipFactory.FromPower<ButterflyDeathPower>()
		];

		public async Task Trigger(PlayerChoiceContext choiceContext)
		{
			int deathButterflies = base.Owner.GetPowerAmount<ButterflyDeathPower>();
			if (deathButterflies <= 0)
			{
				return;
			}

			List<Creature> enemies = base.CombatState?.HittableEnemies.Where(e => e.IsAlive).ToList() ?? new List<Creature>();
			if (enemies.Count == 0)
			{
				return;
			}

			Flash();
			foreach (Creature enemy in enemies)
			{
				await CreatureCmd.LoseMaxHp(choiceContext, enemy, deathButterflies, isFromCard: true);
			}
		}
	}
}
