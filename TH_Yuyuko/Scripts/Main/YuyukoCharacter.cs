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
遗物：

人魂灯 普通遗物
每场战斗开始时，给予所有敌人1层死欲。
人魂灯是冥界的道具，它发出的光，不管相隔多远，不管有没有障碍物，幽灵都可以看见并会被吸引过去。

樱花蛋糕 稀有遗物
拾起时，将你的最大生命提升10。在你的回合开始时，给予所有敌人等同于你25%当前生命层死意。
据说是用西行妖花瓣做成的蛋糕，不知道吃下去会不会死亡。

天冠 普通遗物
在你的回合结束时，如果本回合内你没有打出过攻击牌，受到的伤害减少25%。
日本传统葬礼中覆盖在逝者头上的白色三角巾。

弹幕的亡灵 商店遗物
每当你受到伤害时，如果伤害不高于你的格挡，将伤害降低到0。
西行寺幽幽子的能力，有时能在擦过弹幕时将其消去。对于不可能害怕死亡的亡灵来说
擦弹无法造成任何威胁。

隙间的折叠伞 罕见遗物
每进入2个新房间，将计数增加1点。计数不为0时，你在选择下一层的房间时可以消耗1点计数来无视当前的路线。
能够让普通人也可以使用隙间，但是隙间的大小距离方位都比较固定。

《阿齐夫》稀有遗物
每当你获得卡牌时将其升级。
一本奇特的书籍，书中包罗万象地记载了上到天文，下到地理的知识，以及一些不为人知的隐秘。

孟婆汤 稀有遗物
拾起时，随机移除3张卡牌。
一碗无论怎么看都有些可疑的黄绿色汤汁，喝下去会发生什么？

新生 事件遗物
你不能再获得春度。
在你的回合开始时，获得1层无实体。
抛却了旧日的残躯的你，才是真正天衣无缝的亡灵，肉体的桎梏不再能限制你，你也彻底摆脱了西行妖的威胁。

绽放印记 事件遗物
你不能再回复生命。
你获得的正面效果翻倍。
你给予的负面效果翻倍。
西行妖在你身上留下了一道独特的印记，不愿割舍过去的你复活了死去的自己，但你的寿命也因此有了定数。

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

偶遇妖梦（存在幽幽子时才可出现，仅限前两幕）
幽幽子大人，白玉楼给你吃破产了。
给妖梦一些钱。失去200g。获得1遗物1药水1卡牌。
不管让妖梦自己去搞钱。如果你有200g，获得一张愧疚。否则无事发生。

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