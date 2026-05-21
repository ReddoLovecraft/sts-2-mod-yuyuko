using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class ElegantSpiritPhotoPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override bool IsInstanced => true;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		private bool _triggeredThisTurn;

		public override Task AfterApplied(MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, CardModel? cardSource)
		{
			if (base.CombatState != null)
			{
				_triggeredThisTurn = CombatManager.Instance.History.Entries
					.OfType<CardPlayStartedEntry>()
					.Any(e => e.HappenedThisTurn(base.CombatState) && e.Actor == base.Owner && e.CardPlay.IsFirstInSeries);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side == base.Owner.Side)
			{
				_triggeredThisTurn = false;
			}
			return Task.CompletedTask;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (_triggeredThisTurn || cardPlay.Card.Owner?.Creature != base.Owner)
			{
				return;
			}

			_triggeredThisTurn = true;
			Flash();
			YuyukoVfxManager.PlayPhotoFlashOverlay();

			CardModel copy = cardPlay.Card.CreateClone();
			if (!copy.Keywords.Contains(CardKeyword.Ethereal))
			{
				copy.AddKeyword(CardKeyword.Ethereal);
			}
			await CardPileCmd.Add(copy, PileType.Hand);
		}
	}
}
