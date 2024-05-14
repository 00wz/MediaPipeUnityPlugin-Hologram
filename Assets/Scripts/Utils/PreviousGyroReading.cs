using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviousGyroReading : MonoBehaviour
{
  [SerializeField]
  private float _delayTime = 0.25f;
  [SerializeField]
  private Text _delayText;

  public float delayTime
  {
    get => _delayTime;
    set
    {

    }
  }
  /*
  public void SetDelay(float delay)
  {
    _delayTime = 
  }*/

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

  private Queue<GyroscopeReading> _gyroscopeReadings = new();
  private GyroscopeReading _oldestGyroscopeReading;

  private void Start()
  {
    _oldestGyroscopeReading = new GyroscopeReading
    {
      attitude = Input.gyro.attitude,
      timestamp = Time.realtimeSinceStartup
    };

    _delayText.text = _delayTime.ToString();
  }

  private void Update()
  {
    _gyroscopeReadings.Enqueue(new GyroscopeReading
    {
      attitude = Input.gyro.attitude,
      timestamp = Time.realtimeSinceStartup
    });

    //if(_gyroscopeReadings)
  }
}
