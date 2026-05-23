using BaseLib.Config;

namespace TH_Yuyuko.Scripts.Main;

[ConfigHoverTipsByDefault]
public sealed class YuyukoModConfig : SimpleModConfig
{
	[ConfigSection("VFX")]
	[ConfigHoverTip]
	public static bool FlowerDancingSnowVfx { get; set; } = true;
}

