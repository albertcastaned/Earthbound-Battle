using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeterRoll : MonoBehaviour {

    public float speed;
    private bool rolling;
    private int damage;
    private float newPos;
    private Vector3 aux;
    public GameObject meter;
    private MeterRoll meterRoll;
    private Vector3 velocity = Vector3.zero;
    private float topHeight;
    public float maxSpeed;
    private float ogSpeed;
    private int amp = 1;
    
    // Use this for initializa1tion
    void Awake () {

        rolling = false;
        meterRoll = meter!=null ? meter.GetComponent<MeterRoll>() : null;
        topHeight = transform.localPosition.y;
        ogSpeed = speed;

    }

    public bool GetRolling() => rolling;
    public void SetValue(int val)
    {
        var transform1 = transform;
        var localPosition = transform1.localPosition;
        localPosition = new Vector3(localPosition.x,localPosition.y + (5.2f * val), localPosition.z);
        transform1.localPosition = localPosition;
        
    }


    public void CalculateDistance(int dmg)
    {
        damage = dmg;
        
        if (rolling)
        {
            if (damage < 0)
            {
                if (velocity.y < 0)
                {
                    newPos += (5.2f * damage);
                    var localPosition = transform.localPosition;
                    aux = new Vector3(localPosition.x, newPos, localPosition.z);
                    return;
                }
                if (velocity.y > 0)
                {
                    speed = ogSpeed;

                    Transform transform1;
                    var delta = (5.2f * Mathf.Round((transform.localPosition.y / 5.2f))) - (transform1 = transform).localPosition.y;
                    var localPosition = transform1.localPosition;
                    newPos = localPosition.y + delta;
                    aux = new Vector3(localPosition.x, newPos, localPosition.z);
                    return;
                }
            }else if (damage>0)
            {
                    if (velocity.y < 0)
                    {

                        speed = ogSpeed;
                        float delta;

                        Transform transform1;
                        delta = (5.2f * Mathf.Round((transform.localPosition.y / 5.2f))) - (transform1 = transform).localPosition.y;
                        var localPosition = transform1.localPosition;
                        newPos = (localPosition.y + delta) + (5.2f * damage);
                        aux = new Vector3(localPosition.x, newPos, localPosition.z);
                        velocity = new Vector3(velocity.x, 0.04f, velocity.z);
                        return;
                    }
                    if (velocity.y > 0)
                    {

                        newPos += (5.2f * damage);
                        var localPosition = transform.localPosition;
                        aux = new Vector3(localPosition.x, newPos, localPosition.z);
                        return;
                    }
                    
            }

        }

        var position = transform.localPosition;
        newPos = position.y + (5.2f * damage);
        rolling = true;
        aux = new Vector3(position.x, newPos, position.z);


    }

    private static float Round(float value, int digits)
    {
        var mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }

    private bool CheckDeath()
    {

        if(meterRoll==null)
        {
            if (Mathf.Approximately(transform.localPosition.y, -26f) || Mathf.Approximately(transform.localPosition.y, 26f))
            {
                return true;
            }

        }
        else if(meterRoll.CheckDeath())
        {
            if (Mathf.Approximately(transform.localPosition.y, -26f) || Mathf.Approximately(transform.localPosition.y, 26f))
                return true;
        }
        else
        {
            return false;
        }
        return false;
    }

    public void SetAmp(int val)
    {
        amp = val;
    }
    // Update is called once per frame
    void Update ()
    {
        
        if (!rolling) return;
        
        if (damage > 0)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aux, ref velocity, speed * amp, maxSpeed / amp * (amp > 1 ? 3 : 1));
            
            if (transform.localPosition.y >= (-topHeight) - 1.5f)
            {
                var transform2 = transform;
                var position = transform2.localPosition;
                position = new Vector3(position.x, topHeight, position.z);
                transform2.localPosition = position;
                newPos += topHeight * 2;
                aux = new Vector3(position.x, newPos, position.z);
                if(meter != null)
                    meterRoll.CalculateDistance(1);
            }
            
        }
        else if (damage < 0)
        {
            if (CheckDeath())
            {
                transform.localPosition = new Vector3(transform.localPosition.x, -26f, transform.localPosition.z);
                rolling = false;
                return;
            }
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aux, ref velocity, speed * amp, maxSpeed / amp * (amp > 1 ? 3 : 1));
            

            if (transform.localPosition.y <= topHeight - 0.2f)
            {

                if (meter == null)
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, topHeight, transform.localPosition.z);
                    rolling = false;
                }
                else if(!CheckDeath())
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, -topHeight, transform.localPosition.z);
                    newPos += -topHeight * 2;
                    aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                    meterRoll.CalculateDistance(-1);
                }


            }

            
        }

        
        if (!FloatEqual(velocity.y, 0f, 0.025f)) return;
        {
            rolling = false;
            newPos = Round(newPos, 3);
            var transform1 = transform;
            var localPosition = transform1.localPosition;
            localPosition = new Vector3(localPosition.x, newPos, localPosition.z);
            transform1.localPosition = localPosition;
        }





    }

    private static bool FloatEqual(float a, float b,float precision)
    {
        return Mathf.Sqrt(a*a + b*b) < precision;
    }
}
