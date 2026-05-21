using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class YumeirorakuGhostPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];
		public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
		{
			if ( card.Owner?.Creature != base.Owner)
			{
				return playCount;
			}

			if (!card.Keywords.Contains(CardKeyword.Ethereal))
			{
				return playCount;
			}

			return playCount + base.Amount;
		}

		public override Task AfterModifyingCardPlayCount(CardModel card)
		{
			Flash();
			return Task.CompletedTask;
		}
	}
}
