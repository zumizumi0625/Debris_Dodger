using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class TitleController : MonoBehaviour
{
    // メインゲームのシーン名を指定（既存のシーン名に合わせてください）
    // デフォルトでは "SampleScene" や "GameScene" など
    [SerializeField] private string gameSceneName = "SampleScene";

    void Update()
    {
        // スペースキーが押されたら
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ゲームシーンをロードする
            SceneManager.LoadScene(gameSceneName);
        }
    }
}