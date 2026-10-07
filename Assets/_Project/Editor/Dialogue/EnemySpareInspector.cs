using System;
using Game2Week.Battle;
using Game2Week.Data;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Dialogue
{
    [CustomEditor(typeof(EnemyData))]
    public sealed class EnemySpareInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "spareRule");
            var rule = serializedObject.FindProperty("spareRule");
            EditorGUILayout.LabelField("자비 조건", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rule.FindPropertyRelative("enabled"), new GUIContent("조건 규칙 사용"));
            EditorGUILayout.PropertyField(rule.FindPropertyRelative("noFightTurns"), new GUIContent("공격하지 않은 턴"));
            EditorGUILayout.PropertyField(rule.FindPropertyRelative("halveOnFight"), new GUIContent("공격 시 절반 (해제하면 초기화)"));
            var conditions = rule.FindPropertyRelative("conditions");
            for (int i = 0; i < conditions.arraySize; i++)
            {
                var condition = conditions.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(condition, new GUIContent("조건 " + (i + 1)), true);
                if (GUILayout.Button("조건 삭제 " + (i + 1))) { conditions.DeleteArrayElementAtIndex(i); break; }
            }
            if (GUILayout.Button("조건 추가"))
            {
                var menu = new GenericMenu();
                Add(menu, "행동 순서", () => new ActSequenceCondition());
                Add(menu, "탄막 행동", () => new BulletActionCondition());
                Add(menu, "안 맞은 턴", () => new UnhurtTurnCondition());
                Add(menu, "대화 선택", () => new DialogueFlagCondition());
                Add(menu, "자비 보석", () => new SpareGemCondition());
                Add(menu, "적 체력 비율", () => new EnemyHealthCondition());
                menu.ShowAsContext();
            }
            serializedObject.ApplyModifiedProperties();
        }
        void Add(GenericMenu menu, string label, Func<ISpareCondition> create)
        {
            menu.AddItem(new GUIContent(label), false, () =>
            {
                serializedObject.Update(); var list = serializedObject.FindProperty("spareRule").FindPropertyRelative("conditions");
                int index = list.arraySize++; list.GetArrayElementAtIndex(index).managedReferenceValue = create(); serializedObject.ApplyModifiedProperties();
            });
        }
    }
}
