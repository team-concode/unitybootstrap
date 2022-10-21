using System;
using UnityEditor.Animations;
using UnityEngine;


namespace AsepriteImporter {
    public enum AseFileImportType {
        Sprite,
        Tileset,
    }

    public enum TileNameType {
        Index,
        RowCol,
    }

    public enum EmptyTileBehaviour {
        Keep,
        Remove
    }
    
    public enum AseEditorBindType {
        SpriteRenderer,
        UIImage
    }
    
    public enum AseAnimatorType {
        None,
        AnimatorController,
        AnimatorOverrideController
    }

    [Serializable]
    public class AseFileTextureSettings {
        [SerializeField] public AseFileImportType importType = AseFileImportType.Sprite;
        [SerializeField] public int pixelsPerUnit = 16;
        [SerializeField] public int spriteAlignment;
        [SerializeField] public Vector2 spritePivot = new Vector2(0.5f, 0.5f);
        
        [SerializeField] public AseEditorBindType bindType = AseEditorBindType.SpriteRenderer;
        [SerializeField] public AseAnimatorType animType = AseAnimatorType.None;
        [SerializeField] public AnimatorController baseAnimator;
        [SerializeField] public bool buildAtlas;
        [SerializeField] public bool fitSize;

        [SerializeField] public Vector2Int tileSize = new Vector2Int(16, 16);
        [SerializeField] public TileNameType tileNameType = TileNameType.Index;
        [SerializeField] public EmptyTileBehaviour tileEmpty = EmptyTileBehaviour.Keep;
        [SerializeField] public bool expandEdge = true;
        [SerializeField] public int margin = 1;
        [SerializeField] public int padding;
    }
}