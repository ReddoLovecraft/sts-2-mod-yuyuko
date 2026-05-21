using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Relics
{
[Pool(typeof(YuyukoRelicPool))]
public class BloomMark : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Yuyuko/Artworks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";

	public override bool HasUponPickupEffect => true;
	public override Task AfterObtained()
	{
		IEnumerable<CardModel> enumerable = PileType.Deck.GetPile(base.Owner).Cards;
		foreach (CardModel item in enumerable)
		{
			CardCmd.Upgrade(item);
		}
		return Task.CompletedTask;
	}

	public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (options.Flags.HasFlag(CardCreationFlags.NoHookUpgrades))
		{
			return false;
		}
		EggRelicHelper.UpgradeValidCards(cardRewards, CardType.Attack, this);
		EggRelicHelper.UpgradeValidCards(cardRewards, CardType.Skill, this);
		EggRelicHelper.UpgradeValidCards(cardRewards, CardType.Power, this);
		return true;
	}

	public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
	{
		if (player == base.Owner)
		{
			EggRelicHelper.UpgradeValidCards(cards, CardType.Attack, this);
			EggRelicHelper.UpgradeValidCards(cards, CardType.Skill, this);
			EggRelicHelper.UpgradeValidCards(cards, CardType.Power, this);
		}
	}

	public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		if (card.Owner != base.Owner)
		{
			return false;
		}
		if (!card.IsUpgradable)
		{
			return false;
		}
		if (card.CurrentUpgradeLevel >= 1)
		{
			return false;
		}
		newCard = base.Owner.RunState.CloneCard(card);
		CardCmd.Upgrade(newCard, CardPreviewStyle.None);
		return true;
	}
}
}
