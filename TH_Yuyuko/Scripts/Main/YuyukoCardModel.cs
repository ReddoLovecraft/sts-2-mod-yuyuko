using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;


namespace TH_Yuyuko.Scripts.Main
{
    public abstract class YuyukoCardModel : CustomCardModel
    {
        public override string PortraitPath => $"res://TH_Yuyuko/Artworks/Cards/{Id.Entry}.png";
	    public override string BetaPortraitPath => CardModel.MissingPortraitPath;
        public YuyukoCardModel(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)
     : base(baseCost, type, rarity, target, showInCardLibrary)
        {
            if (autoAdd)
            {
                CustomContentDictionary.AddModel(GetType());
            }
        }


    }
  
}
