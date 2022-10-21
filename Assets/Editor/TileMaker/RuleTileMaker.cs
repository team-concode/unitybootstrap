using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RuleTileMaker : EditorWindow {
    private Texture2D texture;
    private RuleTile template;
    private bool hasAnim;
    private int animCount = 1;
    private float animSpeed = 1;
    
    [MenuItem("Window/RuleTile Maker")]
    public static void ImportMenu() {
        GetWindow(typeof(RuleTileMaker), false, "RuleTile Maker");
    }

    public void OnGUI() {
        EditorGUILayout.Space();
        texture = EditorGUILayout.ObjectField("Tile Texture", texture, typeof(Texture2D), true) as Texture2D;
        template = EditorGUILayout.ObjectField("Template", template, typeof(RuleTile), true) as RuleTile;
        hasAnim = EditorGUILayout.Toggle("Animation", hasAnim);
        
        if (hasAnim) {
            animCount = EditorGUILayout.IntField("Anim Count", animCount);
            animSpeed = EditorGUILayout.FloatField("Anim Speed", animSpeed);
        }

        if (GUILayout.Button("Build")) {
            Build();
        }
    }

    private void Build() {
        if (texture == null) {
            log.error("Select Target First");
            return;
        }
        
        var path = AssetDatabase.GetAssetPath(texture);
        var dir = Path.GetDirectoryName(path);
        var fileName = Path.GetFileNameWithoutExtension(path);
        var to = Path.Combine(dir, fileName + ".asset");

        var sprites = GetAllSpritesFromAssetFile(path);
        var spriteMap = new Dictionary<string, Sprite>(); 
        foreach (var sprite in sprites) {
            spriteMap[sprite.name] = sprite;
        }

        try {
            var tile = BuildRuleTile(fileName, spriteMap);
            SaveRuleTile(tile, to);
        } catch (Exception e) {
            log.error(e.ToString());
        }
    }

    private RuleTile BuildRuleTile(string name, Dictionary<string, Sprite> sprites) {
        log.error(name);
        RuleTile tile = CreateInstance<RuleTile>();
        tile.m_DefaultSprite = sprites[name + "_3_0" + (hasAnim ? "_0" : "")];
        tile.m_DefaultColliderType = Tile.ColliderType.Sprite;

        foreach (var templateRule in template.m_TilingRules) {
            var no = templateRule.m_Sprites[0].name;
            no = no.Substring(no.IndexOf("_"));
            if (templateRule.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Animation) {
                no = no.Substring(0, no.LastIndexOf('_'));
            }

            var rule = new RuleTile.TilingRule();
            rule.m_NeighborPositions = templateRule.m_NeighborPositions;
            rule.m_Neighbors = templateRule.m_Neighbors;

            rule.m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single;
            if (hasAnim) {
                rule.m_Output = RuleTile.TilingRuleOutput.OutputSprite.Animation;
                rule.m_MinAnimationSpeed = animSpeed;
                rule.m_MaxAnimationSpeed = animSpeed;
            }
            
            rule.m_Sprites = new Sprite[animCount];
            for (int index = 0; index < animCount; index++) {
                var spriteName = name + no + (hasAnim ? "_" + index : "");
                rule.m_Sprites[index] = sprites[spriteName];
            } 
            
            tile.m_TilingRules.Add(rule);
        }
        
        return tile;
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

    private void SaveRuleTile(RuleTile tile, string path) {
        var from = AssetDatabase.LoadAssetAtPath<RuleTile>(path);
        if (from == null) {
            AssetDatabase.CreateAsset(tile, path);
            from = tile;
        }

        from.m_DefaultSprite = tile.m_DefaultSprite;
        from.m_TilingRules = tile.m_TilingRules;
        
        EditorUtility.SetDirty(from);
        AssetDatabase.SaveAssets();
    }
}
