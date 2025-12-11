using UnityEngine;
using TMPro; 
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

// データ構造
public class StepData
{
    public string Instruction;
    public string SpiceID;   // Firestoreの "spiceID" (例: "SALT")
    public string VideoUrl;
}

public class RecipeViewer : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("レシピの工程を表示するCanvas上のテキスト")]
    public TextMeshProUGUI instructionText;
    
    [Tooltip("現在のステップ数")]
    public TextMeshProUGUI counterText;

    [Header("Video Settings")]
    public GameObject watchVideoButton; 
    public VideoPopupController videoPopup; 

    [Header("Managers")] 
    [Tooltip("HierarchyにあるSpiceManagerをここにドラッグ")]
    public SpiceManager spiceManager; 

    [Header("Database Settings")]
    public string targetRecipeID = "omlet_cheese";

    private List<StepData> steps = new List<StepData>();
    private int currentIndex = 0;
    private FirebaseFirestore db;

    void Start()
    {
        instructionText.text = "レシピを読み込み中...";
        if (counterText != null) counterText.text = "-- / --";
        if (watchVideoButton != null) watchVideoButton.SetActive(false);

        db = FirebaseFirestore.DefaultInstance;
        LoadRecipeFromFirestore();
    }

    // ---------------------------------------------------------
    // Firestore読み込み
    // ---------------------------------------------------------
    private void LoadRecipeFromFirestore()
    {
        db.Collection("recipes").Document(targetRecipeID).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                instructionText.text = "読み込みエラー";
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                if (data.ContainsKey("steps"))
                {
                    List<object> stepList = data["steps"] as List<object>;
                    ParseSteps(stepList);
                    
                    currentIndex = 0;
                    UpdateDisplay(); // 最初のページを表示
                }
                else
                {
                    instructionText.text = "手順データなし";
                }
            }
            else
            {
                instructionText.text = "レシピが見つかりません";
            }
        });
    }

    private void ParseSteps(List<object> stepList)
    {
        steps.Clear();
        foreach (var item in stepList)
        {
            var map = item as Dictionary<string, object>;
            if (map != null)
            {
                StepData newStep = new StepData();
                newStep.Instruction = map.ContainsKey("instruction") ? map["instruction"].ToString() : "";
                // ▼ Firestoreのフィールド名 "spiceID" を取得
                newStep.SpiceID = map.ContainsKey("spiceID") ? map["spiceID"].ToString() : ""; 
                newStep.VideoUrl = map.ContainsKey("video") ? map["video"].ToString() : "";
                steps.Add(newStep);
            }
        }
    }

    // ---------------------------------------------------------
    // ボタン操作
    // ---------------------------------------------------------
    public void NextStep()
    {
        if (steps.Count == 0) return;
        if (currentIndex < steps.Count - 1)
        {
            currentIndex++;
            UpdateDisplay();
        }
    }

    public void PreviousStep()
    {
        if (steps.Count == 0) return;
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateDisplay();
        }
    }

    public void OnWatchVideoClicked()
    {
        if (steps.Count == 0) return;
        StepData currentStep = steps[currentIndex];
        if (!string.IsNullOrEmpty(currentStep.VideoUrl) && videoPopup != null)
        {
            videoPopup.OpenAndPlay(currentStep.VideoUrl);
        }
    }

    // ---------------------------------------------------------
    // 3. 画面表示の更新 (ここが連携のキモ！)
    // ---------------------------------------------------------
    private void UpdateDisplay()
    {
        if (steps.Count == 0) return;

        StepData currentStep = steps[currentIndex];

        // 1. テキスト更新
        instructionText.text = currentStep.Instruction;
        if (counterText != null) counterText.text = $"{currentIndex + 1} / {steps.Count}";

        // 2. 動画ボタン制御
        if (watchVideoButton != null)
        {
            watchVideoButton.SetActive(!string.IsNullOrEmpty(currentStep.VideoUrl));
        }

        // 3. ハイライト連携
        if (spiceManager != null)
        {
            // A. 前の工程のハイライトを全部消す
            spiceManager.TurnOffAllHighlights();

            // B. 今回のデータに "spiceID" (例: SALT) が入っているか確認
            if (!string.IsNullOrEmpty(currentStep.SpiceID))
            {
                // C. IDを渡して光らせる
                spiceManager.HighlightSeasoning(currentStep.SpiceID, true);
                Debug.Log($"調味料ハイライト指示: {currentStep.SpiceID}");
            }
        }
    }
}