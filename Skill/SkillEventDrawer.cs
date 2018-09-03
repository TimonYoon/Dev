using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SkillEventContainer))]
public class SkillEventContainerDrawer : PropertyDrawer
{
    SkillSetting skillSetting;

    ReorderableList list;
        
    List<string> eventStringList = new List<string>();

    Spine.Unity.SkeletonAnimation _skeletonAnimation = null;
    Spine.Unity.SkeletonAnimation skeletonAnimation
    {
        get
        {
            return _skeletonAnimation;
        }
        set
        {
            if(value == null)
            {
                eventStringList.Clear();
                eventStringList.Add("OnStart");
                eventStringList.Add("OnEnd");
            }

            if (_skeletonAnimation == value)
                return;

            eventStringList.Clear();

            _skeletonAnimation = value;

            eventStringList.Add("OnStart");

            if(skillSetting is SkillSettingDive)
            {
                eventStringList.Add("OnDive");
            }

            //if (value == null)
            //    return;

            if (value != null)
                for (int i = 0; i < value.Skeleton.Data.Events.Count; i++)
                {
                    string s = value.Skeleton.Data.Events.Items[i].Name;
                    eventStringList.Add(s);
                }

            eventStringList.Add("OnEnd");
        }
    }


    ReorderableList GetList(SerializedProperty property)
    {
        if (list == null)
        {
            list = new ReorderableList(property.serializedObject, property, true, true, true, true);
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Events");
            };
                        
            list.drawElementCallback = DrawElement;
        }

        return list;
    }

    float elementHeight = EditorGUIUtility.singleLineHeight;
    
    void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        //위에 약간의 여백
        //rect.x += 4f;
        rect.y += 4f;
        
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
        elementHeight = EditorGUIUtility.singleLineHeight + 4f;

        //float defaultX = rect.x + EditorGUIUtility.currentViewWidth * 0.5f - 100f;
        //float width = EditorGUIUtility.currentViewWidth * 0.5f + 38;
        float width = EditorGUIUtility.currentViewWidth * 0.48f;
        
        //이벤트 타입
        SkillEvent.SkillEventType eventType = (SkillEvent.SkillEventType)element.FindPropertyRelative("eventType").enumValueIndex;

        //이벤트 타입별 색상 다르게 표현
        switch (eventType)
        {
            case SkillEvent.SkillEventType.PlaySound:
                GUI.backgroundColor = Color.green * 0.7f;
                break;
            case SkillEvent.SkillEventType.MeleeHit:
                GUI.backgroundColor = Color.red * 1f;
                break;
            case SkillEvent.SkillEventType.ShowParticle:
                GUI.backgroundColor = Color.yellow * 1f;
                break;
            case SkillEvent.SkillEventType.FireProjectile:
                GUI.backgroundColor = Color.magenta * 0.7f;
                break;
        }


        //이벤트 이륨 표시
        //EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Event Name");
        SerializedProperty propEventName = element.FindPropertyRelative("eventName");
        
        //이벤트 이름을 팝업 형식으로 표현
        int selectedEventIndex = 0;
        if (!string.IsNullOrEmpty(propEventName.stringValue))
            selectedEventIndex = eventStringList.FindIndex(x => x== propEventName.stringValue);

        selectedEventIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, width - rect.x, EditorGUIUtility.singleLineHeight), selectedEventIndex, eventStringList.ToArray());

        //이벤트 이름 저장
        propEventName.stringValue = eventStringList[selectedEventIndex];
        
        //이벤트 타입 팝업 표시
        //EditorGUI.LabelField(new Rect(rect.x, rect.y + elementHeight, 100, EditorGUIUtility.singleLineHeight), "Event Type");
        EditorGUI.PropertyField(new Rect(width, rect.y, width - 12f, elementHeight), element.FindPropertyRelative("eventType"), GUIContent.none);

        //이벤트 타입별 추가로 필요한 것들 표시
        Rect rectLabel = new Rect(rect.x, rect.y + elementHeight * 1f, width - rect.x - 10, EditorGUIUtility.singleLineHeight);
        Rect rectLabel2 = new Rect(rectLabel) { y = rectLabel.y + EditorGUIUtility.singleLineHeight + 4f };
        Rect rectPropertyField = new Rect(width + 4f, rect.y + elementHeight * 1f, width - 16f, EditorGUIUtility.singleLineHeight);
        Rect rectPropertyField2 = new Rect(rectPropertyField) { y = rectPropertyField.y + EditorGUIUtility.singleLineHeight + 4f };
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleRight;
        //labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.gray * 1.5f;

        switch (eventType)
        {
            case SkillEvent.SkillEventType.PlaySound:
                EditorGUI.LabelField(rectLabel, "Audio Source 프리팹", labelStyle);
                EditorGUI.PropertyField(rectPropertyField, element.FindPropertyRelative("audioSource"), GUIContent.none);
                break;

            case SkillEvent.SkillEventType.MeleeHit:
                EditorGUI.LabelField(rectLabel, "피격 이펙트 프리팹", labelStyle);
                EditorGUI.PropertyField(rectPropertyField, element.FindPropertyRelative("hitEffect"), GUIContent.none);
                EditorGUI.LabelField(rectLabel2, "피격 이펙트 대상 따라다님?", labelStyle);
                EditorGUI.PropertyField(rectPropertyField2, element.FindPropertyRelative("attachToTarget"), GUIContent.none);
                break;

            case SkillEvent.SkillEventType.ShowParticle:
                EditorGUI.LabelField(rectLabel, "Particle 프리팹", labelStyle);
                EditorGUI.PropertyField(rectPropertyField, element.FindPropertyRelative("particle"), GUIContent.none);
                EditorGUI.LabelField(rectLabel2, "이펙트 주인 따라다님?", labelStyle);
                EditorGUI.PropertyField(rectPropertyField2, element.FindPropertyRelative("attachToTarget"), GUIContent.none);
                break;

            case SkillEvent.SkillEventType.FireProjectile:
                EditorGUI.LabelField(rectLabel, "발사체 프리팹", labelStyle);
                EditorGUI.PropertyField(rectPropertyField, element.FindPropertyRelative("projectile"), GUIContent.none);

                EditorGUI.LabelField(rectLabel2, "발사체 생성위치", labelStyle);
                EditorGUI.PropertyField(rectPropertyField2, element.FindPropertyRelative("projectilePivot"), GUIContent.none);
                break;

            case SkillEvent.SkillEventType.ExecuteSkill:
                EditorGUI.LabelField(rectLabel, "발동할 스킬 ID", labelStyle);
                EditorGUI.PropertyField(rectPropertyField, element.FindPropertyRelative("executeSkillID"), GUIContent.none);

                EditorGUI.LabelField(rectLabel2, "발동 가중치", labelStyle);
                EditorGUI.PropertyField(rectPropertyField2, element.FindPropertyRelative("executeWeight"), GUIContent.none);
                break;

            case SkillEvent.SkillEventType.Summon:
                EditorGUI.LabelField(rectLabel, "소환물", labelStyle);
                EditorGUI.PropertyField(rectPropertyField, element.FindPropertyRelative("summonObject"), GUIContent.none);               

                break;
        }

        GUI.backgroundColor = Color.white;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent lable)
    {
        if (!skillSetting || !skillSetting.skeletonAnimation)
        {
            return 0f;
        }
        return GetList(property.FindPropertyRelative("skillEventList")).GetHeight();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!skillSetting)
        {
            skillSetting = property.serializedObject.targetObject as SkillSetting;
        }
        
        skeletonAnimation = skillSetting.skeletonAnimation;

        if (!skeletonAnimation)
        {
            skillSetting.skeletonAnimation = skillSetting.gameObject.GetComponentInChildren<Spine.Unity.SkeletonAnimation>();

            skeletonAnimation = skillSetting.skeletonAnimation;
        }

        if (!skeletonAnimation)
        {
            skillSetting.skeletonAnimation = skillSetting.gameObject.GetComponentInParent<Spine.Unity.SkeletonAnimation>();

            skeletonAnimation = skillSetting.skeletonAnimation;
        }

        SerializedProperty listProperty = property.FindPropertyRelative("skillEventList");

        /*ReorderableList*/ list = GetList(listProperty);
        float height = 0f;
        for (int i = 0; i < listProperty.arraySize; i++)
        {
            height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(i))) + 1.5f;
        }

        //애니메이션 지정 안되있으면 이벤트 목록 그리지 않음
        //if (skeletonAnimation)
        {
            list.elementHeight = EditorGUIUtility.singleLineHeight * 3 + 20f;
            //list.DoList(position);
            list.DoLayoutList();
        }
        
    }
}