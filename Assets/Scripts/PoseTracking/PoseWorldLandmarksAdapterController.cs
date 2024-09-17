using Mediapipe;
using Mediapipe.Unity;
using System.Collections.Generic;

public class PoseWorldLandmarksAdapterController : AnnotationController<PoseWorldLandmarksAdapter>
{
  private IList<Landmark> _currentTarget;

  public void DrawNow(IList<Landmark> target)
  {
    _currentTarget = target;
    SyncNow();
  }

  public void DrawNow(LandmarkList target)
  {
    DrawNow(target?.Landmark);
  }

  public void DrawLater(IList<Landmark> target)
  {
    UpdateCurrentTarget(target, ref _currentTarget);
  }

  public void DrawLater(LandmarkList target)
  {
    DrawLater(target?.Landmark);
  }

  protected override void SyncNow()
  {
    isStale = false;
    annotation.HandlePose(_currentTarget);
  }
}
