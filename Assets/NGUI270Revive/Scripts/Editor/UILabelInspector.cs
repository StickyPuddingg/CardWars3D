//----------------------------------------------
//     NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

//edited by sticky to be more efficient


using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
    UILabel mLabel;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    void RegisterUndo() { NGUIEditorTools.RegisterUndo("Label Change", mLabel); }

    /// <summary>
    /// Font selection callback.
    /// </summary>

    void OnSelectFont(MonoBehaviour obj)
    {
        if (mLabel != null)
        {
            NGUIEditorTools.RegisterUndo("Font Selection", mLabel);
            bool resize = (mLabel.font == null);
            mLabel.font = obj as UIFont;
            if (resize) mLabel.MakePixelPerfect();
        }
    }

    protected override bool DrawProperties()
    {
        mLabel = mWidget as UILabel;
        ComponentSelector.Draw<UIFont>(mLabel.font, OnSelectFont);

        if (mLabel.font != null)
        {
            // Force efficiency rules: No encoding, no shadow/outline effects
            if (mLabel.supportEncoding != false || mLabel.effectStyle != UILabel.Effect.None)
            {
                RegisterUndo();
                mLabel.supportEncoding = false;
                mLabel.effectStyle = UILabel.Effect.None;
            }

            GUI.skin.textArea.wordWrap = true;
            string text = string.IsNullOrEmpty(mLabel.text) ? "" : mLabel.text;
            text = EditorGUILayout.TextArea(mLabel.text, GUI.skin.textArea, GUILayout.Height(100f));
            if (!text.Equals(mLabel.text)) { RegisterUndo(); mLabel.text = text; }

            GUILayout.BeginHorizontal();
            int len = EditorGUILayout.IntField("Max Width", mLabel.lineWidth, GUILayout.Width(120f));
            GUILayout.Label("pixels");
            GUILayout.EndHorizontal();
            if (len != mLabel.lineWidth && len >= 0f) { RegisterUndo(); mLabel.lineWidth = len; }

            GUILayout.BeginHorizontal();
            len = EditorGUILayout.IntField("Max Height", mLabel.lineHeight, GUILayout.Width(120f));
            GUILayout.Label("pixels");
            GUILayout.EndHorizontal();
            if (len != mLabel.lineHeight && len >= 0f) { RegisterUndo(); mLabel.lineHeight = len; }

            int count = EditorGUILayout.IntField("Max Lines", mLabel.maxLineCount, GUILayout.Width(100f));
            if (count != mLabel.maxLineCount) { RegisterUndo(); mLabel.maxLineCount = count; }

            GUILayout.BeginHorizontal();
            bool shrinkToFit = EditorGUILayout.Toggle("Shrink to Fit", mLabel.shrinkToFit, GUILayout.Width(100f));
            GUILayout.Label("- adjust scale to fit");
            GUILayout.EndHorizontal();

            if (shrinkToFit != mLabel.shrinkToFit)
            {
                RegisterUndo();
                mLabel.shrinkToFit = shrinkToFit;
                if (!shrinkToFit) mLabel.MakePixelPerfect();
            }

            // Note: Encoding toggles, Symbol popups, and Effect options have been fully removed 
            // to guarantee they are never enabled and to clean up your Inspector UI.

            return true;
        }
        EditorGUILayout.Space();
        return false;
    }
}