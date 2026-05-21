using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Cards;

namespace TH_Yuyuko.Scripts.Patches
{
    
	[HarmonyPatch(typeof(CardPileCmd), "Add", [typeof(IEnumerable<CardModel>), typeof(CardPile), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)])]
	public static class YuyukoDeceasedRelicPickupPatch
	{
		static void Postfix(IEnumerable<CardModel> cards, CardPile newPile)
		{
			if (newPile.Type != PileType.Deck)
			{
				return;
			}

			foreach (CardModel card in cards)
			{
				if (card is DeceasedRelic)
				{
					TaskHelper.RunSafely(CardPileCmd.AddCurseToDeck<Recall>(card.Owner));
				}
			}
		}
	}
}