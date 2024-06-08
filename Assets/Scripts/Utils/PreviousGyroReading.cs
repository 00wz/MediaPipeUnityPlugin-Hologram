using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviousGyroReading : MonoBehaviour
{
  private struct GyroscopeReading
  {
    public GyroscopeReading(Quaternion attitude, float timestamp)
    {
      this.attitude = attitude;
      this.timestamp = timestamp;
    }

    public Quaternion attitude;
    public float timestamp;
  }

  [SerializeField]
  private float _delayTime = 0.25f;
  [SerializeField]
  private Text _delayText;

  public float delayTime
  {
    get => _delayTime;
    set
    {
      _delayTime = Mathf.Max(0f, value);
      _delayText.text = _delayTime.ToString();
    }
  }

  private Queue<GyroscopeReading> _gyroscopeReadings = new();
  private GyroscopeReading _oldestGyroscopeReading;

  private void Start()
  {
    _oldestGyroscopeReading = new GyroscopeReading
    {
      attitude = Input.gyro.attitude,
      timestamp = Time.realtimeSinceStartup
    };
    _gyroscopeReadings.Enqueue(_oldestGyroscopeReading);

    _delayText.text = _delayTime.ToString();
  }

  private void Update()
  {
    _gyroscopeReadings.Enqueue(new GyroscopeReading
    {
      attitude = Input.gyro.attitude,
      timestamp = Time.realtimeSinceStartup
    });

    float previousTime = Time.realtimeSinceStartup - _delayTime;
    while (_gyroscopeReadings.Count > 1 && _gyroscopeReadings.Peek().timestamp < previousTime)
    {
      _oldestGyroscopeReading = _gyroscopeReadings.Dequeue();
    }
  }

  public Quaternion GetPreviousGyroReading()
  {
    /*
    //handling the case when _delayTime ~0
    GyroscopeReading penultimateReading = _gyroscopeReadings.Count > 0?
      _gyroscopeReadings.Peek():
      new GyroscopeReading
      {
        attitude = Input.gyro.attitude,
        timestamp = Time.realtimeSinceStartup
      };
    */
    float previousTime = Time.realtimeSinceStartup - _delayTime;
    float t = (previousTime - _oldestGyroscopeReading.timestamp) /
      (_gyroscopeReadings.Peek().timestamp - _oldestGyroscopeReading.timestamp);
    return Quaternion.Lerp(_oldestGyroscopeReading.attitude,
      _gyroscopeReadings.Peek().attitude, t);
  }
}
