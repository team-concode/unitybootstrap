using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Tilemaps {
    [Serializable]
    [CreateAssetMenu(fileName = "New Animated Tile", menuName = "Tiles/Animated Tile")]
    public class AnimatedTile : TileBase {
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private float _minSpeed = 1f;
        [SerializeField] private float _maxSpeed = 1f;
        [SerializeField] public bool randomStart;
        [SerializeField] public Tile.ColliderType colliderType;

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            tileData.transform = Matrix4x4.identity;
            tileData.color = Color.white;
            if (_sprites != null && _sprites.Length > 0) {
                tileData.sprite = _sprites[randomStart ? GetRandomFrame() : 0];
                tileData.colliderType = colliderType;
            }
        }

        public override bool GetTileAnimationData(Vector3Int location,
                                                  ITilemap tileMap,
                                                  ref TileAnimationData tileAnimationData) {
            if (_sprites.Length > 0) {
                tileAnimationData.animatedSprites = _sprites;
                tileAnimationData.animationSpeed = Random.Range(_minSpeed, _maxSpeed);
                tileAnimationData.animationStartTime = 0;
                return true;
            }

            return false;
        }

        private int GetRandomFrame() {
            return Random.Range(0, _sprites.Length);
        }
    }
}