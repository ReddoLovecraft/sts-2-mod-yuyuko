using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class StarvingSpiritFormPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];

		public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
		{
			if (dealer != base.Owner)
			{
				return;
			}
			if (!props.IsPoweredAttack())
			{
				return;
			}
			int dealt = result.TotalDamage + result.OverkillDamage;
			if (dealt <= 0)
			{
				return;
			}
			Flash();
			VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bite");
			if (target.IsAlive)
			{
				await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), target, dealt, false);
			}
			await CreatureCmd.GainMaxHp(base.Owner, dealt*Amount/100m);
		}
	}
}
