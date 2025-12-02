using UnityEngine;
using UnityEngine.Video; // VideoPlayerを使うために必要

public class VideoDemo : MonoBehaviour
{
    // InspectorでVideo Playerを割り当てる
    public VideoPlayer videoPlayer; 

    // Inspectorで作成した直接ダウンロードURLを入力
    public string googleDriveVideoUrl = "https://drive.google.com/uc?export=download&id=1o1Z-SN7WLTu972TRFqbXc0JQWcxRHEMZ";
    
    // 再生ボタンのOnClick()に割り当てるメソッド
    public void StartGoogleDrivePlayback()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("Video Playerが割り当てられていません。");
            return;
        }

        videoPlayer.source = VideoSource.Url; // ソースをURLに設定
        videoPlayer.url = googleDriveVideoUrl; // ステップ1で作成したURLを設定
        
        // 🚨 動画の準備（バッファリング）を開始
        videoPlayer.Prepare(); 
        
        // 準備完了後に自動再生されるように設定
        videoPlayer.prepareCompleted += OnVideoPrepared;
        Debug.Log("Google Drive動画のロードを開始しました...");
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        Debug.Log("動画の再生を開始しました。");
    }
}