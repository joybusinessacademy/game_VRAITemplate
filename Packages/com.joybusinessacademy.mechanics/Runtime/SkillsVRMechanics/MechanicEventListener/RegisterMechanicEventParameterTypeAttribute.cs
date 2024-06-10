using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum ,  AllowMultiple = true, Inherited = false)]
public class RegisterMechanicEventParameterTypeAttribute : Attribute
{
    public object eventValue;
    public Type parameterType;

    public RegisterMechanicEventParameterTypeAttribute(object targetEvent, Type eventParameterType)
    {
        eventValue = targetEvent;
        parameterType = eventParameterType;
    }
}