using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Cards;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class GazingEyesPityFlowerPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromCard<Sakura>(),
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
		];

		private bool _pendingExhaust=false;
		private CardModel? _pendingSakura;

		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target == null||target!=Owner)
			{
				return 1m;
			}
			if (!props.IsPoweredAttack_())
			{
				return 1m;
			}
			if (dealer == null || dealer == base.Owner)
			{
				return 1m;
			}
			CardModel? sakura = FindAnySakuraInNonExhaustPiles();
			if (sakura == null)
			{
				return 1m;
			}
			_pendingExhaust = true;
			_pendingSakura = sakura;
			Flash();
			return 0m;
		}

		public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (!_pendingExhaust || target == null||target!=Owner || !props.IsPoweredAttack_())
			{
				return;
			}
			CardModel? sakura = _pendingSakura;
			_pendingExhaust = false;
			_pendingSakura = null;
			if (sakura != null)
			{
				this.Flash();
				YuyukoVfxManager.PlaySimple("mirror",Owner,VfxTargetPositionType.TargetCreature);
				await CardCmd.Exhaust(choiceContext, sakura);
			}
		}

		private CardModel? FindAnySakuraInNonExhaustPiles()
		{
			if (base.Owner?.Player == null)
			{
				return null;
			}

			foreach (PileType pileType in new[] { PileType.Draw, PileType.Hand, PileType.Discard, PileType.Play })
			{
				CardModel? sakura = pileType.GetPile(base.Owner.Player).Cards.FirstOrDefault(c => c is Sakura);
				if (sakura != null)
				{
					return sakura;
				}
			}
			return null;
		}
	}
}
