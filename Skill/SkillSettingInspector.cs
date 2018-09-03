using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;


[CustomEditor(typeof(SkillSetting), true)]
public class SkillSettingInspector : Editor
{    
    SkeletonAnimation skeletonAnimation { get { return skillSetting.skeletonAnimation; } }

    SkillSetting skillSetting;

    void OnEnable()
    {
        if (!target)
        {
            Debug.LogWarning("Target is null");
            return;
        }   

        skillSetting = target as SkillSetting;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillID"));
        
        //스파인 지정된 경우만 프로퍼티 노출
        if (skeletonAnimation)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skeletonAnimation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skillRange"));
        }

        if (serializedObject.targetObject is SkillSettingDive)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("acc"));
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curveStartX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curveStartY"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("destPosOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backOriginalPos"));

            if (skeletonAnimation)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationFoward"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationBack"));                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationUp"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationDown"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationFinish"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("minDiveTime"));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("finishAnimStartOffset"));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isNeedExpandPath"));
            }

        }


        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillEventContainer"), true, GUILayout.MinHeight(1));
        
        serializedObject.ApplyModifiedProperties();
    }

}