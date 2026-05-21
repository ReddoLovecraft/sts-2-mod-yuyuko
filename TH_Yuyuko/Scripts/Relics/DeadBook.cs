using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Relics
{
[Pool(typeof(YuyukoRelicPool))]
public class DeadBook : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;
	public override string PackedIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Yuyuko/Artworks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
	bool flag1=true;
	bool flag2=true;
	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
            if (player != base.Owner)
            {
                return;
            }
           this.flag1=true;
           this.flag2=true;
    }
   	public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		if (cardSource == null)
		{
			return amount;
		}
        if(power.Type==PowerType.Buff&&target==base.Owner.Creature&&flag1)
		{
			flag1=false;
			Flash();
			return amount * 2m;
		}
		if(power.Type==PowerType.Debuff&&giver==base.Owner.Creature&&flag2)
		{
			flag2=false;
			Flash();
			return amount * 2m;
		}
		return amount;
	}
}
}
