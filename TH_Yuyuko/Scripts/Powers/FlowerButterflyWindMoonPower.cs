using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class FlowerButterflyWindMoonPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("Butterfly"),
			HoverTipFactory.FromPower<StrengthPower>(),
			HoverTipFactory.FromPower<DexterityPower>()
		];

		public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side != base.Owner.Side || !base.Owner.IsPlayer || base.Amount <= 0)
			{
				return;
			}

			int types = 0
				+ (ToolBox.GetButterfliesByKind(base.Owner, ToolBox.ButterflyKind.Death) > 0 ? 1 : 0)
				+ (ToolBox.GetButterfliesByKind(base.Owner, ToolBox.ButterflyKind.Soul) > 0 ? 1 : 0)
				+ (ToolBox.GetButterfliesByKind(base.Owner, ToolBox.ButterflyKind.Energy) > 0 ? 1 : 0);
			if (types <= 0)
			{
				return;
			}

			Flash();
			int gain = types * base.Amount;
			await PowerCmd.Apply<StrengthPower>(base.Owner, gain, base.Owner, null);
			await PowerCmd.Apply<DexterityPower>(base.Owner, gain, base.Owner, null);
		}
	}
}
