using UnityEngine;

public class URL : Singleton<URL> {
    // local path
    //-------------------------------------------------------------------------
    public string localRoot => Application.persistentDataPath + "/" + App.phase;
    public string localSetting => Application.persistentDataPath + "/setting";
    public string localLastSb => localRoot + "/sb";
    
    public string localAccount => localRoot + "/account";
    public string localAccountPrev => localRoot + "/account.prev";
    public string localSlots => localRoot + "/slots";
    public string localSlotsPrev => localRoot + "/slots.prev";
    
    
    public string localWorld => localRoot + "/world";
    public string localArea => localWorld + "{0}/area";
    public string localPlayer => localWorld + "{0}/player";
    public string localArtifact => localWorld + "{0}/artifact";
    public string localHero => localWorld + "{0}/hero";
    public string localPet => localWorld + "{0}/pet";
    public string localEnemy => localWorld + "{0}/enemy";
    public string localDropMaterial => localWorld + "{0}/dropMaterial";
    public string localDropItem => localWorld + "{0}/dropItem";
    public string localBuilding => localWorld + "{0}/building";
    public string localNpc => localWorld + "{0}/npc";
    public string localObject => localWorld + "{0}/object";
    public string localResource => localWorld + "{0}/resource";
    public string localQuest => localWorld + "{0}/quest";
    public string localMaterial => localWorld + "{0}/material";
    public string localMaterialBag => localWorld + "{0}/materialBag";
    public string localItem => localWorld + "{0}/item";
    public string localSkill => localWorld + "{0}/skill";
    public string localScriptLog => localWorld + "{0}/scriptLog";
    public string localBoard => localWorld + "{0}/board";
    public string localCard => localWorld + "{0}/card";
    public string localActionHistory => localWorld + "{0}/actionHistory";
    public string localPlayHistory => localWorld + "{0}/playHistory";

    public string apiHost => "https://twh-api.concode.co";
    public string apiAccount => apiHost + "/account";
    public string apiAccountLoad => apiAccount + "/{0}";
    public string apiAccountSave => apiAccount + "/save/{0}";
    public string apiAccountDelete => apiAccount + "/delete/{0}";
    public string apiAccountAppleRevoke => apiAccount + "/apple/revoke";
    
    public string apiGpa => apiHost + "/gpa";
    public string apiBetaGift => apiGpa + "/reward/{0}";

    public string prefAppleUserIdKey => "AppleUserId";
    public string prefAppleUserIdToken => "AppleUserIdToken";
    public string prefAppleUserAuthorizationCode => "AppleUserAuthorizationCode";
}


public enum Phase {
    Development = 0,
    Production,
}