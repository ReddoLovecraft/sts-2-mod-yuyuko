using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class DeceasedRelicPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override bool IsInstanced => true;

		private int _enemyDeathsThisCombat;

		public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
		{
			if (!wasRemovalPrevented && base.Owner.Player != null && target.Side != base.Owner.Side)
			{
				_enemyDeathsThisCombat++;
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatEnd(CombatRoom room)
		{
			if (_enemyDeathsThisCombat <= 0 || base.Owner.Player == null)
			{
				return Task.CompletedTask;
			}

			Flash();
			for (int i = 0; i < _enemyDeathsThisCombat; i++)
			{
				room.AddExtraReward(base.Owner.Player, new RelicReward(base.Owner.Player));
			}
			return Task.CompletedTask;
		}
	}
}
