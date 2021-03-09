using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pandora.Engine.Animations;
using System.IO;

[CustomEditor(typeof(AnimationBezier))]
public class AnimationBezierEditor : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationBezier animationBezier = (AnimationBezier)target;

        if (GUILayout.Button("Serialize animation"))
        {
            Debug.Log("Generating animation...");
            var steps = animationBezier.GetSteps(130);
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