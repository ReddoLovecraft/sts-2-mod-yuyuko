using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class NetherworldGhostGirlPower :YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.ForEnergy(this),
			HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
		];

		public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			await base.AfterApplied(applier, cardSource);

			Player? player = base.Owner.Player;
			if (player == null)
			{
				return;
			}

			MakeAllCombatCardsEthereal(player);
		}
		public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner.Player)
        {
            return amount;
        }
        return amount + (decimal)base.Amount;
    }
		public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
		{
			if (side == base.Owner.Side && base.Amount > 0 && base.Owner.Player != null)
			{
				Flash();
				
			}
		}
		public override Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
		{
			if (card.Owner == base.Owner.Player)
			{
				MakeCardEthereal(card);
			}
			return Task.CompletedTask;
		}
		private static void MakeAllCombatCardsEthereal(Player player)
		{
			foreach (PileType pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Play })
			{
				CardPile pile = pileType.GetPile(player);
				foreach (CardModel card in pile.Cards)
				{
					MakeCardEthereal(card);
				}
			}
		}

		private static void MakeCardEthereal(CardModel card)
		{
			if (!card.Keywords.Contains(CardKeyword.Ethereal))
			{
				card.AddKeyword(CardKeyword.Ethereal);
			}
		}
	}
}
