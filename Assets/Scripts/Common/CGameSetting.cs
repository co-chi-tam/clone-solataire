using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameSetting
{
    
    #region CONFIG

    public static float DRAG_SPEED_THREAHOLD = 0.75f;

    #endregion

    #region CACHE

    protected static Dictionary<string, Sprite> SPRITE_CACHE = new Dictionary<string, Sprite>();

    public static Sprite GetSpriteWithPath(string path)
    {
        if (SPRITE_CACHE.ContainsKey(path))
            return SPRITE_CACHE[path];
        // SPRITE
        var spriteResource = Resources.Load<Sprite>(path);
        if (spriteResource != null)
            SPRITE_CACHE.Add (path, spriteResource);
        return spriteResource;
    }

    #endregion

    #region Ultilities

    public static ScreenOrientation DeviceToScreenOrientation(DeviceOrientation device)
    {
        switch (device)
        {
            default:
            case DeviceOrientation.Unknown:
            case DeviceOrientation.Portrait:
            case DeviceOrientation.FaceUp:
            case DeviceOrientation.FaceDown:
                return ScreenOrientation.Portrait;

            case DeviceOrientation.PortraitUpsideDown:
                return ScreenOrientation.PortraitUpsideDown;

            case DeviceOrientation.LandscapeLeft:
                return ScreenOrientation.LandscapeLeft;
                
            case DeviceOrientation.LandscapeRight:
                return ScreenOrientation.LandscapeRight;
        }
        // return ScreenOrientation.Portrait;
    } 

    #endregion

}
