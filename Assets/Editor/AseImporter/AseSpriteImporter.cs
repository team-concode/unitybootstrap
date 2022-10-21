using System;
using System.Collections.Generic;
using System.IO;
using Aseprite;
using Aseprite.Chunks;
using Aseprite.Utils;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace AsepriteImporter {
    public class AseSpriteImporter {
        private AseFileTextureSettings settings;
        private Vector2Int sizeOrigin;
        private RectInt fitRect;
        private string fileName;
        private string directoryName;
        private string filePath;
        private int updateLimit;
        private int rows;
        private int cols;
        private Texture2D []frames;
        private AseFile file;
        
        public void Import(string path, AseFile file, AseFileTextureSettings settings) {
            this.file = file;
            this.settings = settings;
            this.sizeOrigin = new Vector2Int(file.Header.Width, file.Header.Height); 

            frames = file.GetFrames();
            BuildAtlas(path);
            
            updateLimit = 300;
            EditorApplication.update += OnUpdate;
        }

        private void OnUpdate() {
            AssetDatabase.Refresh();
            var done = false;
            if (GenerateSprites()) {
                GeneratorAnimations();
                done = true;
            } else {
                updateLimit--;
                if (updateLimit <= 0) {
                    done = true;
                }
            }

            if (done) {
                EditorApplication.update -= OnUpdate;
            }
        }

        private void BuildAtlas(string acePath) {
            fileName = Path.GetFileNameWithoutExtension(acePath);
            directoryName = Path.GetDirectoryName(acePath) + "/" + fileName;
            if (!AssetDatabase.IsValidFolder(directoryName)) {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(acePath), fileName);
            }

            filePath = directoryName + "/" + fileName + ".png";

            var atlas = GenerateAtlas(frames);
            try {
                File.WriteAllBytes(filePath, atlas.EncodeToPNG());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            } catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }

        private RectInt GetFitRect(Texture2D sprite) {
            var xMin = sizeOrigin.x;
            var yMin = sizeOrigin.y;
            var xMax = 0;
            var yMax = 0;
            var pixels = sprite.GetPixels();
            for (var y = 0; y < sizeOrigin.y; y++) {
                for (var x = 0; x < sizeOrigin.x; x++) {
                    var index = sizeOrigin.x * y + x;
                    if (pixels[index].a == 0) {
                        continue;
                    }

                    xMin = Mathf.Min(xMin, x);
                    xMax = Mathf.Max(xMax, x);
                    yMin = Mathf.Min(yMin, y);
                    yMax = Mathf.Max(yMax, y);
                }
            }

            return new RectInt(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1);
        }

        private RectInt GetFitRect(Texture2D[] sprites) {
            var xMin = sizeOrigin.x;
            var yMin = sizeOrigin.y;
            var xMax = 0;
            var yMax = 0;
            foreach (var sprite in sprites) {
                var rect = GetFitRect(sprite);
                if (rect.width <= 0) {
                    continue;
                }

                xMin = Mathf.Min(xMin, rect.xMin);
                xMax = Mathf.Max(xMax, rect.xMax);
                yMin = Mathf.Min(yMin, rect.yMin);
                yMax = Mathf.Max(yMax, rect.yMax);
            }

            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public Texture2D GenerateAtlas(Texture2D []sprites) {
            if (settings.fitSize) {
                fitRect = GetFitRect(sprites);
            } else {
                fitRect = new RectInt(0, 0, sizeOrigin.x, sizeOrigin.y);
            }

            var w = fitRect.width;
            var h = fitRect.height;

            var padding = settings.padding;
            var area = w * h * sprites.Length;
            if (sprites.Length < 4) {
                if (w <= h) {
                    cols = sprites.Length;
                    rows = 1;
                } else {
                    rows = sprites.Length;
                    cols = 1;
                }
            } else {
                var sqrt = Mathf.Sqrt(area);
                cols = Mathf.CeilToInt(sqrt / w);
                rows = Mathf.CeilToInt(sqrt / h);
                if (sprites.Length <= cols * (rows - 1)) {
                    rows--;
                } 
            }

            var width = cols * (w + padding * 2);
            var height = rows * (h + padding * 2);
            var atlas = Texture2DUtil.CreateTransparentTexture(width, height);

            var index = 0;
            for (var row = 0; row < rows; row++) {
                for (var col = 0; col < cols; col++) {
                    if (index == sprites.Length) {
                        break;
                    }

                    var sprite = sprites[index];
                    var rect = new RectInt(col * (w + padding * 2) + padding, 
                                           height - (row + 1) * (h + padding * 2) + padding, 
                                           w, 
                                           h);
                    CopyColors(sprite, atlas, rect);
                    index++;
                }
            }

            return atlas;
        }

        private Color[] GetPixels(Texture2D sprite) {
            var res = sprite.GetPixels(fitRect.x, fitRect.y, fitRect.width, fitRect.height);
            return res;
        }

        private void CopyColors(Texture2D sprite, Texture2D atlas, RectInt to) {
            atlas.SetPixels(to.x, to.y, to.width, to.height, GetPixels(sprite));
        }
        
        private bool GenerateSprites() {
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer == null) {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = settings.pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;

            var metaList = CreateMetaData(fileName);
            var oldProperties = AseSpritePostProcess.GetPhysicsShapeProperties(importer, metaList);
            var borders = AseSpritePostProcess.GetPrevBorders(importer, metaList);
            
            for (var index = 0; index < metaList.Count; index++) {
                var meta = metaList[index];
                if (index < borders.Count) {
                    meta.border = borders[index];
                }
                metaList[index] = meta;
            }
            
            importer.spritesheet = metaList.ToArray();
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            EditorUtility.SetDirty(importer);
            try {
                importer.SaveAndReimport();
            } catch (Exception e) {
                Debug.LogWarning("There was a problem with generating sprite file: " + e);
            }

            var newProperties = AseSpritePostProcess.GetPhysicsShapeProperties(importer, metaList);

            AseSpritePostProcess.RecoverPhysicsShapeProperty(newProperties, oldProperties);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            return true;
        }

        private List<SpriteMetaData> CreateMetaData(string fileName) {
            var w = fitRect.width;
            var h = fitRect.height;
            var padding = settings.padding;
            var res = new List<SpriteMetaData>();
            var index = 0;
            var height = rows * (h + padding * 2);
            var done = false;
            var count10 = frames.Length >= 100 ? 3 : (frames.Length >= 10 ? 2 : 1);
            
            for (var row = 0; row < rows; row++) {
                for (var col = 0; col < cols; col++) {
                    var rect = new Rect(col * (w + padding * 2) + padding,
                                         height - (row + 1) * (h + padding * 2) + padding, 
                                         w,
                                         h);
                    var meta = new SpriteMetaData();
                    meta.name = fileName + "_" + index.ToString("D" + count10);
                    meta.rect = rect;
                    meta.alignment = settings.spriteAlignment;
                    meta.pivot = settings.spritePivot;
                    res.Add(meta);
                    index++;

                    if (index >= frames.Length) {
                        done = true;
                        break;
                    }
                }

                if (done) {
                    break;
                }
            }

            return res;
        }

        private void GeneratorAnimations() {
            var sprites = GetAllSpritesFromAssetFile(filePath);
            sprites.Sort((lhs, rhs) => String.CompareOrdinal(lhs.name, rhs.name));

            var clips = GenerateAnimations(file, sprites);
            if (settings.buildAtlas) {
                Debug.Log("Generate Atlas");
                CreateSpriteAtlas(sprites);
            }

            if (settings.animType == AseAnimatorType.AnimatorController) {
                Debug.Log("Generate AnimatorController");
                CreateAnimatorController(clips);
            } else if (settings.animType == AseAnimatorType.AnimatorOverrideController) {
                Debug.Log("Generate AnimatorOverrideController");
                CreateAnimatorOverrideController(clips);
            }
        }

        private WrapMode GetDefaultWrapMode(string animName) {
            animName = animName.ToLower();
            if (animName.IndexOf("walk", StringComparison.Ordinal) >= 0 || 
                animName.IndexOf("run", StringComparison.Ordinal) >= 0 || 
                animName.IndexOf("idle", StringComparison.Ordinal) >= 0) {
                return WrapMode.Loop;
            }

            return WrapMode.Once;
        }
        
        private List<AnimationClip> GenerateAnimations(AseFile aseFile, List<Sprite> sprites) {
            List<AnimationClip> res = new List<AnimationClip>();
            var animations = aseFile.GetAnimations();
            if (animations.Length <= 0) {
                return res;
            }

            var metadatas = aseFile.GetMetaData(settings.spritePivot, settings.pixelsPerUnit);

            int index = 0;
            foreach (var animation in animations) {
                var path = directoryName + "/" + fileName + "_" + animation.TagName + ".anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, path);
                    clip.wrapMode = GetDefaultWrapMode(animation.TagName);
                } else {
                    AnimationClipSettings animSettings = AnimationUtility.GetAnimationClipSettings(clip);
                    clip.wrapMode = animSettings.loopTime ? WrapMode.Loop : WrapMode.Once; 
                }
                
                clip.name = fileName + "_" + animation.TagName;
                clip.frameRate = 25;

                EditorCurveBinding editorBinding = new EditorCurveBinding();
                editorBinding.path = "";
                editorBinding.propertyName = "m_Sprite";

                switch (this.settings.bindType) {
                    case AseEditorBindType.SpriteRenderer:
                        editorBinding.type = typeof(SpriteRenderer);
                        break;
                    case AseEditorBindType.UIImage:
                        editorBinding.type = typeof(Image);
                        break;
                }

                // plus last frame to keep the duration
                int length = animation.FrameTo - animation.FrameFrom + 1;
                ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[length + 1];
                Dictionary<string, AnimationCurve> transformCurveX = new Dictionary<string, AnimationCurve>(),
                                                   transformCurveY = new Dictionary<string, AnimationCurve>();

                float time = 0;
                int from = (animation.Animation != LoopAnimation.Reverse) ? animation.FrameFrom : animation.FrameTo;
                int step = (animation.Animation != LoopAnimation.Reverse) ? 1 : -1;

                int keyIndex = from;
                for (int i = 0; i < length; i++) {
                    if (i >= length) {
                        keyIndex = from;
                    }

                    ObjectReferenceKeyframe frame = new ObjectReferenceKeyframe();
                    frame.time = time;
                    frame.value = sprites[keyIndex];

                    time += aseFile.Frames[keyIndex].FrameDuration / 1000f;
                    spriteKeyFrames[i] = frame;

                    foreach (var metadata in metadatas) {
                        if (metadata.Type == MetaDataType.TRANSFORM && metadata.Transforms.ContainsKey(keyIndex)) {
                            var childTransform = metadata.Args[0];
                            if (!transformCurveX.ContainsKey(childTransform)) {
                                transformCurveX[childTransform] = new AnimationCurve();
                                transformCurveY[childTransform] = new AnimationCurve();
                            }
                            var pos = metadata.Transforms[keyIndex];
                            transformCurveX[childTransform].AddKey(i, pos.x);
                            transformCurveY[childTransform].AddKey(i, pos.y);
                        }
                    }

                    keyIndex += step;
                }

                float frameTime = 1f / clip.frameRate;
                ObjectReferenceKeyframe lastFrame = new ObjectReferenceKeyframe();
                lastFrame.time = time - frameTime;
                lastFrame.value = sprites[keyIndex - step];

                spriteKeyFrames[spriteKeyFrames.Length - 1] = lastFrame;
                foreach (var metadata in metadatas) {
                    if (metadata.Type == MetaDataType.TRANSFORM && metadata.Transforms.ContainsKey(keyIndex - step)) {
                        var childTransform = metadata.Args[0];
                        var pos = metadata.Transforms[keyIndex - step];
                        transformCurveX[childTransform].AddKey(spriteKeyFrames.Length - 1, pos.x);
                        transformCurveY[childTransform].AddKey(spriteKeyFrames.Length - 1, pos.y);
                    }
                }

                AnimationUtility.SetObjectReferenceCurve(clip, editorBinding, spriteKeyFrames);
                foreach (var childTransform in transformCurveX.Keys) {
                    EditorCurveBinding
                    bindingX = new EditorCurveBinding {
                        path = childTransform, 
                        type = typeof(Transform), 
                        propertyName = "m_LocalPosition.x"
                    },
                    bindingY = new EditorCurveBinding {
                        path = childTransform, 
                        type = typeof(Transform), 
                        propertyName = "m_LocalPosition.y"
                    };
                    MakeConstant(transformCurveX[childTransform]);
                    AnimationUtility.SetEditorCurve(clip, bindingX, transformCurveX[childTransform]);
                    MakeConstant(transformCurveY[childTransform]);
                    AnimationUtility.SetEditorCurve(clip, bindingY, transformCurveY[childTransform]);
                }

                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                clipSettings.loopTime = (clip.wrapMode == WrapMode.Loop);

                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
                EditorUtility.SetDirty(clip);
                index++;
                res.Add(clip);
            }

            return res;
        }

        private static void MakeConstant(AnimationCurve curve) {
            for (int i = 0; i < curve.length; ++i) {
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }
        }
        
        private static List<Sprite> GetAllSpritesFromAssetFile(string imageFilename) {
            var assets = AssetDatabase.LoadAllAssetsAtPath(imageFilename);

            // make sure we only grab valid sprites here
            List<Sprite> sprites = new List<Sprite>();
            foreach (var item in assets) {
                if (item is Sprite) {
                    sprites.Add(item as Sprite);
                }
            }

            return sprites;
        }
        
        private void CreateAnimatorController(List<AnimationClip> animations) {
            var path = directoryName + "/" + fileName + ".controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            if (controller == null) {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                controller.AddLayer("Default");

                foreach (var animation in animations) {
                    var stateName = animation.name;
                    stateName = stateName.Replace(fileName + "_", "");
                    
                    AnimatorState state = controller.layers[0].stateMachine.AddState(stateName);
                    state.motion = animation;
                }
            } else {
                var clips = new Dictionary<string, AnimationClip>();
                foreach (var anim in animations) {
                    var stateName = anim.name;
                    stateName = stateName.Replace(fileName + "_", "");
                    clips[stateName] = anim;
                }
                
                var childStates = controller.layers[0].stateMachine.states;
                foreach (var childState in childStates) {
                    if (clips.TryGetValue(childState.state.name, out AnimationClip clip)) {
                        childState.state.motion = clip;
                    }
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        
        private void CreateAnimatorOverrideController(List<AnimationClip> animations) {
            var path = directoryName + "/" + fileName + ".overrideController";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            var baseController = controller?.runtimeAnimatorController;
            if (controller == null) {
                controller = new AnimatorOverrideController();
                AssetDatabase.CreateAsset(controller, path);
                baseController = settings.baseAnimator;
            } 
            
            if (baseController == null) {
                Debug.LogError("Can not make override controller");
                return;
            }

            controller.runtimeAnimatorController = baseController;
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var anim in animations) {
                var stateName = anim.name;
                stateName = stateName.Replace(fileName + "_", "");
                clips[stateName] = anim;
            }
            
            var clipPairs = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
            controller.GetOverrides(clipPairs);

            foreach (var pair in clipPairs) {
                string animationName = pair.Key.name;
                if (clips.TryGetValue(animationName, out AnimationClip clip)) {
                    controller[animationName] = clip;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        
        private void CreateSpriteAtlas(List<Sprite> sprites) {
            var path = directoryName + "/" + fileName + ".spriteatlas";
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (atlas == null) {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, path);
            }

            var texSetting = new SpriteAtlasTextureSettings();
            texSetting.filterMode = FilterMode.Point;
            texSetting.generateMipMaps = false;

            var packSetting = new SpriteAtlasPackingSettings();
            packSetting.padding = 2;
            packSetting.enableRotation = false;
            packSetting.enableTightPacking = true;

            var platformSetting = new TextureImporterPlatformSettings();
            platformSetting.textureCompression = TextureImporterCompression.Uncompressed;
            
            atlas.SetTextureSettings(texSetting);
            atlas.SetPackingSettings(packSetting);
            atlas.SetPlatformSettings(platformSetting);
            atlas.Add(sprites.ToArray());
            
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
        }        
    }
}