using Godot;

namespace TH_Yuyuko.Scripts.VFX
{
[Tool]
public partial class YuyukoBlackKeyApplier : Node
{
	[Export] public NodePath SpritePath { get; set; } = new NodePath();
	[Export] public bool ForceBlackKey { get; set; } = false;
	[Export] public float Threshold { get; set; } = 0.04f;
	[Export] public float Softness { get; set; } = 0.06f;

	public override void _EnterTree()
	{
		if (Engine.IsEditorHint())
		{
			SetProcess(true);
		}
	}

	public override void _Ready()
	{
		Apply();
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			Apply();
		}
	}

	private void Apply()
	{
		if (SpritePath.IsEmpty)
		{
			return;
		}

		Sprite2D? sprite = GetNodeOrNull<Sprite2D>(SpritePath);
		if (sprite == null)
		{
			return;
		}

		var shader = new Shader
		{
			Code = @"
shader_type canvas_item;
uniform bool force_black_key = false;
uniform float threshold = 0.04;
uniform float softness = 0.06;

void fragment() {
	vec4 c = texture(TEXTURE, UV);
	vec4 out_c = c;
	bool do_key = force_black_key || c.a >= 0.99;
	if (do_key) {
		float lum = max(c.r, max(c.g, c.b));
		float a = smoothstep(threshold, threshold + softness, lum);
		out_c = vec4(c.rgb, a);
	}
	COLOR = out_c;
}
"
		};

		var mat = new ShaderMaterial
		{
			Shader = shader
		};
		mat.SetShaderParameter("force_black_key", ForceBlackKey);
		mat.SetShaderParameter("threshold", Threshold);
		mat.SetShaderParameter("softness", Softness);

		sprite.Material = mat;
	}
}
}
