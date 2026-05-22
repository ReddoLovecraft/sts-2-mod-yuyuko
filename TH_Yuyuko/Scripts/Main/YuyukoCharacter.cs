using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Yuyuko.Scripts.Cards;
using TH_Yuyuko.Scrpits.Relics;

namespace TH_Yuyuko.Scripts.Main
{
	public class YuyukoCharacter : PlaceholderCharacterModel
	{
		public override Color NameColor => new Color("d05a82ff");
		public override Color EnergyLabelOutlineColor => new Color("fb90adff");
		public override Color DialogueColor => new Color("fa7ea8ff");
		public override Color MapDrawingColor => new Color("f87cc1ff");
		public override Color RemoteTargetingLineColor => new Color("f9a7c9ff");
		public override Color RemoteTargetingLineOutline => new Color("f85492ff");
		public override CharacterGender Gender => CharacterGender.Feminine;
		public override bool ShouldAlwaysShowStarCounter => false;
		public override int StartingHp => 80;
		public override string CustomVisualPath => "res://TH_Yuyuko/Artworks/Character/yuyuko.tscn";
		public override string CustomTrailPath => "res://TH_Yuyuko/Artworks/VFX/YuyukoCardTrail.tscn";
		public override string CustomIconTexturePath => "res://TH_Yuyuko/Artworks/Character/yuyuko_icon.png";
		public override string CustomIconPath => "res://TH_Yuyuko/Artworks/Character/yuyuko_icon.tscn";
		public override string CustomEnergyCounterPath => "res://TH_Yuyuko/Artworks/Character/yuyuko_energy_counter.tscn";
		// 篝火休息动画。
		public override string CustomRestSiteAnimPath => "res://TH_Yuyuko/Artworks/Character/yuyukorest.tscn";
		// 商店人物动画。
		public override string CustomMerchantAnimPath => "res://TH_Yuyuko/Artworks/Character/yuyuko_merchant.tscn";
		public override string CustomArmPointingTexturePath => "res://TH_Yuyuko/Artworks/Character/multiplayer_hand_yuyuko_point.png";
		public override string CustomArmRockTexturePath => "res://TH_Yuyuko/Artworks/Character/multiplayer_hand_yuyuko_rock.png";
		public override string CustomArmPaperTexturePath => "res://TH_Yuyuko/Artworks/Character/multiplayer_hand_yuyuko_paper.png";
		public override string CustomArmScissorsTexturePath => "res://TH_Yuyuko/Artworks/Character/multiplayer_hand_yuyuko_scissors.png";
		public override string CustomCharacterSelectBg => "res://TH_Yuyuko/Artworks/Character/Yuyuko_bg.tscn";
		public override string CustomCharacterSelectIconPath => "res://TH_Yuyuko/Artworks/Character/char_select_yuyuko.png";
		public override string CustomCharacterSelectLockedIconPath => "res://TH_Yuyuko/Artworks/Character/char_select_yuyuko_locked.png";
		public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/silent_transition_mat.tres";
		public override string CustomMapMarkerPath => "res://TH_Yuyuko/Artworks/Character/map_marker_yuyuko.png";
		// 攻击音效
		public override string CustomAttackSfx => YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/attack.wav");
		// 施法音效
		public override string CustomCastSfx => YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/cast.wav");
		// 死亡音效
		public override string CustomDeathSfx => YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/die.ogg");
		public override string CharacterSelectSfx  => YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/characterselect.wav");
		public override string CharacterTransitionSfx => YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/transition.wav");
		public override CardPoolModel CardPool => ModelDb.CardPool<YuyukoCardPool>();
		public override RelicPoolModel RelicPool => ModelDb.RelicPool<YuyukoRelicPool>();
		public override PotionPoolModel PotionPool => ModelDb.PotionPool<YuyukoPotionPool>();

		// 初始卡组
		public override IEnumerable<CardModel> StartingDeck => [
			ModelDb.Card<Strike>(),
			ModelDb.Card<Strike>(),
			ModelDb.Card<Strike>(),
			ModelDb.Card<Strike>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<NoLifeDream>(),
			ModelDb.Card<DeathDesire>()

	];
		// 初始遗物
		public override IReadOnlyList<RelicModel> StartingRelics => [
			ModelDb.Relic<SealedSaigyouAyakashi>()
	];

		// 攻击建筑师的攻击特效列表
		public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_bite",
		"vfx/vfx_bite",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_bite",
		"vfx/vfx_bite",
		"vfx/vfx_bloody_impact",
        "vfx/vfx_heavy_blunt",
		];
		public override CreatureAnimator GenerateAnimator(MegaSprite controller)
		{
			AnimState animState = new AnimState("idle", isLooping: true);
			AnimState animState2 = new AnimState("cast");
			AnimState animState3 = new AnimState("attack");
			AnimState animState4 = new AnimState("hit");
			AnimState animState6 = new AnimState("summon");
			AnimState state = new AnimState("die");
			AnimState animState5 = new AnimState("relaxed_loop", isLooping: true);
			animState2.NextState = animState;
			animState3.NextState = animState;
			animState4.NextState = animState;
			animState6.NextState = animState;
			animState5.AddBranch("Idle", animState);
			CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
			creatureAnimator.AddAnyState("Idle", animState);
			creatureAnimator.AddAnyState("Dead", state);
			creatureAnimator.AddAnyState("Hit", animState4);
			creatureAnimator.AddAnyState("Attack", animState3);
			creatureAnimator.AddAnyState("Cast", animState2);
			creatureAnimator.AddAnyState("relaxed_loop", animState5);
			creatureAnimator.AddAnyState("Summon", animState6);
			return creatureAnimator;
		}
	}
}
/*
事件：
全员均为幽幽子时，在二三幕随机出现。
友人之助（aaa隙间无相手17岁美少女）：
删除一张牌。然后任选一张牌获得。
放入一件遗物。然后任选一件遗物获得。
放入一瓶药水。然后任选一瓶药水获得。

存在幽幽子时才可出现。
夜雀食堂（老板娘，唉唉）：
正常点单。 失去全部金币。获得金币/20点最大生命。
我要吃小碎骨。将一张愧疚加入牌组。获得20点最大生命。
离开。（当最大生命不满时，显示 不行我好饿，否则可以正常离开）

将绽的西行妖：
全员均为幽幽子时出现 仅限第三幕。
西行妖快要彻底绽放了，你选择？
促使其彻底绽放。失去40点最大生命。获得绽放印记。
阻止其彻底绽放。失去40点最大生命。获得新生。

偶遇圣白莲
遇到了飙车的鬼火尼姑。
交流佛法。随机升级3张牌。
捎我一程。直接传送到当幕的Boss处。

偶遇阎魔
偶遇了四季映姬。
直接离开。假装看不见。将一张愧疚加入牌组。
上前搭话。↓开启下一阶段事件
聆听说教。删2张牌。将一张苦恼加入牌组。
转移话题。变化2张牌。失去8点最大生命。
直接逃跑。失去10点生命。

隐退的妖忌（存在幽幽子时才出现，仅限前两幕）
遇到了失踪的妖忌，向他请教。
升级3张牌。
变化2张牌。
删除1张牌。

*/