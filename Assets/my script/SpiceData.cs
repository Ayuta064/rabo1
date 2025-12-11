using UnityEngine;
using System;

[Serializable]
public class SpiceData
{
    [Tooltip("マーカー/QRコードの内容 (例: \"SALT\", \"SUGAR\")")]
    public string QrCodeData; 
    
    [Tooltip("調味料の名前 (Inspectorで表示される)")]
    public string SeasoningName; 
    
    [Tooltip("追跡用のホログラムオブジェクト (ハイライト表示用)")]
    public GameObject HighlightObject; 

    // アンカーが登録済みであるかを示すフラグ
    [HideInInspector]
    public bool IsAnchorRegistered = false; 
}