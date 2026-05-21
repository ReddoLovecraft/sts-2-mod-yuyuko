using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
public class DanmaGhost : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Shop;
	public override string PackedIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Yuyuko/Artworks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
	    protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
        {
		 	HoverTipFactory.Static(StaticHoverTip.Block)
        });
	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target!=Owner.Creature)
			{
				return 1m;
			}
			if(amount<=Owner.Creature.Block)
			{
				return 0m;
			}
			return 1m;
		}
	
}
}
