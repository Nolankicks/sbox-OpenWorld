using System;
using Sandbox;
using Sandbox.Utility;
[Icon("waves"), Category("Post Processing")]
public sealed class OpiumPostprocess : PostProcess, Component.ExecuteInEditor
{
	IDisposable renderHook;
	[Property] public bool VertexSnapping { get; set; } = true;
	[Property] public bool UsePostProcess { get; set; } = true;

	protected override void OnEnabled()
	{
		renderHook = Camera.AddHookBeforeOverlay( "My Post Processing", 1000, RenderEffect);
	}
	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;
	}
	RenderAttributes attributes = new RenderAttributes();
	public void RenderEffect(SceneCamera camera)
	{
		if (!camera.EnablePostProcessing) return;
		camera.Attributes.SetCombo("D_VERTEX_SNAP", VertexSnapping ? 1 : 0);
		Graphics.GrabFrameTexture( "ColorBuffer", attributes );
        Graphics.GrabDepthTexture( "DepthBuffer", attributes );
		if (UsePostProcess)
		{
			Graphics.Blit(Material.FromShader(Cloud.Shader("facepunch.goback_postprocess")), attributes);
		}

	}
}
