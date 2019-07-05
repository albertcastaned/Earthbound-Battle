using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMeterValue : MonoBehaviour
{

    public MeterRoll meter1;
    public MeterRoll meter2;
    public MeterRoll meter3;

    public int value;

    public int Value => value;

    public bool rolling;
    public bool Rolling => rolling;

    // Start is called before the first frame update
    public void SetValue(int num1, int num2, int num3)
    {
        meter1.SetValue(num1);
        meter2.SetValue(num2);
        meter3.SetValue(num3);
    }
    

    // Update is called once per frame
    void Update()
    {

        if (meter3.GetRolling() || meter2.GetRolling() || meter1.GetRolling())
        {
            rolling = true;
            return;
        }
        value = (Mathf.RoundToInt((meter1.transform.localPosition.y + 26f) / 5.2f) * 100);
        
        value += (Mathf.RoundToInt((meter2.transform.localPosition.y + 26f) / 5.2f) * 10);

        value += (Mathf.RoundToInt((meter3.transform.localPosition.y + 26f) / 5.2f) * 1);

        rolling = false;
    }

    public int GetValue()
    {
        value = (Mathf.RoundToInt((meter1.transform.localPosition.y + 26f) / 5.2f) * 100);
        
    value += (Mathf.RoundToInt((meter2.transform.localPosition.y + 26f) / 5.2f) * 10);

    value += (Mathf.RoundToInt((meter3.transform.localPosition.y + 26f) / 5.2f) * 1);
    return value;
    }
}
