using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spoonx.Editor
{
    [InitializeOnLoad]
    public static class BetterHierarchy
    {
        private const string ToggleStyleName = "OL Toggle";
        private const string MixedToggleStyleName = "OL ToggleMixed";

        private static bool _includeNotImportant;
        private const string IncludeNotImportantPrefsKey = "{E0EF3D35-59F0-4531-8040-7341E3093C84}";

        // ===============================================================================================

        // Allows to override icon used to draw component (using EditorGUIUtility.IconContent)
        private static readonly Dictionary<Type, string> IconOverrides = new Dictionary<Type, string>()
        {
        };

        // Highlighted components
        private static readonly HashSet<Type> ImportantList = new HashSet<Type>
        {
            typeof(Camera),
            typeof(Rigidbody2D),
            typeof(Rigidbody),
            typeof(TMPro.TMP_Text),
            typeof(Collider),
            typeof(Collider2D),
            typeof(Renderer),
            typeof(CanvasRenderer)
        };

        // Not draw components
        private static readonly HashSet<Type> Blacklist = new HashSet<Type>
        {
            typeof(Transform),
            typeof(RectTransform)
        };


        // ===============================================================================================

        static BetterHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI = DrawItem;
            _includeNotImportant = EditorPrefs.GetBool(IncludeNotImportantPrefsKey);
        }

        [MenuItem("Tools/BetterHierarchy/Toggle Non-Important")]
        public static void ToggleNonImportant()
        {
            _includeNotImportant = !_includeNotImportant;
            EditorPrefs.SetBool(IncludeNotImportantPrefsKey, _includeNotImportant);
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void DrawItem(int instanceId, Rect rect)
        {
            // Gets object for given item
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

            if (go == null) return;

            bool isHeader = go.name.StartsWith("---");

            bool shouldHaveActivityToggle = !isHeader || go.transform.childCount > 0;

            DrawComponentIcons(rect, go, out int numberOfIconsDraw);

            if (shouldHaveActivityToggle)
            {
                DrawActivityToggle(rect, go);
            }

            if (isHeader)
            {
                DrawHeader(rect, go, shouldHaveActivityToggle, numberOfIconsDraw);
            }
        }

        private static void DrawHeader(Rect rect, Object go, bool cutLeft, int componentDrawCut)
        {
            // Creating highlight rect and style
            Rect highlightRect = new Rect(rect);
            highlightRect.width -= highlightRect.height;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
            };
            labelStyle.fontSize -= 1;
            highlightRect.height -= 1;
            highlightRect.y += 1;

            // Drawing background
            string colorHtml = EditorGUIUtility.isProSkin ? "#2D2D2D" : "#AAAAAA";
            ColorUtility.TryParseHtmlString(colorHtml, out Color headerColor);

            Rect headerRect = new Rect(highlightRect);
            headerRect.y -= 1;
            headerRect.xMin -= 28;
            headerRect.xMax += 28;

            Rect fullRect = new Rect(headerRect);

            if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab && componentDrawCut == 0)
            {
                headerRect.xMax -= 10;
            }

            if (componentDrawCut > 0)
            {
                headerRect.xMax -= 16;
                headerRect.xMax -= componentDrawCut * 16;
            }

            if (cutLeft)
            {
                headerRect.xMin += 28;
            }

            EditorGUI.DrawRect(headerRect, headerColor);

            // Offsetting text
            highlightRect.height -= 2;

            // Drawing label
            EditorGUI.LabelField(highlightRect, go.name.Replace("---", "").ToUpperInvariant(), labelStyle);
        }

        private static void DrawComponentIcons(Rect rect, GameObject go, out int numberOfIconsDrawn)
        {
            Dictionary<Texture, int> usedIcons = new Dictionary<Texture, int>();
            List<(Texture texture, bool important)> iconsToDraw = new List<(Texture icon, bool important)>();

            foreach (Component component in go.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                Type type = component.GetType();

                if (Blacklist.Contains(type))
                    continue;

                Texture texture = GetIconFor(component, type);
                bool important = CheckTypeRecursive(type, ImportantList);

                if (!_includeNotImportant && !important)
                    continue;

                if (usedIcons.TryGetValue(texture, out int index))
                {
                    (Texture texture, bool important) icon = iconsToDraw[index];
                    icon.important |= important;
                    iconsToDraw[index] = icon;
                }
                else
                {
                    iconsToDraw.Add((texture, important));
                    usedIcons.Add(texture, iconsToDraw.Count - 1);
                }
            }

            for (int i = 0; i < iconsToDraw.Count; i++)
            {
                (Texture texture, bool important) = iconsToDraw[i];
                Color tint = important
                    ? new Color(1, 1, 1, 1)
                    : new Color(0.8f, 0.8f, 0.8f, 0.25f);
                GUI.DrawTexture(GetRightRectWithOffset(rect, i), texture, ScaleMode.ScaleToFit, true, 0, tint, 0, 0);
            }

            numberOfIconsDrawn = iconsToDraw.Count;
        }

        private static bool CheckTypeRecursive(Type t, HashSet<Type> set)
        {
            if (set.Contains(t))
                return true;

            return t.BaseType != null && CheckTypeRecursive(t.BaseType, set);
        }

        private static Texture GetIconFor(Object c, Type type)
        {
            return IconOverrides.TryGetValue(type, out string icon)
                ? EditorGUIUtility.IconContent(icon).image
                : EditorGUIUtility.ObjectContent(c, type).image;
        }

        private static void DrawActivityToggle(Rect rect, GameObject go)
        {
            // Get's style of toggle
            bool active = go.activeInHierarchy;

            GUIStyle toggleStyle = active
                ? ToggleStyleName
                : MixedToggleStyleName;

            // Sets rect for toggle
            Rect toggleRect = new Rect(rect);
            toggleRect.width = toggleRect.height;
            toggleRect.x -= 28;

            // Creates toggle
            bool state = GUI.Toggle(toggleRect, go.activeSelf, GUIContent.none, toggleStyle);

            // Sets game's active state to result of toggle
            if (state == go.activeSelf) return;

            Undo.RecordObject(go, $"{(state ? "Enabled" : "Disabled")}");
            go.SetActive(state);
            Undo.FlushUndoRecordObjects();
        }

        private static Rect GetRightRectWithOffset(Rect rect, int offset)
        {
            Rect newRect = new Rect(rect);
            newRect.width = newRect.height;
            newRect.x = rect.x + rect.width - (rect.height * offset) - 16;
            return newRect;
        }
    }
}
