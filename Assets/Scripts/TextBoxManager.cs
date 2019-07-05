using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxManager : MonoBehaviour
{

    public GameObject textBox;
    public Image textBoxImage;
    public TextMeshProUGUI theText;
    public TextAsset textFile;
    public string[] textLines;

    public int currentLine;
    public int endAtLine;
    public bool isActive;
    private bool isTyping;
    private bool cancelTyping;
    public float typeSpeed;

    private float size;

    public Movement player;
    // Start is called before the first frame update
    void Start()
    {

        isTyping = false;
        cancelTyping = false;
        size = 0;

        player = FindObjectOfType<Movement>();
        if (textFile != null)
        {
            textLines = (textFile.text.Split('\n'));

        }

        if(endAtLine == 0)
        {
            endAtLine = textLines.Length - 1;
        }
        if(isActive)
        {
            EnableTextBox();
        }
        else
        {
            DisableTextBox();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(!isActive)
        {
            return;
        }


        if (size<1f)
        {
            size += 0.1f;
            textBoxImage.transform.localScale = new Vector3(size, size, size);

        }

        //if (currentLine<=endAtLine)
        //theText.text = textLines[currentLine];

        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            if(!isTyping)
            {
                currentLine += 1;
                if (currentLine > endAtLine)
                {
                    DisableTextBox();
                }
                else
                {
                    StartCoroutine(TextScroll(textLines[currentLine]));
                }
            }
            else if(isTyping && !cancelTyping && size >= 1f)
            {
                cancelTyping = true;
            }

        }

    }
    private IEnumerator Shrink()
    {


        while (size > 0)
        {
            size -= 0.1f;
            textBoxImage.transform.localScale = new Vector3(size, size, size);
            yield return new WaitForSeconds(0.005f);
        }
        textBox.SetActive(false);
        player.canMove = true;
    }

    private IEnumerator TextScroll(string lineOfText)
    {

        int letter = 0;
        
        theText.text = "";
        isTyping = true;
        cancelTyping = false;
        int lineLength = lineOfText.Length - 1;
        while (isTyping && (letter <= lineLength))
        {
            if(lineOfText[letter] != ' ' && lineOfText[letter] != ',' && lineOfText[letter] != '.')
                AudioManager.instance.Play("Text");
            if (cancelTyping)
            {
                typeSpeed = 0.005f;
            }
            else
            {
                if (lineOfText[letter] == ',')
                    typeSpeed = 0.5f;
                else
                    typeSpeed = 0.05f;
            }

            if (lineOfText[letter] == '¬')
            {
                theText.text += "\n";
                letter++;
            }
            else
            { 
                theText.text += lineOfText[letter];
                letter++;
            }
            
            yield return new WaitForSeconds(typeSpeed);
        }
        typeSpeed = 0.05f;
        isTyping = false;
        cancelTyping = false;
    }
    public void EnableTextBox()
    {
        size = 0;
        textBoxImage.transform.localScale = new Vector3(0, 0, 0);
        textBox.SetActive(true);
        isActive = true;
        player.canMove = false;
        StartCoroutine(TextScroll(textLines[currentLine]));

    }

    public void DisableTextBox()
    {
        isActive = false;
        StartCoroutine(Shrink());


    }

    public void ReloadScript(TextAsset theText)
    {
        if(theText != null)
        {
            textLines = new string[1];
            textLines = (theText.text.Split('\n'));
        }
        else
        {
            textLines = new string[1];
            textLines[0] = "This should not appear...";
        }
        currentLine = 0;
        endAtLine = textLines.Length - 1;
        

    }
}
