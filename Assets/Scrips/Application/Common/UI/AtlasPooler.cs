using System.Collections.Generic;
using UnityBean;
using UnityEngine;
using UnityEngine.U2D;

[Module]
public class AtlasPooler {
    private Dictionary<string, SpriteAtlas> atlasSet = new Dictionary<string, SpriteAtlas>();

    public SpriteAtlas GetAtlas(string name) {
        SpriteAtlas atlas;
        if (atlasSet.TryGetValue(name, out atlas)) {
            return atlas;
        }

        atlas = Resources.Load<SpriteAtlas>("Atlas/" + name);
        atlasSet.Add(name, atlas);
        return atlas;
    }
}

