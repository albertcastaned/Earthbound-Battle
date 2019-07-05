using UnityEngine;

public class SlotMove : MonoBehaviour
{

    private bool selected;
    private RectTransform rt;
    
    private float position;

    [SerializeField] private float distance = 10;
    [SerializeField] private float speed = 100;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        position = rt.offsetMin.x;
    }
    void Update()
    {
        if (!selected)
        {
            if (rt.offsetMin.x > position - distance)
            {
                rt.offsetMin += new Vector2(-Time.deltaTime * speed,0);
                rt.offsetMax -= new Vector2(Time.deltaTime * speed,0);
                
            }
        }
        else
        {
            if (rt.offsetMin.x < position + distance)
            {
                rt.offsetMin += new Vector2(Time.deltaTime * speed,0);
                rt.offsetMax -= new Vector2(-Time.deltaTime * speed,0);
            }
        }
    }
    
    public void MoveSelect(bool value)
    {
        
        selected = value;
    }
}
