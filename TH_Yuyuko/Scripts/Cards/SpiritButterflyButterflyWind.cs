using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SpiritButterflyButterflyWind : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2),new EnergyVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("SummonButterfly"),
			Tools.GetStaticKeyword("Butterfly"),
			HoverTipFactory.FromPower<ButterflySoulPower>(),
			HoverTipFactory.FromPower<SpiritButterflyButterflyWindPower>(),
			HoverTipFactory.FromKeyword(CardKeyword.Retain),
			base.EnergyHoverTip
		];

		public SpiritButterflyButterflyWind() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Color color = new Color("00cccd80");
			double num2 = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2 : 0.3);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(color, 0.8 + 4 * num2));
			NRun.Instance?.GlobalUi.AddChildSafely(NSmokyVignetteVfx.Create(color, color));
			await ToolBox.SummonButterflies<ButterflySoulPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Cards.IntValue);
			await PowerCmd.Apply<SpiritButterflyButterflyWindPower>(base.Owner.Creature, 1, base.Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
