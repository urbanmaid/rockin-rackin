using UnityEngine;

public sealed class TemporaryMaterialColorChanger : MonoBehaviour
{
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Color originalColor = Color.white;
    [SerializeField] private Color changedColor = Color.white;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private bool isChanged;

    private void OnDisable()
    {
        RestoreOriginalColor();
    }

    private void OnDestroy()
    {
        RestoreOriginalColor();
    }

    public void SetChanged(bool changed)
    {
        if (isChanged == changed)
        {
            return;
        }

        if (changed)
        {
            ApplyChangedColor();
        }
        else
        {
            RestoreOriginalColor();
        }
    }

    public void ApplyChangedColor()
    {
        SetMaterialColor(changedColor);
        isChanged = true;
    }

    public void RestoreOriginalColor()
    {
        SetMaterialColor(originalColor);
        isChanged = false;
    }

    private void SetMaterialColor(Color color)
    {
        if (targetMaterial == null)
        {
            return;
        }

        if (targetMaterial.HasProperty(BaseColorId))
        {
            targetMaterial.SetColor(BaseColorId, color);
        }
        else if (targetMaterial.HasProperty(ColorId))
        {
            targetMaterial.SetColor(ColorId, color);
        }
    }
}
