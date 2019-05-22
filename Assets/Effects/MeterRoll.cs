using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeterRoll : MonoBehaviour {

    public float speed;
    public bool rolling;
    public int damage;
    public int damagePrev;
    public float newPos;
    private Vector3 aux;
    public GameObject meter;
    private MeterRoll meterRoll;
    private Vector3 velocity = Vector3.zero;
    private float topHeight;
    public GameObject line;
    public float maxSpeed;
    private Vector3 linePos;
    public int value;
    private float ogSpeed;
    public int id;
    // Use this for initializa1tion
    void Start () {
        
        rolling = false;
        damagePrev = 0;
        if(meter!=null)
        meterRoll = meter.GetComponent<MeterRoll>();
        else
        meterRoll = null;
        linePos = line.transform.position;
        topHeight = transform.localPosition.y;
        ogSpeed = speed;
        SetValue();


    }

    public void SetValue()
    {
        transform.localPosition = new Vector3(transform.localPosition.x,transform.localPosition.y + (5.2f * (float)value), transform.localPosition.z);
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
                    newPos += (5.2f * (float)damage);
                    aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                    return;
                } else if (velocity.y > 0)
                {
                    speed = ogSpeed;

                    float delta;

                    delta = (5.2f * Mathf.Round((transform.localPosition.y / 5.2f))) - transform.localPosition.y;
                    newPos = transform.localPosition.y + delta;
                    aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                    return;
                }
            }else if (damage>0)
            {
                    if (velocity.y < 0)
                    {

                        speed = ogSpeed;
                        float delta;

                        delta = (5.2f * Mathf.Round((transform.localPosition.y / 5.2f))) - transform.localPosition.y;
                        newPos = (transform.localPosition.y + delta) + (5.2f * (float)damage);
                        aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                        velocity = new Vector3(velocity.x, 0.04f, velocity.z);
                        return;
                    }
                    else if (velocity.y > 0)
                    {

                        newPos += (5.2f * (float)damage);
                        aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                        return;
                    }
                    
            }

        }
        newPos = transform.localPosition.y + (5.2f * (float)damage);
        rolling = true;
        aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);


    }
    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
    void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Vector3 og = new Vector3(linePos.x, linePos.y, linePos.z);
        Vector3 line = new Vector3(linePos.x + 50f, linePos.y, linePos.z);
        Gizmos.DrawLine(og, line);

    }
    public bool CheckDeath()
    {
       if(meterRoll==null)
        {
            if (transform.localPosition.y <= topHeight)
                return true;
        }
       else if(meterRoll.CheckDeath())
        {
            if (transform.localPosition.y <= topHeight)
                return true;
        }
        else
        {
            return false;
        }
        return false;
    }
    // Update is called once per frame
    void Update () {

        if (rolling)
        {
            if (damage > 0)
            {
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aux, ref velocity, speed, maxSpeed);
                if (transform.localPosition.y >= (-topHeight) - 0.3f)
                {

                    transform.localPosition = new Vector3(transform.localPosition.x, topHeight, transform.localPosition.z);
                    newPos += topHeight * 2;
                    aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);

                    if (meter != null)
                    {
                        meterRoll.CalculateDistance(1);
                    }

                }

                if (FloatEqual(velocity.y, 0f))
                {
                    rolling = false;
                    damagePrev = damage;
                    newPos = Round(newPos, 3);
                    transform.localPosition = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                }

            }
            else if (damage < 0)
            {

                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aux, ref velocity, speed, maxSpeed);
                if (transform.localPosition.y <= topHeight - 0.2f)
                {
                    if (meter == null)
                    {
                        transform.localPosition = new Vector3(transform.localPosition.x, topHeight, transform.localPosition.z);
                        rolling = false;
                        damagePrev = damage;

                    }
                    else
                    {
                        transform.localPosition = new Vector3(transform.localPosition.x, -topHeight, transform.localPosition.z);
                        newPos += -topHeight * 2;
                        aux = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                        meterRoll.CalculateDistance(-1);
                    }

                }
                if (FloatEqual(velocity.y, 0f))
                {

                    rolling = false;
                    damagePrev = damage;
                    newPos = Round(newPos, 3);
                    transform.localPosition = new Vector3(transform.localPosition.x, newPos, transform.localPosition.z);
                }
            }


        }



        
		
	}

    public bool FloatEqual(float a, float b)
    {
        return Mathf.Sqrt(a*a + b*b) < 0.025f;
    }
}
