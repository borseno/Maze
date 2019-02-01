using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputField : MonoBehaviour
{
    public TMP_InputField TMPInputField;
    public TMP_InputField UIInputField;
    
    public void ChangeName(string value)
    {
        Player.Name = value;
    }

}
