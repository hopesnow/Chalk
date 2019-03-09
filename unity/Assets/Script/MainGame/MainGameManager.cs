﻿using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController[] players;

    [SerializeField] private Transform stageParent;
    [SerializeField] private GameObject[] stages;

    [SerializeField] private Text textOrigin;
    [SerializeField] private Image goalImage;
    [SerializeField] private Image goalMark;

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    private void Start()
    {
        PlayerController[] test = FindObjectsOfType<PlayerController>();
        this.players = test;

        this.textOrigin.gameObject.SetActive(false);
        this.mainCamera.orthographicSize = 3.6f;

        foreach (var player in this.players)
        {
            // ゴール判定オブザーバーの登録
            player.IsGoal.ObserveEveryValueChanged(x => x.Value).Subscribe(goal =>
            {
                if (goal)
                {
                    Log(string.Format("Player{0} Goal.", player.PlayerNo + 1));
                }
            });
        }

        // ステージを設定
        SetStage(0);

        Log("GameStart... !");
        Log("ESCキーでリセット");
        Log("ELECOM DIRECT INPUT ONLY NOW.");
    }

    /** ********************************************************************************
     * @summary 更新処理
     ***********************************************************************************/
    private void Update()
    {
        if (Input.GetButtonDown("DebugReset"))
        {
            foreach (var player in this.players)
            {
                player.Reset();
            }

            // ステージをランダムで設定
            this.SetStage();

            Log("GameReset... !");
        }

        if (Input.GetButtonDown("Pause"))
        {
            Debug.Log("Pause");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CloseUp(new Vector3(5.22f, 1.336f, 0f));
        }
        if (Input.GetKeyDown(KeyCode.LeftCommand))
        {
            ResetMainCamera();
        }
    }

    /** ********************************************************************************
     * @summary ログ出力処理
     ***********************************************************************************/
    public void Log(string str)
    {
        Debug.LogFormat("{0}", str);

        this.textOrigin.gameObject.SetActive(true);

        var log = Instantiate(this.textOrigin, this.textOrigin.transform.parent);
        log.transform.SetAsFirstSibling();
        log.text = str;

        this.textOrigin.gameObject.SetActive(false);
    }

    /** ********************************************************************************
     * @summary ステージ選択処理(number: 0から)範囲外だとランダムになる
     ***********************************************************************************/
    public void SetStage(int number = -1)
    {
        // 前回ステージを削除
        foreach (Transform stageChild in this.stageParent)
        {
            Destroy(stageChild.gameObject);
        }

        // 範囲外チェック
        if (number < 0 || number > this.stages.Length - 1)
        {
            // ランダムに選ぶ
            number = Random.Range(0, this.stages.Length);   // stages.Lengthは含まない(intだから)
        }

        // ステージ生成
        Instantiate(this.stages[number], this.stageParent);
    }

    public void CloseUp(Vector3 goalPosition)
    {
        Sequence sequence = DOTween.Sequence();
        float movetime = 0.750f;
        goalPosition.z = mainCamera.transform.position.z;
        goalImage.rectTransform.localScale = new Vector3(3f, 3f, 1f);

        //ゴールイメージ表示
        sequence.Append(goalImage.rectTransform.DOScale(new Vector3(1f, 1f, 1f), movetime).SetEase(Ease.InSine))
        .Join(DOTween.ToAlpha(
            () => goalImage.color,
            (alpha) => goalImage.color = alpha,
            1.0f,
            movetime
        ).SetEase(Ease.InSine))
            .AppendInterval(1f)//1秒待機
        .Append(DOTween.ToAlpha(//Image非表示
            () => goalImage.color,
            (alpha) => goalImage.color = alpha,
            0f,
            0f
        ))
            .Join(goalImage.rectTransform.DOScale(new Vector3(3f, 3f, 1f), 0f));
        //ズームイン
        sequence.Append( mainCamera.transform.DOMove(goalPosition, movetime).SetEase(Ease.InSine))
        .Join(DOTween.To(
            () => mainCamera.orthographicSize,
            (size) => mainCamera.orthographicSize = size,
            1.8f,
            movetime
        ).SetEase(Ease.InSine))
        .AppendInterval(1f)
        .Append(goalMark.rectTransform.DOPunchPosition(new Vector3(5f, -5f, 0f), movetime))
        .Join(DOTween.ToAlpha(
            () => goalMark.color,
            (alpha) => goalMark.color = alpha,
            1.0f,
            0f
        ))
        .AppendInterval(1f)
        .Join(DOTween.ToAlpha(
            () => goalMark.color,
            (alpha) => goalMark.color = alpha,
            0f,
            0f
        ))
        .OnComplete(() => {//Reset
            foreach (var player in this.players)
            {
                player.Reset();
            }
            ResetMainCamera();
            Log("GameReset... !");
        })
        .Play();
    }

    public void ResetMainCamera()
    {
        mainCamera.transform.position = new Vector3(0, 0, mainCamera.transform.position.z);
        mainCamera.orthographicSize = 3.6f;
    }
}
