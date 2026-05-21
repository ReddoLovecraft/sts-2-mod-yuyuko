using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GhostLead : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move), new CardsVar(1)];

		public GhostLead() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitFx("mod_vfx://spirit", null, "heavy_attack.mp3").Execute(choiceContext);
			await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.IntValue, base.Owner);
			int handCount = PileType.Hand.GetPile(base.Owner).Cards.Count;
			if (handCount <= 0)
			{
				return;
			}

			CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1, 1);
			CardModel selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this)).First();
			await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top, this);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
