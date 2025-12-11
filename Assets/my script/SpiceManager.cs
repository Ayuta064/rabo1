using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using Microsoft.MixedReality.OpenXR; 
using Microsoft.MixedReality.OpenXR.ARSubsystems;

public class SpiceManager : MonoBehaviour
{
    [Header("Basic Settings")]
    public ARMarkerManager MarkerManager;
    public List<SpiceData> seasoningList;

    [Header("Optional Settings")]
    [Tooltip("指から出すビームのプレハブ (空欄ならビームなし)")]
    public GameObject BeamPrefab;

    private GameObject activeBeamInstance;
    private BeamController activeBeamController;

    void Start()
    {
        if (MarkerManager == null) MarkerManager = FindObjectOfType<ARMarkerManager>();
        
        if (MarkerManager != null)
        {
            MarkerManager.markersChanged += OnARMarkersChanged;
        }

        // 初期化: 全ハイライトを非表示
        TurnOffAllHighlights();

        // ▼▼▼ テスト用: QRなしで強制登録するコード (本番はコメントアウト) ▼▼▼
        //#if UNITY_EDITOR
        // StartCoroutine(DebugSimulateQR());
        //#endif
    }

    void OnDestroy()
    {
        if (MarkerManager != null) MarkerManager.markersChanged -= OnARMarkersChanged;
    }

    // ----------------------------------------------------------------
    // 1. QRコード検出処理
    // ----------------------------------------------------------------
    private void OnARMarkersChanged(ARMarkersChangedEventArgs args)
    {
        foreach (var marker in args.added) ProcessMarker(marker);
        foreach (var marker in args.updated) ProcessMarker(marker);
    }

    private void ProcessMarker(ARMarker marker)
    {
        string text = marker.GetDecodedString();
        if (string.IsNullOrEmpty(text)) return;

        SpiceData data = seasoningList.Find(d => d.QrCodeData == text);

        if (data != null && !data.IsAnchorRegistered)
        {
            RegisterAnchorForSpice(marker, data);
        }
    }

    private void RegisterAnchorForSpice(ARMarker marker, SpiceData data)
    {
        GameObject anchorRoot = new GameObject($"Anchor_{data.SeasoningName}");
        anchorRoot.transform.SetPositionAndRotation(marker.transform.position, marker.transform.rotation);
        anchorRoot.AddComponent<ARAnchor>();

        if (data.HighlightObject != null)
        {
            data.HighlightObject.transform.SetParent(anchorRoot.transform, true);
            data.HighlightObject.transform.localPosition = Vector3.zero;
            data.HighlightObject.transform.localRotation = Quaternion.identity;
            
            // 登録成功の合図（3秒ピカッ）
            StartCoroutine(FlashHighlight(data.HighlightObject, 3.0f));
        }

        data.IsAnchorRegistered = true;
        Debug.Log($"✅ QR登録完了: {data.SeasoningName}");
    }

    private IEnumerator FlashHighlight(GameObject obj, float duration)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
    }

    // ----------------------------------------------------------------
    // 2. レシピ連携 & ビーム制御 (デバッグ強化版)
    // ----------------------------------------------------------------
    public void HighlightSeasoning(string requiredSeasoningName, bool show)
    {
        // 1. 名前で検索
        SpiceData data = seasoningList.Find(d => d.SeasoningName == requiredSeasoningName);

        // ▼ エラー診断 ▼
        if (data == null)
        {
            Debug.LogError($"❌ エラー: '{requiredSeasoningName}' がリストに見つかりません！Inspectorの'Seasoning Name'と一致していますか？(空白注意)");
            return;
        }
        if (data.HighlightObject == null)
        {
            Debug.LogError($"❌ エラー: '{requiredSeasoningName}' のHighlight Objectが空です！Inspectorでセットしてください。");
            return;
        }
        if (!data.IsAnchorRegistered)
        {
            Debug.LogWarning($"⚠️ 待機中: '{requiredSeasoningName}' を表示したいですが、まだQRコードが読み込まれていません。実物のQRを見てください。");
            return;
        }

        // ▼ 表示処理 ▼
        if (show)
        {
            Debug.Log($"✨ ハイライトON: {requiredSeasoningName}");
            data.HighlightObject.SetActive(true);
            if (BeamPrefab != null) ControlBeam(data, true);
        }
        else
        {
            data.HighlightObject.SetActive(false);
            // 個別のOFF指示だが、今は全消し関数を使う運用なのでここはシンプルでOK
        }
    }

    // すべて消す (レシピのページめくり時に呼ぶ)
    public void TurnOffAllHighlights()
    {
        // ビーム停止
        if (activeBeamInstance != null)
        {
            activeBeamInstance.SetActive(false);
            if (activeBeamController != null) activeBeamController.StopBeam();
        }

        // 全アイコン消灯
        foreach (var data in seasoningList)
        {
            if (data.HighlightObject != null)
            {
                data.HighlightObject.SetActive(false);
            }
        }
    }

    private void ControlBeam(SpiceData data, bool show)
    {
        if (show)
        {
            if (activeBeamInstance == null)
            {
                activeBeamInstance = Instantiate(BeamPrefab);
                activeBeamController = activeBeamInstance.GetComponent<BeamController>();
            }
            
            if (activeBeamController != null)
            {
                // アイコンの親(Anchor)をターゲットにする
                activeBeamController.SetTarget(data.HighlightObject.transform);
                activeBeamInstance.SetActive(true);
            }
        }
        else
        {
            if (activeBeamInstance != null)
            {
                activeBeamInstance.SetActive(false);
                if(activeBeamController != null) activeBeamController.StopBeam();
            }
        }
    }

    // デバッグ用 (無効化中)
    private IEnumerator DebugSimulateQR()
    {
        yield return new WaitForSeconds(1.0f);
        foreach (var data in seasoningList)
        {
            if (data.HighlightObject != null)
            {
                GameObject fakeAnchor = new GameObject($"FakeAnchor_{data.SeasoningName}");
                fakeAnchor.transform.position = data.HighlightObject.transform.position;
                fakeAnchor.transform.rotation = data.HighlightObject.transform.rotation;
                data.HighlightObject.transform.SetParent(fakeAnchor.transform);
                data.IsAnchorRegistered = true;
                Debug.Log($"🧪 強制登録: {data.SeasoningName}");
            }
        }
    }
}