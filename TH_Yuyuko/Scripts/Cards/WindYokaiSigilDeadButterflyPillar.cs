using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class WindYokaiSigilDeadButterflyPillar : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("SummonButterfly"),
			Tools.GetStaticKeyword("Butterfly"),
			HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
			HoverTipFactory.FromPower<ButterflyDeathPower>()
		];

		public WindYokaiSigilDeadButterflyPillar() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			YuyukoVfxManager.PlaySimple("dead_butterfly_pillar_smash", cardPlay.Target, YuyukoVfxManager.VfxTargetPositionType.TargetCreature, 0.9f);
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			await ToolBox.SummonButterflies<ButterflyDeathPower>(choiceContext, base.Owner.Creature, 4);

			IEnumerable<WindYokaiSigilDeadButterflyPillar> copies = base.Owner.PlayerCombatState.AllCards.OfType<WindYokaiSigilDeadButterflyPillar>();
			foreach (WindYokaiSigilDeadButterflyPillar card in copies)
			{
				card.BaseReplayCount += 1;
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(4m);
		}
	}
}
