using UnityEngine;
using TMPro; // TextMeshPro (UI用)
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

// 1. データ構造の定義（Firestoreの中身と合わせる）
public class StepData
{
    public string instruction;
    public string spiceID;
    public string video;
}

public class RecipeViewer : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("レシピの工程を表示するCanvas上のテキスト")]
    public TextMeshProUGUI instructionText; // 🚨 Canvas用は 'UGUI' がつきます
    
    [Tooltip("現在のステップ数 (例: 1/5)")]
    public TextMeshProUGUI counterText;

    [Header("Database Settings")]
    [Tooltip("取得したいレシピのドキュメントID (例: omlet_cheese)")]
    public string targetRecipeID = "tz5vBFXPEGdxJaAvZPYG";

    // 内部データ
    private List<StepData> steps = new List<StepData>();
    private int currentIndex = 0;
    private FirebaseFirestore db;

    void Start()
    {
        instructionText.text = "Firebase初期化中...";

        // Firebaseの依存関係をチェック
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // 初期化成功！ここで初めてデータベースに接続
                db = FirebaseFirestore.DefaultInstance;
                instructionText.text = "レシピを読み込み中...";
                LoadRecipeFromFirestore();
            }
            else
            {
                Debug.LogError($"Firebaseの初期化に失敗: {dependencyStatus}");
                instructionText.text = "初期化エラー";
            }
        });
    }

    // ---------------------------------------------------------
    // 2. Firestoreからデータを取得する処理
    // ---------------------------------------------------------
    private void LoadRecipeFromFirestore()
    {
        db.Collection("recipes").Document(targetRecipeID).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                instructionText.text = "読み込みエラー";
                Debug.LogError(task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                // Firestoreの "steps" 配列を取得
                Dictionary<string, object> data = snapshot.ToDictionary();
                
                if (data.ContainsKey("steps"))
                {
                    List<object> stepList = data["steps"] as List<object>;
                    ParseSteps(stepList); // データをC#リストに変換
                    
                    // 最初のステップを表示
                    currentIndex = 0;
                    UpdateDisplay();
                }
            }
            else
            {
                instructionText.text = "レシピが見つかりません";
            }
        });
    }

    // Firestoreのデータを使いやすい形に変換する
    private void ParseSteps(List<object> stepList)
    {
        steps.Clear();
        foreach (var item in stepList)
        {
            // 各ステップは Map (Dictionary) として保存されている
            var map = item as Dictionary<string, object>;
            
            StepData newStep = new StepData();
            newStep.instruction = map.ContainsKey("instruction") ? map["instruction"].ToString() : "";
            newStep.spiceID = map.ContainsKey("spiceID") ? map["spiceID"].ToString() : "";
            newStep.video = map.ContainsKey("video") ? map["video"].ToString() : "";
            
            steps.Add(newStep);
        }
    }

    // ---------------------------------------------------------
    // 3. ボタン操作と表示更新
    // ---------------------------------------------------------

    // 「次へ」ボタンから呼ぶ
    public void NextStep()
    {
        if (steps.Count == 0) return;

        if (currentIndex < steps.Count - 1)
        {
            currentIndex++;
            UpdateDisplay();
        }
    }

    // 「前へ」ボタンから呼ぶ
    public void PreviousStep()
    {
        if (steps.Count == 0) return;

        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateDisplay();
        }
    }

    // 画面のテキストを更新する
    private void UpdateDisplay()
    {
        StepData currentStep = steps[currentIndex];

        // テキストの更新
        instructionText.text = currentStep.instruction;
        
        // カウンターの更新 (例: 1 / 5)
        if (counterText != null)
        {
            counterText.text = $"{currentIndex + 1} / {steps.Count}";
        }

        // 🚨 ここに将来的に「ハイライト機能」や「動画再生」を追加します
        // if (!string.IsNullOrEmpty(currentStep.SpiceID)) { ... }
        
        Debug.Log($"ステップ {currentIndex + 1}: {currentStep.instruction}");
    }
}