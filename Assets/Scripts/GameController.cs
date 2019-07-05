using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController controllerInstance;

    private static Vector3 NessPosition = Vector3.zero;
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (controllerInstance == null) {
            controllerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public static void SetNessPosition(Vector3 newPos)
    {
        NessPosition = newPos;
    }

    public static Vector3 GetNessPosition => NessPosition;

}
