using UnityEngine;

[System.Serializable]
public struct Transform4DScriptableObject
{
    public Vector4 localPosition;
    public Vector4 localScale;


	public void SetDefaultValues()
	{
        localPosition = Vector4.zero;
        localScale = Vector4.one;
	}
}