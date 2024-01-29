using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSView : MonoBehaviour
{
  [SerializeField]
  private Text _fps_text;

    void Update()
    {
      _fps_text.text = (1f / Time.smoothDeltaTime).ToString();
    }
}
