using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Cards
{
[Pool(typeof(YuyukoCardPool))]
public class RemysteryGensokyoYomigaeri : YuyukoCardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	public RemysteryGensokyoYomigaeri() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		 CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 2);
			List<CardModel> cardsIn = (from c in PileType.Exhaust.GetPile(base.Owner).Cards
                           where c is not RemysteryGensokyoYomigaeri  
                           orderby c.Rarity, c.Id
                           select c).ToList();
		if(cardsIn.Count>0)
		foreach(CardModel card in await CardSelectCmd.FromSimpleGrid(choiceContext, cardsIn, base.Owner, prefs))
		{
			if(card!=null)
				await CardPileCmd.Add(card, PileType.Hand);
		}
	}
	protected override void OnUpgrade()
	{
		this.EnergyCost.UpgradeBy(-1);
	}
}
}
