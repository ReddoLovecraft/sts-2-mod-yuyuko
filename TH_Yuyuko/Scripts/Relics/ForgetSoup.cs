using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Relics
{
[Pool(typeof(YuyukoRelicPool))]
public class ForgetSoup : CustomRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;
	public override bool HasUponPickupEffect => true;

	public override string PackedIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
	protected override string PackedIconOutlinePath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
	protected override string BigIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";

	public override async Task AfterObtained()
	{
		List<CardModel> removableCards = base.Owner.Deck.Cards.Where(c => c.IsRemovable).ToList();
		if (removableCards.Count <= 0)
		{
			return;
		}

		Flash();
		for (int i = 0; i < 3; i++)
		{
			if (removableCards.Count <= 0)
			{
				break;
			}
			CardModel card = base.Owner.RunState.Rng.UpFront.NextItem(removableCards);
			removableCards.Remove(card);
			CardCmd.Preview(card, 1.2f, CardPreviewStyle.MessyLayout);
			await Cmd.CustomScaledWait(0.3f, 0.5f);
			await CardPileCmd.RemoveFromDeck(card, showPreview: false);
		}
	}
}
}
