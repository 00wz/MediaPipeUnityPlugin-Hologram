using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe;

namespace Hologrampipe
{
  public class IrisTracker : ImageSourceSolution<IrisTrackingGraph>
  {
    [SerializeField] private DetectionListAnnotationController _faceDetectionsAnnotationController;
    [SerializeField] private NormalizedRectAnnotationController _faceRectAnnotationController;
    [SerializeField] private FaceLandmarkListAnnotationController _faceLandmarksWithIrisAnnotationController;

    protected override void OnStartRun()
    {
      if (!runningMode.IsSynchronous())
      {
        graphRunner.OnFaceLandmarksWithIrisOutput += OnFaceLandmarksWithIrisOutput;
      }

      var imageSource = ImageSourceProvider.ImageSource;
      SetupAnnotationController(_faceDetectionsAnnotationController, imageSource);
      SetupAnnotationController(_faceRectAnnotationController, imageSource);
      SetupAnnotationController(_faceLandmarksWithIrisAnnotationController, imageSource);
    }

    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      graphRunner.AddTextureFrameToInputStream(textureFrame);
    }

    protected override IEnumerator WaitForNextValue()
    {
      NormalizedLandmarkList faceLandmarksWithIris = null;

      if (runningMode == RunningMode.Sync)
      {
        var _ = graphRunner.TryGetNext(out faceLandmarksWithIris, true);
      }
      else if (runningMode == RunningMode.NonBlockingSync)
      {
        yield return new WaitUntil(() => graphRunner.TryGetNext(out faceLandmarksWithIris, false));
      }

      _faceLandmarksWithIrisAnnotationController.DrawNow(faceLandmarksWithIris);
    }

    private void OnFaceLandmarksWithIrisOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
    {
      _faceLandmarksWithIrisAnnotationController.DrawLater(eventArgs.value);
    }

  }
}
