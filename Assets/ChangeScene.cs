using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{
    public Animator animator;

    private string nextLevel;
    private static readonly int FadeOut = Animator.StringToHash("FadeOut");
    
    public void FadeToLevel(string name)
    {
        nextLevel = name;
        animator.SetTrigger(FadeOut);
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene(nextLevel);
    }

}
