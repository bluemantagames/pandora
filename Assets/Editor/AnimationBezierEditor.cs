using System;
using UnityEngine;
using UnityEditor;
using Pandora.Engine.Animations;
using System.IO;
using Pandora;

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
            var speed = animationBezier.GetUnitSpeed(targetGameObject);

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
        var animationsPath = SerializedAnimationsSingleton.Instance.GetAnimationsDirectory();
        var animationFile = SerializedAnimationsSingleton.Instance.GenerateAnimationFileName(animationName);
        var animationPath = $"{animationsPath}/{animationFile}";

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