using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class FujimiNoOnnaPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 4)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<DeathDesirePower>(),
			HoverTipFactory.FromPower<DeathPower>()
		];

		public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side != base.Owner.Side || !base.Owner.IsPlayer)
			{
				return;
			}

			int desire = base.Amount;
			if (desire > 0)
			{
				Flash();
				await PowerCmd.Apply<DeathDesirePower>(combatState.HittableEnemies, desire, base.Owner, null);
			}
		}

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			SyncDisplayVars();
			return base.AfterApplied(applier, cardSource);
		}

		public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (ReferenceEquals(power, this))
			{
				SyncDisplayVars();
			}
			return Task.CompletedTask;
		}

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target == null || !target.IsAlive)
			{
				return;
			}

			if (target.Side == base.Owner.Side)
			{
				return;
			}

			if (result.TotalDamage + result.OverkillDamage <= 0)
			{
				return;
			}

			int death = base.Amount * 4;
			if (death <= 0)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<DeathPower>(target, death, base.Owner, null);
		}

		private void SyncDisplayVars()
		{
			base.DynamicVars["Power"].BaseValue = base.Amount * 4;
			InvokeDisplayAmountChanged();
		}
	}
}
