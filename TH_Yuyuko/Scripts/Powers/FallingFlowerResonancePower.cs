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
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class FallingFlowerResonancePower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromCard<Sakura>(),
			HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
		];

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
		{
			if (base.CombatState == null || card.Owner?.Creature != base.Owner)
			{
				return;
			}
			if (card is not Sakura)
			{
				return;
			}
			var enemies = base.CombatState.HittableEnemies;
			if (enemies.Count <= 0 || base.Owner.Player == null)
			{
				return;
			}
			Creature target = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(enemies);
			if (target == null)
			{
				return;
			}
			Flash();
			VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_starry_impact");
			await CreatureCmd.Damage(choiceContext, target, base.Amount, ValueProp.Unpowered, base.Owner, null);
		}
	}
}
