using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Spoonx.EditorExtensions.GameObjectPositioning
{
    public class GameObjectPositioningWindow : EditorWindow
    {
        private float _spacing = 1.0f;

        private float _spacingGridRow = 1.0f;

        private float _spacingGridColumn = 1.0f;

        private int _columns = 3;

        private bool _showSimple = true;

        private bool _showGrid = false;

        [MenuItem("Tools/GameObject Positioning")]
        public static void Open()
        {
            GetWindow<GameObjectPositioningWindow>(false, "GameObject Positioning");
        }

        private void OnGUI()
        {
            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.BeginVertical("box");
            Draw();
            EditorGUILayout.EndVertical();

            obj.ApplyModifiedProperties();
        }

        private void Draw()
        {
            DrawSimple();
            DrawGrid();
        }

        private void DrawGrid()
        {
            _showGrid = EditorGUILayout.BeginFoldoutHeaderGroup(_showGrid, "Grid align");

            if (_showGrid)
            {
                EditorGUILayout.BeginHorizontal("box");
                _spacingGridRow = EditorGUILayout.FloatField("Spacing row:", _spacingGridRow);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                _spacingGridColumn = EditorGUILayout.FloatField("Spacing column:", _spacingGridColumn);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                _columns = EditorGUILayout.IntField("Columns per row:", _columns);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Align")) AlignGrid();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSimple()
        {
            _showSimple = EditorGUILayout.BeginFoldoutHeaderGroup(_showSimple, "Simple align");

            if (_showSimple)
            {
                EditorGUILayout.BeginHorizontal("box");
                _spacing = EditorGUILayout.FloatField("Spacing:", _spacing);

                if (GUILayout.Button("Align")) AlignSimple();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void AlignOnXAxis(Vector3 rootPosition, Transform[] transforms, float spacing)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].position = new Vector3()
                {
                    x = rootPosition.x + (i * spacing),
                    y = rootPosition.y,
                    z = rootPosition.z,
                };
            }
        }

        private void AlignSimple()
        {
            Vector3 rootPosition = Selection.transforms[0].position;

            Undo.RegisterCompleteObjectUndo(Selection.transforms, "Reposition GameObjects.");

            AlignOnXAxis(rootPosition, Selection.transforms, _spacing);
        }

        private void AlignGrid()
        {
            Vector3 rootPosition = Selection.transforms[0].position;
            int rows = (Selection.transforms.Length + _columns - 1) / _columns;

            Undo.RegisterCompleteObjectUndo(Selection.transforms, "Reposition GameObjects.");

            for (int i = 0; i < rows; i++)
            {
                Vector3 root = new Vector3()
                {
                    x = rootPosition.x,
                    y = rootPosition.y,
                    z = rootPosition.z - (i * _spacingGridRow),
                };

                Transform[] transforms = Selection.transforms.Skip(i * _columns).Take(_columns).ToArray();

                AlignOnXAxis(root, transforms, _spacingGridColumn);
            }
        }
    }
}
