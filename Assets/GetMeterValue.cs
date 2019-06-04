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

    public bool GetRolling => meter3.GetRolling();
    // Start is called before the first frame update
    void Start()
    {
        value = ((meter1.transform.localPosition.y + 26f) / 5.2f) >= 10f ? (int)0f : ((int)Mathf.Round((meter1.transform.localPosition.y + 26f) / 5.2f) * 100);
        value += ((meter1.transform.localPosition.y + 26f) / 5.2f) >= 10f ? (int)0f : ((int)Mathf.Round((meter2.transform.localPosition.y + 26f) / 5.2f) * 10);
        value += ((meter1.transform.localPosition.y + 26f) / 5.2f) >= 10f ? (int)0f : ((int)Mathf.Round((meter3.transform.localPosition.y + 26f) / 5.2f) * 1);

    }

    // Update is called once per frame
    void Update()
    {
        if (meter3.GetRolling())
        {
            value = ((meter1.transform.localPosition.y + 26f) / 5.2f) >= 10f ? (int)0f : ((int)Mathf.Round((meter1.transform.localPosition.y + 26f) / 5.2f) * 100);
            value += ((meter1.transform.localPosition.y + 26f) / 5.2f) >= 10f ? (int)0f : ((int)Mathf.Round((meter2.transform.localPosition.y + 26f) / 5.2f) * 10);
            value += ((meter1.transform.localPosition.y + 26f) / 5.2f) >= 10f ? (int)0f : ((int)Mathf.Round((meter3.transform.localPosition.y + 26f) / 5.2f) * 1);
        }
    }
}
