using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using Microsoft.MixedReality.OpenXR;  
using Microsoft.MixedReality.OpenXR.ARSubsystems;

public class SpiceManager : MonoBehaviour
{
    [Tooltip("シーン内のARマーカーマネージャー (Inspectorで割り当て)")]
    public ARMarkerManager MarkerManager;

    [Tooltip("Inspectorで設定する、すべての調味料データリスト")]
    public List<SpiceData> seasoningList;

    void Start()
    {
        if (MarkerManager == null)
        {
            Debug.LogError("ARMarkerManager が割り当てられていません。");
            return;
        }

        // ▼ QRコードのイベント購読
        MarkerManager.markersChanged += OnARMarkersChanged;

        // ▼ 最初はハイライトを非表示にする
        foreach (var data in seasoningList)
        {
            if (data.HighlightObject != null)
            {
                data.HighlightObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (MarkerManager != null)
        {
            MarkerManager.markersChanged -= OnARMarkersChanged;
        }
    }

    // ================================================================
    // QRコードの検出イベント (ログ強化版)
    // ================================================================
    private void OnARMarkersChanged(ARMarkersChangedEventArgs args)
    {
        // 1. 新しく見つかったマーカーをチェック
        foreach (var marker in args.added)
        {
            ProcessMarker(marker, "新規発見");
        }

        // 2. 情報が更新されたマーカーもチェック
        // (※重要: 最初のフレームではデータが空で、次のフレームで文字が入ることがあるため)
        foreach (var marker in args.updated)
        {
            ProcessMarker(marker, "更新");
        }
    }

    // マーカー処理の共通メソッド
    private void ProcessMarker(ARMarker marker, string state)
    {
        // QRコードの文字列を取得
        string decodedData = marker.GetDecodedString();

        // データが空なら「見つけたけどまだ読めてない」とログを出す
        if (string.IsNullOrEmpty(decodedData))
        {
            // Debug.Log($"[{state}] QRコードを認識しましたが、データはまだ空です...");
            return; 
        }

        // データが入っていたら、はっきりとログを出す
        Debug.Log($"👁️‍🗨️ 【{state}】QRコード読み取り成功！ 内容: 「{decodedData}」");

        // ▼ リストから一致する調味料を探す
        SpiceData data = seasoningList.Find(d => d.QrCodeData == decodedData);

        if (data != null)
        {
            Debug.Log($"   ➡ リスト内の調味料「{data.SeasoningName}」と一致しました。");

            // まだアンカー登録されていなければ登録
            if (!data.IsAnchorRegistered)
            {
                RegisterAnchorForSpice(marker, data);
            }
        }
        else
        {
            Debug.LogWarning($"   ⚠️ リストに登録されていないQRコードです: {decodedData}");
        }
    }

    // ================================================================
    // マーカー位置にアンカーを作成してハイライトを固定
    // ================================================================
    private void RegisterAnchorForSpice(ARMarker marker, SpiceData data)
    {
        Transform markerTransform = marker.transform;

        // ▼ アンカーのルートオブジェクトを生成
        GameObject anchorRoot = new GameObject($"Anchor_{data.SeasoningName}");
        anchorRoot.transform.SetPositionAndRotation(markerTransform.position, markerTransform.rotation);

        // ▼ アンカーを追加（空間に固定）
        ARAnchor anchor = anchorRoot.AddComponent<ARAnchor>();

        // ▼ ハイライトをアンカーの子にし、表示開始
        if (data.HighlightObject != null)
        {
            data.HighlightObject.transform.SetParent(anchorRoot.transform, true);
            data.HighlightObject.transform.localPosition = Vector3.zero; // 位置ズレ防止のためリセット
            data.HighlightObject.transform.localRotation = Quaternion.identity;
            data.HighlightObject.SetActive(true);
        }

        // ▼ 状態更新
        data.IsAnchorRegistered = true;

        Debug.Log($"✅ 【完了】空間アンカーを作成し、{data.SeasoningName} の位置を固定しました。");
    }

    // ================================================================
    // レシピ工程から呼び出されるハイライトの ON/OFF
    // ================================================================
    public void HighlightSeasoning(string requiredSeasoningName, bool show)
    {
        SpiceData data = seasoningList.Find(d => d.SeasoningName == requiredSeasoningName);

        if (data != null && data.IsAnchorRegistered && data.HighlightObject != null)
        {
            data.HighlightObject.SetActive(show);
            Debug.Log($"🔦 ハイライト切り替え: {data.SeasoningName} -> {(show ? "ON" : "OFF")}");
        }
    }
}