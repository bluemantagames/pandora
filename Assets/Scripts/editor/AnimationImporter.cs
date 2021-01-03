using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Linq;
using System.IO;

namespace Pandora.Editor
{
    public class AnimationImporter
    {
        [MenuItem("Assets/Create SpriteAtlas and AnimationClips for selected Sprites")]
        public static void CreateAtlasForSelectedSprites()
        {
            SpriteAtlas sa = new SpriteAtlas();

            var path = EditorUtility.SaveFilePanelInProject("Choose where to save the SpriteAtlas", "atlas", "spriteatlas", "");

            var projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            var texturesPath = EditorUtility.OpenFolderPanel("Pick the texture folder", "Assets/Art/Sprites/Characters/", "");
            var projectTexturesPath = texturesPath.Replace(projectFolder, "");

            var filenames =
                (from texturePath in Directory.GetFiles(texturesPath, "*.png")
                select Path.GetFileName(texturePath)).ToList();

            Debug.Log($"Loading from {texturesPath}, removed {projectFolder}");

            var percent = 0f;
            var index = 0f;

            foreach (var filename in filenames)
            {
                percent = (++index / filenames.Count);

                progressBar($"Processing {filename}..", percent);

                var assetPath = Path.Combine(projectTexturesPath, filename);
                var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                foreach (var obj in objects)
                {
                    Debug.Log(obj.ToString());

                    if (obj is Texture2D texture)
                    {
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(texture));

                        SpriteAtlasExtensions.Add(sa, new UnityEngine.Object[] { sprite });
                    }
                }
            }

            percent = 0f;
            index = 0f;

            AssetDatabase.CreateAsset(sa, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Reload the atlas from disk, since the original one is destroyed by unity when saved
            var sprites =
                (from packable in sa.GetPackables()
                 select packable as Sprite).ToList();

            var frames = new Dictionary<int, Dictionary<int, Sprite>> { };
            var clips = new Dictionary<int, AnimationClip> { };

            Debug.Log($"Clips {frames}");

            foreach (var sprite in sprites)
            {
                var components = sprite.texture.name.Split(new char[] { '-' });
                var angle = Int32.Parse(components[0]);
                var frameNumber = Int32.Parse(components[1]);

                if (!frames.ContainsKey(angle))
                    frames[angle] = new Dictionary<int, Sprite> { };

                Debug.Log($"Adding frame {frameNumber} for angle {angle}");

                frames[angle].Add(frameNumber, sprite);
            }

            foreach (var angle in frames.Keys)
            {
                percent += (++index / frames.Count);

                progressBar($"Processing angle {angle}..", percent);

                var animClip = new AnimationClip();
                var spriteBinding = new EditorCurveBinding();

                spriteBinding.type = typeof(SpriteRenderer);
                spriteBinding.path = "";
                spriteBinding.propertyName = "m_Sprite";

                var spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Count];

                for (var i = 0; i < sprites.Count; i++)
                {
                    spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                    spriteKeyFrames[i].time = i;
                    spriteKeyFrames[i].value = sprites[i];
                }

                AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);

                var clipName = path.Replace(".spriteatlas", $"-{angle}.anim");

                Debug.Log($"Saving clip {clipName}");

                AssetDatabase.CreateAsset(animClip, clipName);

                clips[angle] = animClip;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var controllerPath = path.Replace(".spriteatlas", ".controller");

            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            var rootStateMachine = controller.layers[0].stateMachine;

            var blendTree = new BlendTree();

            controller.CreateBlendTreeInController("Walking", out blendTree);

            var blendIndex = controller.parameters.ToList().FindIndex((parameter) => parameter.name == "Blend");

            if (blendIndex > 0)
            {
                controller.RemoveParameter(blendIndex);
            }

            controller.AddParameter("BlendX", AnimatorControllerParameterType.Float);
            controller.AddParameter("BlendY", AnimatorControllerParameterType.Float);

            blendTree.blendType = BlendTreeType.SimpleDirectional2D;

            blendTree.blendParameter = "BlendX";
            blendTree.blendParameterY = "BlendY";

            var directions = 12;

            for (var i = 0; i < directions; i++)
            {
                var direction = Quaternion.AngleAxis(-i * (360f / directions), Vector3.forward) * Vector2.up;

                if (clips.ContainsKey(i))
                {
                    blendTree.AddChild(clips[i], direction);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
        }

        private static void progressBar(string message, float percent)
        {
            EditorUtility.DisplayProgressBar("Animation importer", message, percent);
        }
    }

}