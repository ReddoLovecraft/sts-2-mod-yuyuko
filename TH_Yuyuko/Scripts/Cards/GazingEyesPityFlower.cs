using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GazingEyesPityFlower : YuyukoCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];

		public GazingEyesPityFlower() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<GazingEyesPityFlowerPower>(base.Owner.Creature, 1, base.Owner.Creature, this);
		}
		protected override void OnUpgrade()
		{
			this.AddKeyword(CardKeyword.Innate);
		}
	}
}
