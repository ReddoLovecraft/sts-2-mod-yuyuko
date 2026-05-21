using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class SakuraUnattachedBanquetPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>(), HoverTipFactory.FromKeyword(CardKeyword.Retain)];

		private readonly HashSet<CardModel> _temporaryRetain = new();

		public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side != base.Owner.Side || base.Owner.Player == null)
			{
				return;
			}

			CardPile hand = PileType.Hand.GetPile(base.Owner.Player);
			int count = hand.Cards.Count;
			if (count <= 0)
			{
				return;
			}

			var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, count)
			{
				Cancelable = true
			};

			List<CardModel> selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner.Player, prefs, null, null)).ToList();
			if (selected.Count <= 0)
			{
				return;
			}

			foreach (CardModel card in selected)
			{
				if (card.Keywords.Contains(CardKeyword.Retain))
				{
					continue;
				}
				card.AddKeyword(CardKeyword.Retain);
				_temporaryRetain.Add(card);
			}

			int sakuraCount = selected.Count;
			if (base.CombatState != null)
			{
				Flash();
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner.Player, sakuraCount, base.CombatState), PileType.Discard, addedByPlayer: true));
			}
		}

		public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side != base.Owner.Side || _temporaryRetain.Count == 0)
			{
				return Task.CompletedTask;
			}

			foreach (CardModel card in _temporaryRetain)
			{
				card.RemoveKeyword(CardKeyword.Retain);
			}
			_temporaryRetain.Clear();
			return Task.CompletedTask;
		}
	}
}
