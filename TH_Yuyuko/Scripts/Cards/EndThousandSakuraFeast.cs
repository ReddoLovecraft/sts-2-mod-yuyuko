using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class EndThousandSakuraFeast : YuyukoCardModel
	{
		//public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(17, ValueProp.Move)];

		public EndThousandSakuraFeast() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}
		protected override void OnUpgrade()
		{
			this.DynamicVars.Damage.UpgradeValueBy(4);
			//this.RemoveKeyword(CardKeyword.Ethereal);
		}
		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int sakuraCount = CountSakuraEverywhere(base.Owner);
			decimal damage = base.DynamicVars.Damage.BaseValue;
			for (int i = 0; i < sakuraCount; i++)
			{
				damage *= 2m;
			}
			Color color = new Color("fcaed680");
			double num2 = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2 : 0.3);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(color, 0.8 + (double)Mathf.Min(8, (int)damage) * num2));
			SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
			NRun.Instance?.GlobalUi.AddChildSafely(NSmokyVignetteVfx.Create(color, color));

			await DamageCmd.Attack(damage).FromCard(this).TargetingAllOpponents(base.CombatState).WithHitFx("vfx/vfx_giant_horizontal_slash").Execute(choiceContext);
			decimal capped = damage > 1000000m ? 1000000m : damage;
			int petalCount = Mathf.Clamp((int)(Mathf.Log((float)capped + 1f) * 10f), 14, 160);
			GhostDanceOukaRanmanPower.SpawnPetalVfx(petalCount);
			PlayerCmd.EndTurn(base.Owner, canBackOut: false);
		}

		private static int CountSakuraEverywhere(Player owner)
		{
			int count = 0;
			foreach (PileType pileType in new[] { PileType.Deck, PileType.Draw, PileType.Hand, PileType.Discard, PileType.Exhaust, PileType.Play })
			{
				foreach (var card in pileType.GetPile(owner).Cards)
				{
					if (card is Sakura)
					{
						count++;
					}
				}
			}
			return count;
		}
	}
}
