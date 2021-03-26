using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{

  public GameObject anotherReading;
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

    if (GameRunner.enableButton == false)
    {
      // anotherReading.SetActive(false);
    }
    if (GameRunner.enableButton == true)
    {
      // anotherReading.SetActive(true);
    }
  }
}
