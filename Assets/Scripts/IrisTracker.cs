using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe;

namespace Hologrampipe
{
  public class IrisTracker : ImageSourceSolution<IrisTrackingGraph>
  {
    [SerializeField] private FaceLandmarkListAnnotationController _faceLandmarksWithIrisAnnotationController;

    private const int FACE_LANDMARK_COUNT = 468;
    private const int IRIS_LANDMARK_COUNT = 5;

    private const int LEFT_IRIS_LANDMARKS_FIRST_INDEX = FACE_LANDMARK_COUNT;
    private const int RIGHT_IRIS_LANDMARKS_FIRST_INDEX = LEFT_IRIS_LANDMARKS_FIRST_INDEX + IRIS_LANDMARK_COUNT;
    private const int RIGHT_IRIS_LANDMARKS_OUT_INDEX = RIGHT_IRIS_LANDMARKS_FIRST_INDEX + IRIS_LANDMARK_COUNT;

    protected override void OnStartRun()
    {
      if (!runningMode.IsSynchronous())
      {
        graphRunner.OnFaceLandmarksWithIrisOutput += OnFaceLandmarksWithIrisOutput;
      }

      var imageSource = ImageSourceProvider.ImageSource;
      Debug.LogWarning($"imageSource.resolution: {imageSource.resolution}\n" +
        $"imageSource.textureHeight: {imageSource.textureHeight}\n" +
        $"imageSource.textureWidth: {imageSource.textureWidth}");
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
      RefreshEyesPosition(faceLandmarksWithIris.Landmark);
    }

    private void OnFaceLandmarksWithIrisOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
    {
      _faceLandmarksWithIrisAnnotationController.DrawLater(eventArgs.value);
      RefreshEyesPosition(eventArgs.value?.Landmark);
    }

    private void RefreshEyesPosition(IList<NormalizedLandmark> landmarks)
    {
      var (leftLandmarks, rightLandmarks) = PartitionLandmarkList(landmarks);

    }

    private (IList<NormalizedLandmark> leftIris, IList<NormalizedLandmark> rightIris) PartitionLandmarkList(IList<NormalizedLandmark> landmarks)
    {
      if (landmarks == null || landmarks.Count < RIGHT_IRIS_LANDMARKS_FIRST_INDEX)
      {
        return (null, null);
      }

      var leftIrisLandmarks = new List<NormalizedLandmark>(IRIS_LANDMARK_COUNT);
      for (int i = LEFT_IRIS_LANDMARKS_FIRST_INDEX; i < RIGHT_IRIS_LANDMARKS_FIRST_INDEX; i++)
      {
        leftIrisLandmarks.Add(landmarks[i]);
      }

      if (landmarks.Count < RIGHT_IRIS_LANDMARKS_OUT_INDEX)
      {
        return (leftIrisLandmarks, null);
      }

      var rightIrisLandmarks = new List<NormalizedLandmark>(IRIS_LANDMARK_COUNT);
      for (int i = RIGHT_IRIS_LANDMARKS_FIRST_INDEX; i < RIGHT_IRIS_LANDMARKS_OUT_INDEX; i++)
      {
        rightIrisLandmarks.Add(landmarks[i]);
      }

      return (leftIrisLandmarks, rightIrisLandmarks);
    }
  }
}
