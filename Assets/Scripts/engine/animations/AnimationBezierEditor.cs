using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pandora.Engine.Animations;
using Pandora.Combat;
using System.IO;

[CustomEditor(typeof(AnimationBezier))]
public class AnimationBezierEditor : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationBezier animationBezier = (AnimationBezier)target;
        var targetGameObject = animationBezier.gameObject;

        if (GUILayout.Button("Serialize animation"))
        {
            var projectileBehaviour = targetGameObject.GetComponent<ProjectileBehaviour>();
            var speed = projectileBehaviour != null ? projectileBehaviour.Speed : 130;

            Debug.Log("Generating animation...");
            var steps = animationBezier.GetSteps(speed);
            var collection = new AnimationStepCollection { steps = steps };

            Debug.Log("Serializing animation to JSON...");
            var serializedSteps = JsonUtility.ToJson(collection);

            CreateAnimationFile(serializedSteps, animationBezier.AnimationName);
        }
    }

    private void CreateAnimationFile(string animationJson, string animationName)
    {
        var projectPath = Application.dataPath;
        var animationPath = $"{projectPath}/GeneratedAnimations/{animationName}.json";

        Debug.Log($"Generating animation file: {animationPath}");

        try
        {
            using (StreamWriter sw = File.CreateText(animationPath))
            {
                sw.WriteLine(animationJson);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while writing the animation file: {e.Message}");
        }
    }
}