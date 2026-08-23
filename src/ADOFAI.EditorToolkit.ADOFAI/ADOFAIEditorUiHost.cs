using System;
using System.Reflection;
using UnityEngine;

namespace ADOFAI.EditorToolkit.Game
{
    /// <summary>
    /// Low-level host for mounting reusable mod UI into ADOFAI's stock level editor canvas.
    /// This class intentionally owns only ADOFAI-specific hierarchy/layout knowledge;
    /// higher-level workspace and document semantics belong in consumer mods.
    /// </summary>
    public static class ADOFAIEditorUiHost
    {
        private const float FallbackLeft = 340f;
        private const float FallbackRight = 340f;
        private const float FallbackTop = 55f;
        private const float FallbackBottom = 120f;

        /// <summary>The stock <c>/levelEditorScene</c> RectTransform.</summary>
        public static RectTransform Root
        {
            get
            {
                RectTransform root = RequireEditor().transform as RectTransform;
                if (root == null)
                    throw new InvalidOperationException("ADOFAI stock editor root is not a RectTransform.");
                return root;
            }
        }

        /// <summary>
        /// Measures the stock editor chrome around the central chart viewport.
        /// Values are expressed in the stock editor canvas coordinate system (1600x900 in ADOFAI 3.3.1).
        /// </summary>
        public static EditorUiInsets MeasureViewportInsets()
        {
            scnEditor editor = RequireEditor();

            float left = WidthOf(GetMemberObject(editor, "settingsPanel"), FallbackLeft);
            float right = WidthOf(GetMemberObject(editor, "levelEventsPanel"), FallbackRight);
            float bottom = HeightOf(GetMemberObject(editor, "levelStringPanel"), FallbackBottom);
            float top = TopInsetOf(GetMemberObject(editor, "settingsPanel"), FallbackTop);

            return new EditorUiInsets(left, right, top, bottom);
        }

        /// <summary>
        /// Gets or creates a full-stretch host under the stock editor canvas.
        /// The host is inserted immediately before the stock shortcuts panel when possible,
        /// keeping shortcuts/popups/notifications above consumer UI.
        /// </summary>
        public static RectTransform GetOrCreateOverlayRoot(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A host name is required.", nameof(name));

            scnEditor editor = RequireEditor();
            RectTransform root = Root;
            Transform existing = FindDirectChild(root, name);
            RectTransform result;

            if (existing != null)
            {
                result = existing as RectTransform;
                if (result == null)
                    throw new InvalidOperationException("Existing editor UI host '" + name + "' is not a RectTransform.");
            }
            else
            {
                var go = new GameObject(name, typeof(RectTransform));
                result = (RectTransform)go.transform;
                result.SetParent(root, false);
            }

            Stretch(result, Vector2.zero, Vector2.zero);
            PutBelowStockOverlays(editor, result);
            return result;
        }

        /// <summary>
        /// Gets or creates a child RectTransform that occupies only the central chart viewport,
        /// excluding the stock left/right inspectors, top chrome and bottom event controls.
        /// </summary>
        public static RectTransform GetOrCreateViewportRoot(string name)
        {
            RectTransform overlay = GetOrCreateOverlayRoot(name);
            EditorUiInsets insets = MeasureViewportInsets();
            Stretch(
                overlay,
                new Vector2(insets.Left, insets.Bottom),
                new Vector2(-insets.Right, -insets.Top));
            return overlay;
        }

        /// <summary>
        /// Resolves a public field/property on <see cref="scnEditor"/> to its GameObject.
        /// This is useful for cloning stock buttons, text objects, and other controls without
        /// making the toolkit depend on a particular Unity UI/TMP control type.
        /// </summary>
        public static GameObject GetStockObject(string publicMemberName)
        {
            if (string.IsNullOrWhiteSpace(publicMemberName))
                throw new ArgumentException("A public member name is required.", nameof(publicMemberName));

            object value = GetMemberObject(RequireEditor(), publicMemberName);
            if (value == null) return null;

            GameObject gameObject = value as GameObject;
            if (gameObject != null) return gameObject;

            Component component = value as Component;
            return component != null ? component.gameObject : null;
        }

        /// <summary>Clones a stock editor object beneath a consumer-owned parent.</summary>
        public static GameObject CloneStockObject(string publicMemberName, Transform parent, string cloneName = null)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            GameObject template = GetStockObject(publicMemberName);
            if (template == null)
                throw new InvalidOperationException("Could not resolve stock editor UI member '" + publicMemberName + "'.");

            GameObject clone = UnityEngine.Object.Instantiate(template, parent, false);
            if (!string.IsNullOrWhiteSpace(cloneName)) clone.name = cloneName;
            clone.SetActive(true);
            return clone;
        }

        private static scnEditor RequireEditor()
        {
            scnEditor editor = ADOBase.editor;
            if (editor == null) throw new InvalidOperationException("ADOFAI stock editor is not active.");
            return editor;
        }

        private static void PutBelowStockOverlays(scnEditor editor, RectTransform host)
        {
            object shortcuts = GetMemberObject(editor, "shortcutsPanel");
            GameObject shortcutsObject = ToGameObject(shortcuts);
            if (shortcutsObject != null && shortcutsObject.transform.parent == host.parent)
            {
                host.SetSiblingIndex(shortcutsObject.transform.GetSiblingIndex());
                return;
            }

            // ADOFAI 3.3.1: settings, bottom, file actions and right inspector are the first four children.
            // Index 4 therefore keeps the host above ordinary editor chrome but below popup-style overlays.
            host.SetSiblingIndex(Math.Min(4, host.parent.childCount - 1));
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && string.Equals(child.name, name, StringComparison.Ordinal)) return child;
            }
            return null;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private static float WidthOf(object value, float fallback)
        {
            RectTransform rect = RectOf(value);
            return rect != null && rect.rect.width > 0f ? rect.rect.width : fallback;
        }

        private static float HeightOf(object value, float fallback)
        {
            RectTransform rect = RectOf(value);
            return rect != null && rect.rect.height > 0f ? rect.rect.height : fallback;
        }

        private static float TopInsetOf(object value, float fallback)
        {
            RectTransform rect = RectOf(value);
            if (rect == null) return fallback;

            // Stock side inspectors are top-anchored with anchoredPosition.y == -55 in ADOFAI 3.3.1.
            float inset = -rect.anchoredPosition.y;
            return inset >= 0f && inset < Root.rect.height ? inset : fallback;
        }

        private static RectTransform RectOf(object value)
        {
            GameObject go = ToGameObject(value);
            return go != null ? go.transform as RectTransform : null;
        }

        private static GameObject ToGameObject(object value)
        {
            GameObject go = value as GameObject;
            if (go != null) return go;
            Component component = value as Component;
            return component != null ? component.gameObject : null;
        }

        private static object GetMemberObject(scnEditor editor, string name)
        {
            Type type = editor.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            FieldInfo field = type.GetField(name, flags);
            if (field != null) return field.GetValue(editor);

            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null && property.CanRead) return property.GetValue(editor, null);

            return null;
        }
    }

    /// <summary>Stock editor chrome thickness around the central chart viewport.</summary>
    public struct EditorUiInsets
    {
        public EditorUiInsets(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Top { get; private set; }
        public float Bottom { get; private set; }

        public override string ToString()
        {
            return "L" + Left.ToString("0.#")
                + " R" + Right.ToString("0.#")
                + " T" + Top.ToString("0.#")
                + " B" + Bottom.ToString("0.#");
        }
    }
}
