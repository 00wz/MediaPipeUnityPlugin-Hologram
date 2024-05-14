using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe;
using UnityEngine.UI;

namespace HologramPipe
{
  public class IrisTracker : ImageSourceSolution<IrisTrackingGraph>
  {
    [SerializeField] private FaceLandmarkListAnnotationController _faceLandmarksWithIrisAnnotationController;
    [SerializeField] private Text _queueLength;
    [SerializeField] private Text _calculationTimeText;

    private ThreadSafeProperty<IrisPositionStruct?> _irisPositions = new();

    /// <summary>
    /// The position of the irises in relation to the camera
    /// </summary>
    public IrisPositionStruct? IrisPositions
    {
      get => _irisPositions.value;
      private set => _irisPositions.value = value;
    }

    public float FocalLengthPixel { get; set; } = 1040f;//TODO: calculate by resolution and ratio of sensor size to focal length
    
    private const float IRIS_SIZE_MM = 11.8f;
    private const int FACE_LANDMARK_COUNT = 468;
    private const int IRIS_LANDMARK_COUNT = 5;

    private const int LEFT_IRIS_LANDMARKS_FIRST_INDEX = FACE_LANDMARK_COUNT;
    private const int RIGHT_IRIS_LANDMARKS_FIRST_INDEX = LEFT_IRIS_LANDMARKS_FIRST_INDEX + IRIS_LANDMARK_COUNT;
    private const int RIGHT_IRIS_LANDMARKS_OUT_INDEX = RIGHT_IRIS_LANDMARKS_FIRST_INDEX + IRIS_LANDMARK_COUNT;

    private Queue<Quaternion> _quaternions = new();
    //private Quaternion _lastInputStreamAttitude;
    private float _lastCalculationFrameTime = 0f;
    private float _calculationTimeInteval;

    private void RefreshComputeTimeInteval()
    {
      _calculationTimeInteval = Time.realtimeSinceStartup - _lastCalculationFrameTime;
      _lastCalculationFrameTime = Time.realtimeSinceStartup;
      _calculationTimeText.text = _calculationTimeInteval.ToString();
    }

    public void SetTrackerActive(bool value)
    {
      if(value)
      {
        Play();
      }
      else
      {
        Stop();
      }
    }
      
    protected override void OnStartRun()
    {
      if (!runningMode.IsSynchronous())
      {
        graphRunner.OnFaceLandmarksWithIrisOutput += OnFaceLandmarksWithIrisOutput;
      }

      var imageSource = ImageSourceProvider.ImageSource;
      /*Debug.LogWarning($"imageSource.resolution: {imageSource.resolution}\n" +
        $"imageSource.textureHeight: {imageSource.textureHeight}\n" +
        $"imageSource.textureWidth: {imageSource.textureWidth}" +
        $"imageSource.rotation: {imageSource.rotation}\n" +
        $"imageSource.isFrontFacing: {imageSource.isFrontFacing}\n");*/
      SetupAnnotationController(_faceLandmarksWithIrisAnnotationController, imageSource);
    }

    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      _quaternions.Enqueue(Input.gyro.attitude);
      //_lastInputStreamAttitude = Input.gyro.attitude;
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
      RefreshEyesPosition(faceLandmarksWithIris?.Landmark);
      RefreshComputeTimeInteval();
    }

    private void OnFaceLandmarksWithIrisOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
    {
      _faceLandmarksWithIrisAnnotationController.DrawLater(eventArgs.value);
      RefreshEyesPosition(eventArgs.value?.Landmark);
    }

    private void RefreshEyesPosition(IList<NormalizedLandmark> landmarks)
    {
      if (landmarks == null ||
        !TryPartitionLandmarkList(landmarks, out var rightLandmarks, out var leftLandmarks))
      {
        _quaternions.Dequeue();
        IrisPositions = null;
        return;
      }

      var leftIrisPositionPx = GetLandmarkPositionPx(leftLandmarks[0],
        ImageSourceProvider.ImageSource.resolution, FocalLengthPixel);
      var leftIrisDiameterPx = CalculateIrisDiameterPx(ImageSourceProvider.ImageSource.resolution,
        leftLandmarks);
      var leftIrisPositionMM = leftIrisPositionPx / leftIrisDiameterPx * IRIS_SIZE_MM;

      var rightIrisPositionPx = GetLandmarkPositionPx(rightLandmarks[0],
        ImageSourceProvider.ImageSource.resolution, FocalLengthPixel);
      var rightIrisDiameterPx = CalculateIrisDiameterPx(ImageSourceProvider.ImageSource.resolution,
        rightLandmarks);
      var rightIrisPositionMM = rightIrisPositionPx / rightIrisDiameterPx * IRIS_SIZE_MM;

      IrisPositions = new IrisPositionStruct
      {
        LeftIrisPositionMM = leftIrisPositionMM,
        RightIrisPositionMM = rightIrisPositionMM,
        Attitude = _quaternions.Dequeue()
        //Attitude = _lastInputStreamAttitude
      };
    }

    private bool TryPartitionLandmarkList(IList<NormalizedLandmark> landmarks, 
      out List<NormalizedLandmark> rightIrisLandmarks, out List<NormalizedLandmark> leftIrisLandmarks)
    {
      if (landmarks.Count < RIGHT_IRIS_LANDMARKS_OUT_INDEX)
      {
        rightIrisLandmarks = null;
        leftIrisLandmarks = null;
        return false;
      }

      rightIrisLandmarks = new List<NormalizedLandmark>(IRIS_LANDMARK_COUNT);
      for (int i = LEFT_IRIS_LANDMARKS_FIRST_INDEX; i < RIGHT_IRIS_LANDMARKS_FIRST_INDEX; i++)
      {
        rightIrisLandmarks.Add(landmarks[i]);
      }

      leftIrisLandmarks = new List<NormalizedLandmark>(IRIS_LANDMARK_COUNT);
      for (int i = RIGHT_IRIS_LANDMARKS_FIRST_INDEX; i < RIGHT_IRIS_LANDMARKS_OUT_INDEX; i++)
      {
        leftIrisLandmarks.Add(landmarks[i]);
      }

      return true;
    }

    private Vector3 GetLandmarkPositionPx(NormalizedLandmark normalizedLandmark,
      ImageSource.ResolutionStruct resolution, float focalLengthPixel)
    {
      return new Vector3(
        (normalizedLandmark.X - 0.5f) * resolution.width,
        (0.5f - normalizedLandmark.Y) * resolution.height,
        focalLengthPixel
        );
    }

    private float CalculateIrisDiameterPx(ImageSource.ResolutionStruct resolution, IList<NormalizedLandmark> target)
    {
      var sqrDiam1 = CalculateSqrDistancePx(resolution, target[1], target[3]);
      var sqrDiam2 = CalculateSqrDistancePx(resolution, target[2], target[4]);
      return Mathf.Sqrt((sqrDiam1 + sqrDiam2) * 0.5f);
    }

    private float CalculateSqrDistancePx(ImageSource.ResolutionStruct resolution,
      NormalizedLandmark a, NormalizedLandmark b)
    {
      var x = (a.X - b.X) * resolution.width;
      var y = (a.Y - b.Y) * resolution.height;
      return (x * x) + (y * y);
    }
    /*
    private Vector3 GetLandmarkPositionPx(NormalizedLandmark normalizedLandmark, 
      ImageSource.ResolutionStruct resolution, bool isInverted, bool IsXReversed, bool IsYReversed)
    {
      var (nx, ny) = isInverted ?
        (normalizedLandmark.Y, normalizedLandmark.X) :
        (normalizedLandmark.X, normalizedLandmark.Y);
      var x = IsXReversed ? Mathf.LerpUnclamped(xMax, xMin, nx) : Mathf.LerpUnclamped(xMin, xMax, nx);
      var y = IsYReversed ? Mathf.LerpUnclamped(yMax, yMin, ny) : Mathf.LerpUnclamped(yMin, yMax, ny);
      var z = zScale * normalizedZ;
      return new Vector3(x, y, z);

    }
    */
    private void Update()
    {
      _queueLength.text = _quaternions.Count.ToString();
    }
  }
}
