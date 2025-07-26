using Hacknet;
using Hacknet.Effects;
using Microsoft.Xna.Framework;
using Pathfinder.Util;


public class EnableScreenGlitch : Pathfinder.Action.DelayablePathfinderAction
{
    public override void Trigger(OS os)
    {
        PostProcessor.EndingSequenceFlashOutActive = true;
        PostProcessor.EndingSequenceFlashOutPercentageComplete = GlitchMult;

        Rectangle fullscreenRect = PostProcessor.GetFullscreenRect();
        FlickeringTextEffect.DrawFlickeringSprite(PostProcessor.sb, fullscreenRect, PostProcessor.target, 12f * GlitchMult, 0f, null, Color.White);

    }
    [XMLStorage]
    float GlitchMult = 1f;


}
public class DisableScreenGlitch : Pathfinder.Action.DelayablePathfinderAction
{
    public override void Trigger(OS os)
    {
        PostProcessor.EndingSequenceFlashOutActive = false;
        PostProcessor.EndingSequenceFlashOutPercentageComplete = 0f;
    }
}    

