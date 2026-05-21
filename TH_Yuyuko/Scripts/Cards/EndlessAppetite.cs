using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class EndlessAppetite : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			base.EnergyHoverTip,
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
		];
		protected override bool ShouldGlowGoldInternal => PileType.Hand.GetPile(base.Owner).Cards.Any(c => c.Type==CardType.Status||c.Type==CardType.Curse);
		public EndlessAppetite() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int handCount = PileType.Hand.GetPile(base.Owner).Cards.Count;
			if (handCount <= 0)
			{
				return;
			}
			VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, "vfx/vfx_bite");
			CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1, 1);
			CardModel selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this)).First();
			bool doubled = selected.Type == CardType.Status || selected.Type == CardType.Curse;
			await CardCmd.Exhaust(choiceContext, selected);
			int energy = base.DynamicVars.Energy.IntValue;
			if (doubled)
			{
				energy *= 2;
			}
			await PlayerCmd.GainEnergy(energy, base.Owner);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Energy.UpgradeValueBy(1);
		}
	}
}
