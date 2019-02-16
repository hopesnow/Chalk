namespace UniRx.Triggers
{
    using System;
    using System.Linq;
    using UnityEngine;

    static public class AnimatorExtension
    {
        /** ********************************************************************************
         * @summary Animatorの進捗をObserverとして返す
         ***********************************************************************************/
        static public IObservable<AnimatorStateInfo> PlayAsObservable(this Animator animator, string stateName, int layer = 0)
        {
            animator.Play(stateName, layer);
            return animator.EndAsObservable(stateName, layer);
        }

        /** ********************************************************************************
         * @summary アニメが完了するまで待機する
         ***********************************************************************************/
        static public IObservable<AnimatorStateInfo> EndAsObservable(this Animator animator, string stateName = default(string), int layer = 0)
        {
            IObservable<AnimatorStateInfo> observer;

            // もしステート名が指定されていなかった場合
            if (string.IsNullOrEmpty(stateName))
            {
                observer = animator.UpdateAsObservable()
                    // 初期化まで待機
                    .SkipWhile(_ => !animator.isInitialized)
                    .TakeWhile(_ => animator.isActiveAndEnabled)
                    .Select(_ => animator.GetCurrentAnimatorStateInfo(layer))
                    .Where(info => 1f <= info.normalizedTime)
                    .Take(1);
            }
            else
            {
                // もしステート名が指定されていた場合
                observer = animator.UpdateAsObservable()
                    // 初期化まで待機
                    .SkipWhile(_ => !animator.isInitialized)
                    // アニメの指定名がある場合は、指定アニメのステートになるまで待機する
                    .SkipWhile(_ => !animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName))
                    .TakeWhile(_ => animator.isActiveAndEnabled)
                    .Select(_ => animator.GetCurrentAnimatorStateInfo(layer))
                    .Where(info => (info.IsName(stateName) && 1f <= info.normalizedTime) || !info.IsName(stateName))
                    .Take(1);
            }

            return observer;
        }
    }
}