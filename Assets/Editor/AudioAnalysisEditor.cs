
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioAnalysis))]
public class AudioAnalysisEditor : UnityEditor.Editor
{
    class Styles
    {
        public static Color Background = new Color(0.1f, 0.1f, 0.1f, 1);
        public static Color Gray = new Color(0.3f, 0.3f, 0.3f, 1);
        public static Color Green1 = new Color(0, 0.3f, 0, 1);
        public static Color Green2 = new Color(0, 0.7f, 0, 1);
        public static Color Red = new Color(1, 0, 0, 1);
    }
    PropertyBinderEditor _propertyBinderEditor;

    private void OnEnable()
    {
        var finder = new PropertyFinder(serializedObject);

        _propertyBinderEditor
         = new PropertyBinderEditor(serializedObject.FindProperty("_propertyBinders"));

    }
    public override bool RequiresConstantRepaint()
    {
        // Keep updated while playing.
        return EditorApplication.isPlaying && targets.Length == 1;
    }

    public override void OnInspectorGUI()
    {
        AudioAnalysis data = (AudioAnalysis)target;
        base.OnInspectorGUI();
        // Background

        if (RequiresConstantRepaint())
        {
            EditorGUILayout.Space();
            var rect = GUILayoutUtility.GetRect(128, 128);
            DrawRect(0, 0, 1, 1, rect, Styles.Background);
            for (int i = 0; i < 8; i++)
            {
                Color c = Color.Lerp(Styles.Green2, Styles.Red, data.audioBand[i]);
                DrawRect(i / 8.0f, 1 - data.audioBand[i], (i + 1) / 8.0f, 1, rect, c);
                c = Color.Lerp(Styles.Green2, Styles.Red, 1 - data.audioBand[i]);
                DrawRect(i / 8.0f, 1 - data.audioBandBuffer[i], (i + 1) / 8.0f, 1 - data.audioBandBuffer[i] + .05f, rect, c);


            }
        }

        // Property binders
        if (targets.Length == 1) _propertyBinderEditor.ShowGUI();




    }
    Vector3[] _rectVertices = new Vector3[4];
    void DrawRect
         (float x1, float y1, float x2, float y2, Rect area, Color color)
    {
        x1 = area.xMin + area.width * Mathf.Clamp01(x1);
        x2 = area.xMin + area.width * Mathf.Clamp01(x2);
        y1 = area.yMin + area.height * Mathf.Clamp01(y1);
        y2 = area.yMin + area.height * Mathf.Clamp01(y2);

        _rectVertices[0] = new Vector2(x1, y1);
        _rectVertices[1] = new Vector2(x1, y2);
        _rectVertices[2] = new Vector2(x2, y2);
        _rectVertices[3] = new Vector2(x2, y1);

        Handles.DrawSolidRectangleWithOutline(_rectVertices, color, Color.clear);
    }
}

