using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class EnriEshiKinguJodoPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<DeathPower>(),
			HoverTipFactory.FromPower<DeathDesirePower>()
		];

		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (!props.IsPoweredAttack() || dealer != base.Owner || target == null)
			{
				return 1m;
			}

			if (target.GetPowerAmount<DeathPower>() <= 0 && target.GetPowerAmount<DeathDesirePower>() <= 0)
			{
				return 1m;
			}

			return base.Amount <= 0 ? 1m : base.Amount;
		}
	}
}
