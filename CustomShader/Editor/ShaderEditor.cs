using UnityEditor;

public class ShaderEditor : ShaderGUI
{
    MaterialProperty testFloat = null;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        testFloat = FindProperty("_TestFloat", properties);
        materialEditor.FloatProperty(testFloat,"Å×½ºÆ®");
        
        base.OnGUI(materialEditor, properties);
    }
}
