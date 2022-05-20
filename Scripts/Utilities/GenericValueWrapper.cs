

using System;
using UnityEngine;

public enum ValueTypes
{
    INT,
    FLOAT,
    BOOL,
    VECTOR3,
    VECTOR2,
    CURVE,
    TEX2D
}

[System.Serializable]
public class GenericValueWrapper
{
    public ValueTypes valueType;
    public object Value;

    public int intVal;
    public float floatVal;
    public bool boolVal;
    public Vector3 vec3Val;
    public Vector2 vec2Val;
    public AnimationCurve curveVal;
    public Texture2D textureVal;

    public GenericValueWrapper()
    {
        valueType = ValueTypes.INT;
        Value = null;
    }

    public object GetValue()
    {
        switch (valueType)
        {
            case ValueTypes.INT:
                return intVal;
            case ValueTypes.FLOAT:
                return floatVal;
            case ValueTypes.BOOL:
                return boolVal;
            case ValueTypes.VECTOR3:
                return vec3Val;
            case ValueTypes.VECTOR2:
                return vec2Val;
            case ValueTypes.CURVE:
                return curveVal;
            case ValueTypes.TEX2D:
                return textureVal;
            default:
                return null;
        }
    }

    public bool IsObjectSaved(Type type)
    {
        if (type == typeof(int))
        {
            valueType = ValueTypes.INT;
            Value = intVal;
            return true;
        }
        if (type == typeof(bool))
        {
            valueType = ValueTypes.BOOL;
            Value = boolVal;
            return true;
        }
        if (type == typeof(float))
        {
            valueType = ValueTypes.FLOAT;
            Value = floatVal;
            return true;
        }
        if (type == typeof(Vector3))
        {
            valueType = ValueTypes.VECTOR3;
            Value = vec3Val;
            return true;
        }
        if (type == typeof(Vector2))
        {
            valueType = ValueTypes.VECTOR2;
            Value = vec2Val;
            return true;
        }
        if (type == typeof(AnimationCurve) && curveVal != null)
        {
            valueType = ValueTypes.CURVE;
            Value = curveVal;
            return true;
        }
        if (type == typeof(Texture2D) && textureVal != null)
        {
            valueType = ValueTypes.TEX2D;
            Value = textureVal;
            return true;
        }

        return false;
    }
}