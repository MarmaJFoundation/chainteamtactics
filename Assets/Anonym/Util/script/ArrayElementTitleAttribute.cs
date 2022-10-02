using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayElementTitleAttribute : PropertyAttribute
{
    public string Varname;
    public System.Type Type = null;
    public ArrayElementTitleAttribute(string ElementTitleVar)
    {
        Varname = ElementTitleVar;
    }

    public ArrayElementTitleAttribute(System.Type type)
    {
        Type = type;

    }
}