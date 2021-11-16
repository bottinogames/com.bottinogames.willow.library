﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.EditorTools;
#endif


namespace Willow.IDLUI 
{
    public class IDLUIButton : MonoBehaviour
    {
        

        private void OnEnable() { Manager.AddActiveButton(this);}

        private void OnDisable() { Manager.RemoveActiveButton(this); }

        public Manager.Category category;

        public int priority = 0;

        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

        public UnityEvent onSelect = new UnityEvent();
        public UnityEvent onGainFocus = new UnityEvent();
        public UnityEvent onLoseFocus = new UnityEvent();

        [HideInInspector]
        public IDLUIButton up;
        [HideInInspector]
        public IDLUIButton down;
        [HideInInspector]
        public IDLUIButton left;
        [HideInInspector]
        public IDLUIButton right;


        //Built-In Behaviours
        public void SetActiveCategory(Manager.Category category) { Manager.activeCategory = category; }

        public void ForceFocus(IDLUIButton button) { Manager.FocusButton(button); }


        public void CloseApplication(float delay) { StartCoroutine(CloseAfterDelay(delay)); }
        private IEnumerator CloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Application.Quit();
        }


        //Extension Class
        [RequireComponent(typeof(IDLUIButton))]
        public class Extension : MonoBehaviour
        {
            IDLUIButton button;

            private void Awake() { button = GetComponent<IDLUIButton>(); }

            private void OnEnable() 
            {
                button = GetComponent<IDLUIButton>();
                button.onSelect.AddListener(OnSelect);
                button.onGainFocus.AddListener(OnGainFocus);
                button.onLoseFocus.AddListener(OnLoseFocus);
            }
            private void OnDisable() 
            {
                button.onSelect.RemoveListener(OnSelect);
                button.onGainFocus.RemoveListener(OnGainFocus);
                button.onLoseFocus.RemoveListener(OnLoseFocus);
            }

            protected virtual void OnSelect() { }
            protected virtual void OnGainFocus() { }
            protected virtual void OnLoseFocus() { }
        }

#if UNITY_EDITOR
        [ContextMenu("Set Bounds to Renderer Bounds")]
        private void SetSizeToBounds()
        {
            if(TryGetComponent<MeshFilter>(out MeshFilter filter))
            {
                bounds = filter.sharedMesh.bounds;
            }
        }
#endif

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(IDLUIButton))]
    public class IDLUIButtonInspector : Editor
    {

        BoxBoundsHandle handle;

        private void OnEnable()
        {
            if (handle == null)
                handle = new BoxBoundsHandle();
        }

        private void OnSceneGUI()
        {
            IDLUIButton button = (IDLUIButton)target;

            Matrix4x4 rotatedMatrix = button.transform.localToWorldMatrix;
            using (new Handles.DrawingScope(rotatedMatrix))
            {
                Undo.RecordObject(button, "BoxButton Bounds Handle");
                handle.center = button.bounds.center;
                handle.size = button.bounds.size;
                handle.SetColor(Color.yellow);
                handle.DrawHandle();
                button.bounds.center = handle.center;
                button.bounds.size = handle.size;
            }

            Handles.color = Color.green;
            if (button.up)
                Handles.DrawLine(button.transform.position, button.up.transform.position);
            Handles.color = Color.blue;
            if (button.down)
                Handles.DrawLine(button.transform.position, button.down.transform.position);
            Handles.color = Color.red;
            if (button.right)
                Handles.DrawLine(button.transform.position, button.right.transform.position);
            Handles.color = Color.yellow;
            if (button.left)
                Handles.DrawLine(button.transform.position, button.left.transform.position);

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            IDLUIButton button = (IDLUIButton)target;

            var up = serializedObject.FindProperty("up");
            var down = serializedObject.FindProperty("down");
            var left = serializedObject.FindProperty("left");
            var right = serializedObject.FindProperty("right");

            GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(up);
                if (GUILayout.Button("Reciprocate", GUILayout.Width(70)) && button.up)
                    button.up.down = button;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(down);
                if (GUILayout.Button("Reciprocate", GUILayout.Width(70)) && button.down)
                    button.down.up = button;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(left);
                if (GUILayout.Button("Reciprocate", GUILayout.Width(70)) && button.left)
                    button.left.right = button;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(right);
                if (GUILayout.Button("Reciprocate", GUILayout.Width(70)) && button.right)
                    button.right.left = button;
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }






    //Setup Tool


    [EditorTool("IDLUI Button Layout")]
    class IDLUIButtonConnector : EditorTool
    {

        
        const int UP = 0;
        const int DOWN = 1;
        const int LEFT = 2;
        const int RIGHT = 3;

        IDLUIButton focusedButton;
        int focusedDirection;

        private void OnEnable()
        {
            focusedButton = null;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            IDLUIButton[] buttons = GameObject.FindObjectsOfType<IDLUIButton>();

            Camera sceneCam = Camera.current;
            if (sceneCam) 
            {
                foreach (IDLUIButton button in buttons)
                {
                    if (button.up)
                    {
                        if(button.up.down == button)
                        {
                            Handles.color = Color.green;
                            Handles.DrawLine(button.transform.position, button.up.transform.position);
                        }
                        else
                        {
                            Handles.color = Color.green;
                            Handles.DrawDottedLine(button.transform.position, button.up.transform.position, 5f);
                        }
                    }

                    if (button.down)
                    {
                        if (button.down.up == button)
                        {
                            Handles.color = Color.green;
                            Handles.DrawLine(button.transform.position, button.down.transform.position);
                        }
                        else
                        {
                            Handles.color = Color.green;
                            Handles.DrawDottedLine(button.transform.position, button.down.transform.position, 5f);
                        }
                    }

                    if (button.left)
                    {
                        if (button.left.right == button)
                        {
                            Handles.color = Color.red;
                            Handles.DrawLine(button.transform.position, button.left.transform.position);
                        }
                        else
                        {
                            Handles.color = Color.red;
                            Handles.DrawDottedLine(button.transform.position, button.left.transform.position, 5f);
                        }
                    }

                    if (button.right)
                    {
                        if (button.right.left == button)
                        {
                            Handles.color = Color.red;
                            Handles.DrawLine(button.transform.position, button.right.transform.position);
                        }
                        else
                        {
                            Handles.color = Color.red;
                            Handles.DrawDottedLine(button.transform.position, button.right.transform.position, 5f);
                        }
                    }
                }
            }


            Handles.BeginGUI();

            if(focusedButton != null)
            {
                if (UnityEngine.GUI.Button(new Rect(5, 5, 80, 20), "Cancel"))
                {
                    focusedButton = null;
                }
            }

            int id = 24910;
            if (sceneCam)
            {
                foreach (IDLUIButton button in buttons)
                {
                    if (button.up)
                    {

                    }

                    Vector3 screenPos = sceneCam.WorldToScreenPoint(button.transform.position);
                    Rect boxRect = new Rect(screenPos.x - 30, (Screen.height - screenPos.y) - 40, 60, 80);
                    GUILayout.BeginArea(boxRect);
                    SceneViewGuiArea(button);
                    if (UnityEngine.GUI.Button(new Rect(2f, 2f, 10f, 10f), GUIContent.none))
                        Selection.SetActiveObjectWithContext(button.gameObject, null);
                    GUILayout.EndArea();
                    id++;
                }
            }

            Handles.EndGUI();
        }

        public void SceneViewGuiArea(IDLUIButton button)
        {
            if (focusedButton == null)
            {
                GUILayout.BeginVertical("HelpBox");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("↑", GUILayout.Width(25)))
                        {
                            focusedButton = button;
                            focusedDirection = UP;
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("←", GUILayout.Width(25)))
                        {
                            focusedButton = button;
                            focusedDirection = LEFT;
                        }
                        if (GUILayout.Button("→", GUILayout.Width(25)))
                        {
                            focusedButton = button;
                            focusedDirection = RIGHT;
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("↓", GUILayout.Width(25)))
                        {
                            focusedButton = button;
                            focusedDirection = DOWN;
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginHorizontal("HelpBox");
                {
                    string recip = focusedDirection == UP || focusedDirection == DOWN ? "⇅" : "⇄";
                    string direct = focusedDirection == UP ? "↑" :
                        (focusedDirection == RIGHT ? "→" :
                        (focusedDirection == LEFT ? "←" :
                        "↓"));


                    int oldsize = UnityEngine.GUI.skin.button.fontSize;
                    UnityEngine.GUI.skin.button.fontSize = 16;
                    if (GUILayout.Button(recip, GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        switch (focusedDirection)
                        {
                            case UP:
                                focusedButton.up = button;
                                button.down = focusedButton;
                                focusedButton = null;
                                break;
                            case DOWN:
                                focusedButton.down = button;
                                button.up = focusedButton;
                                focusedButton = null;
                                break;
                            case LEFT:
                                focusedButton.left = button;
                                button.right = focusedButton;
                                focusedButton = null;
                                break;
                            case RIGHT:
                                focusedButton.right = button;
                                button.left = focusedButton;
                                focusedButton = null;
                                break;
                            default:
                                break;
                        }
                    }
                    if (GUILayout.Button(direct, GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        switch (focusedDirection)
                        {
                            case UP:
                                focusedButton.up = button;
                                focusedButton = null;
                                break;
                            case DOWN:
                                focusedButton.down = button;
                                focusedButton = null;
                                break;
                            case LEFT:
                                focusedButton.left = button;
                                focusedButton = null;
                                break;
                            case RIGHT:
                                focusedButton.right = button;
                                focusedButton = null;
                                break;
                            default:
                                break;
                        }
                    }
                    UnityEngine.GUI.skin.button.fontSize = oldsize;
                }
                GUILayout.EndHorizontal();
            }
        }
    }
#endif
}