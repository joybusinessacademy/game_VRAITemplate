using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public class RegisterMechanicEventAttribute : Attribute
{
    public object eventValue;
    public Type enumEventType;

    public RegisterMechanicEventAttribute() 
    {
    }

    public RegisterMechanicEventAttribute(object customEvent)
    {
        eventValue = customEvent;
    }
}
