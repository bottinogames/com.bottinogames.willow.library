/* NOTE:
 *     This script should *NOT* be placed under an Editor folder in Unity, otherwise the struct would not be accessible outside 
 * of the editor, and would fail to compile, or uses of it would need to be wrapped in #if UNITY_EDITOR blocks.        
 */


using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif


// This script is implemented through a PropertyDrawer, so as to not interfere with custom editors.


/// <summary>
/// An easy to use Button that can be added to MonoBehaviours and ScriptableObjects without the need of editor scripting.<br/>
/// An implicit cast from string exists that allows for easy declaration when no additional options are needed, like so: 
/// <code>InspectorButton button = nameof(Method);</code>
/// </summary>

[Serializable]
public struct InspectorButton
{
    [System.Flags]
    public enum Options : byte
    {
        None = 0,

        /// <summary> Calls Undo.RecordObject on the inspectors target objects. </summary>
        RecordUndo = 1 << 0,

        /// <summary> Disables the button when Application.isPlayer != true. </summary>
        DisableInEditMode = 1 << 1,

        /// <summary> Disables the button when Application.isPlayer != true. </summary>
        DisableInPlayMode = 1 << 2,

        /// <summary> Suspends Asset Importing while the button functions are being called. 
        /// This is useful for if a button makes multiple file edits. </summary>
        SuspendImporting = 1 << 3,
    }


    private readonly string method;
    private readonly Options options;


    


    // ---- Construction ----

    public InspectorButton(string nameof, Options options = Options.RecordUndo)
    {
        method = nameof;
        this.options = options;
    }

    // Implicit cast of string to InspectorButton, allows for easy declarations of buttons like so: 
    // InspectorButton button = nameof(Method);
    public static implicit operator InspectorButton(string nameof) => new InspectorButton(nameof);





    // ---- Editor ----
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorButton))]
    public class InspectorButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            var target = property.serializedObject.targetObject;
            var data = (InspectorButton)fieldInfo.GetValue(target);

            bool previouslyEnabled = GUI.enabled;

            if (!Application.isPlaying && data.options.HasFlag(Options.DisableInEditMode))
                GUI.enabled &= false;
            if (Application.isPlaying && data.options.HasFlag(Options.DisableInPlayMode))
                GUI.enabled &= false;

            bool buttonPressed = GUI.Button(position, label);

            GUI.enabled = previouslyEnabled;

            if (buttonPressed)
            {
                bool suspendImporting = data.options.HasFlag(Options.SuspendImporting);

                try
                {
                    if (suspendImporting)
                        AssetDatabase.StartAssetEditing();

                    UnityEngine.Object obj = property.serializedObject.targetObject;
                    UnityEngine.Object[] objs = property.serializedObject.targetObjects;

                    bool isStatic = false;

                    MethodInfo method = obj.GetType().GetMethod(data.method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy); ;
                    if (method == null)
                        method = obj.GetType().GetMethod(data.method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);


                    if (method == null)
                    {
                        isStatic = true;

                        method = obj.GetType().GetMethod(data.method, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                        if (method == null)
                            method = obj.GetType().GetMethod(data.method, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                        if (method == null)
                        {
                            Debug.LogError($"No function of name \"{data.method}\" was found.");
                            return;
                        }
                    }

                    if (method.GetParameters().Length > 0)
                    {
                        Debug.LogError("Inspector Buttons will only work on functions with no input parameters.");
                        return;
                    }


                    bool recordUndo = data.options.HasFlag(Options.RecordUndo);

                    if (isStatic)
                    {
                        _ = method.Invoke(null, null);
                    }
                    else
                    {
                        for (int i = 0; i < objs.Length; i++)
                        {
                            if (recordUndo)
                                Undo.RecordObject(objs[i], label.text);

                            _ = method.Invoke(objs[i], null);

                            EditorUtility.SetDirty(objs[i]);
                        }
                    }

                    if(recordUndo)
                        Undo.FlushUndoRecordObjects();
                }
                catch(Exception e)
                {
                    Debug.LogException(new Exception("An Exception was thrown during an InspectorButton press.", e);
                }
                finally
                {
                    if (suspendImporting)
                        AssetDatabase.StopAssetEditing();
                }
            }
        }
    }
#endif
}
