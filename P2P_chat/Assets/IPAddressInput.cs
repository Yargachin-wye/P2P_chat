using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IPAddressInput : MonoBehaviour
{
    void Start()
    {
        TMP_InputField inputField = GetComponent<TMP_InputField>();
        inputField.keyboardType = TouchScreenKeyboardType.Default;
        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
    }
}

