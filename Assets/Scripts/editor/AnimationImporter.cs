using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Linq;
using System.IO;
using Pandora.Editor.Data;

namespace Pandora.Editor
{
    public class AnimationImporter
    {
        [MenuItem("Assets/Create SpriteAtlas and AnimationClips for selected Sprites")]
        public static void CreateAtlasForSelectedSprites()
        {
            SpriteAtlas sa = new SpriteAtlas();

            var path = EditorUtility.SaveFilePanelInProject("Choose where to save the SpriteAtlas", "atlas", "spriteatlas", "", "Assets/Art/Sprites/Characters/");

            var projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            var texturesPath = EditorUtility.OpenFolderPanel("Pick the texture folder", "Assets/Art/Sprites/Characters/", "");
            var projectTexturesPath = texturesPath.Replace(projectFolder, "");

            progressBar("Loading clips from manifest", 1f);

            var filenames =
                (from texturePath in Directory.GetFiles(texturesPath, "*.png")
                 select Path.GetFileName(texturePath)).ToList();

            Debug.Log($"Loading from {texturesPath}, removed {projectFolder}");

            foreach (var filename in filenames)
            {
                progressBar($"Processing {filename}..", filenames.Count);

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

            resetProgressBar();

            AssetDatabase.CreateAsset(sa, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Reload the atlas from disk, since the original one is destroyed by unity when saved
            var sprites =
                (from packable in sa.GetPackables()
                 select packable as Sprite).ToList();

            progressBar("Parsing animation manifest", 0f);

            var animationManifest = JsonUtility.FromJson<AnimationManifest>(
                File.ReadAllText(
                    Path.Combine(texturesPath, "animation-manifest.json")
                )
            );

            progressBar("Parsing animation manifest", 1f);

            var animations = new Dictionary<int, ClipManifest> { };
            var blendTrees = new Dictionary<string, BlendTree> { };

            int? framesNum = null;

            progressBar("Loading clips from manifest", 0f);

            var controllerPath = path.Replace(".spriteatlas", ".controller");

            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            controller.AddParameter("BlendX", AnimatorControllerParameterType.Float);
            controller.AddParameter("BlendY", AnimatorControllerParameterType.Float);

            var rootStateMachine = controller.layers[0].stateMachine;

            foreach (var clip in animationManifest.animations)
            {
                progressBar($"Loading clip {clip.name}", animationManifest.animations.Count);

                if (!framesNum.HasValue || framesNum.Value < clip.endFrame)
                {
                    framesNum = clip.endFrame;
                }

                for (var i = clip.startFrame; i <= clip.endFrame; i++)
                {
                    animations[i] = clip;
                }

                var blendTree = new BlendTree();

                controller.CreateBlendTreeInController(clip.name, out blendTree);

                var blendIndex = controller.parameters.ToList().FindIndex((parameter) => parameter.name == "Blend");

                if (blendIndex > 0)
                {
                    controller.RemoveParameter(blendIndex);
                }

                blendTree.blendType = BlendTreeType.SimpleDirectional2D;

                blendTree.blendParameter = "BlendX";
                blendTree.blendParameterY = "BlendY";

                blendTrees[clip.name] = blendTree;
            }

            resetProgressBar();

            var clips = new Dictionary<int, AnimationClip> { };
            var clipFrames = new Dictionary<string, Dictionary<int, Sprite[]>> { };


            foreach (var sprite in sprites)
            {
                progressBar($"Indexing {sprite.texture.name}..", sprites.Count);

                var components = sprite.texture.name.Split(new char[] { '-' });

                var angle = Int32.Parse(components[0]);
                var frameNumber = Int32.Parse(components[1]);

                if (!animations.ContainsKey(frameNumber)) continue;

                var clipManifest = animations[frameNumber];
                var clipName = clipManifest.name;

                if (!clipFrames.ContainsKey(clipManifest.name))
                {
                    clipFrames[clipName] = new Dictionary<int, Sprite[]> { };
                }

                if (!clipFrames[clipName].ContainsKey(angle))
                {
                    clipFrames[clipName][angle] = new Sprite[(clipManifest.endFrame - clipManifest.startFrame) + 1];
                }

                clipFrames[clipName][angle][frameNumber - clipManifest.startFrame] = sprite;

                Debug.Log($"Adding frame {frameNumber} for angle {angle}");
            }

            resetProgressBar();

            foreach (var clip in clipFrames.Keys)
            {
                foreach (var angle in clipFrames[clip].Keys)
                {
                    progressBar($"Building animation clip for {clip} at angle {angle}..", clipFrames[clip].Count);

                    var animClip = new AnimationClip();
                    var spriteBinding = new EditorCurveBinding();

                    spriteBinding.type = typeof(SpriteRenderer);
                    spriteBinding.path = "";
                    spriteBinding.propertyName = "m_Sprite";

                    var clipLength = clipFrames[clip][angle].Length;

                    var spriteKeyFrames = new ObjectReferenceKeyframe[clipLength];

                    for (var i = 0; i < clipLength; i++)
                    {
                        spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                        spriteKeyFrames[i].time = i / animClip.frameRate;
                        spriteKeyFrames[i].value = clipFrames[clip][angle][i];
                    }

                    AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);

                    var clipName = path.Replace(".spriteatlas", $"{clip}-{angle}.anim");

                    Debug.Log($"Saving clip {clipName}");

                    AssetDatabase.CreateAsset(animClip, clipName);

                    var direction = Quaternion.AngleAxis(angle * (360f / clipFrames[clip].Count), Vector3.forward) * Vector2.up;

                    blendTrees[clip].AddChild(animClip, direction);
                }

                resetProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
        }

        private static void progressBar(string message, float percent)
        {
            EditorUtility.DisplayProgressBar("Animation importer", message, percent);
        }

        private static float index = 0f;

        private static void resetProgressBar()
        {
            index = 0f;
        }

        private static void progressBar(string message, int total)
        {
            progressBar(message, ++index / total);
        }
    }

}