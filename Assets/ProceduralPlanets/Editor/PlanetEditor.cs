using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;

    private void OnEnable()
    {
        planet = (Planet)target;
    }

    public override void OnInspectorGUI()
    {
        using(var check = new EditorGUI.ChangeCheckScope())
        {
            bool res = false;

            base.OnInspectorGUI();
            res |= check.changed;

            res |= DrawSetting(planet.ShapeSetting);
            res |= DrawSetting(planet.ColorSetting);

            if (res == true)
            {
                planet.Generate();
            }
        }
    }

    bool DrawSetting(ISettingData _setting)
    {
        if (_setting == null) return false;

        _setting.Foldout = EditorGUILayout.InspectorTitlebar(_setting.Foldout, _setting);
        if(_setting.Foldout == false) return false;

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            Editor editor = CreateEditor(_setting);
            editor.OnInspectorGUI();

            return check.changed;
        }
    }
}
