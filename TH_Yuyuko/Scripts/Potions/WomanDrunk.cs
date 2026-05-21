using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Potions;
[Pool(typeof(YuyukoPotionPool))]
public sealed class WomanDrunk : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusedPower>()];
    public override string? CustomPackedImagePath => "res://TH_Yuyuko/Artworks/Potions/WOMAN_DRUNK.png";
    public override string? CustomPackedOutlinePath => "res://TH_Yuyuko/Artworks/Potions/Outlines/WOMAN_DRUNK.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards)
		{
			if (!card.EnergyCost.CostsX)
			{
				card.SetToFreeThisTurn();
			}
		}
       await PowerCmd.Apply<ConfusedPower>(Owner.Creature,1,Owner.Creature,null);
    }
}
