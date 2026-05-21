using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class HakugyokurouGhostPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<StrengthPower>(),
			HoverTipFactory.FromPower<DexterityPower>(),
			HoverTipFactory.FromPower<WeakPower>(),
			HoverTipFactory.FromPower<FrailPower>(),
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
		];

		private bool _exhaustedAnyCardThisTurn;

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			if (base.CombatState != null)
			{
				_exhaustedAnyCardThisTurn = CombatManager.Instance.History.Entries
					.OfType<CardExhaustedEntry>()
					.Any(e => e.HappenedThisTurn(base.CombatState) && e.Actor == base.Owner);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side == base.Owner.Side)
			{
				_exhaustedAnyCardThisTurn = false;
			}
			return Task.CompletedTask;
		}

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
		{
			if (card.Owner?.Creature != base.Owner)
			{
				return;
			}

			_exhaustedAnyCardThisTurn = true;
			if (base.Amount <= 0)
			{
				return;
			}

			Flash();
			VfxCmd.PlayOnCreatureCenter(base.Owner, "vfx/vfx_bite");
			await PowerCmd.Apply<StrengthPower>(base.Owner, base.Amount, base.Owner, null);
			await PowerCmd.Apply<DexterityPower>(base.Owner, base.Amount, base.Owner, null);
		}

		public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side != base.Owner.Side || _exhaustedAnyCardThisTurn)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<WeakPower>(base.Owner, 2, base.Owner, null);
			await PowerCmd.Apply<FrailPower>(base.Owner, 2, base.Owner, null);
		}
	}
}
