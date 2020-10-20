﻿namespace com.faith.core
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;
    using System.Threading.Tasks;

    public class SceneTransitionController
    {
        #region Private Variables

        private const float LOAD_READY_PERCENTAGE = 0.9f;

        private static bool _isSceneLoadOperationRunning = false;

        #endregion

        #region Configuretion

        private static async void ControllerForLoadingScene(string sceneName, UnityAction<float> OnUpdatingProgression, UnityAction OnSceneLoaded, float animationSpeedForLoadingBar = 1, float initalDelayToInvokeOnSceneLoaded = 0)
        {
            float animatedLerpValue = 0f;

            AsyncOperation asyncOperationForLoadingScene = SceneManager.LoadSceneAsync(sceneName);
            asyncOperationForLoadingScene.allowSceneActivation = false;
            while (/*!asyncOperationForLoadingScene.isDone && */ animatedLerpValue <= 0.99f)
            {
                animatedLerpValue = Mathf.Lerp(
                    animatedLerpValue,
                    Mathf.Clamp(asyncOperationForLoadingScene.progress, 0, 0.9f) / LOAD_READY_PERCENTAGE,
                    animationSpeedForLoadingBar);

                CoreDebugger.Debug.LogWarning("AnimatedLerpValue : " + asyncOperationForLoadingScene.progress  + ", " + animatedLerpValue);
                OnUpdatingProgression?.Invoke(animatedLerpValue);

                await Task.Delay(1);
            }

            OnUpdatingProgression?.Invoke(1);

            while (!asyncOperationForLoadingScene.isDone) {

                await Task.Delay(33);
            }

            asyncOperationForLoadingScene.allowSceneActivation = true;

            await Task.Delay(33);

            await Task.Delay((int)(initalDelayToInvokeOnSceneLoaded * 1000));

            OnSceneLoaded?.Invoke();

            _isSceneLoadOperationRunning = false;

        }

        #endregion

        #region Public Callback

        public static void LoadScene(
            string sceneName,
            UnityAction<float> OnUpdatingProgression = null,
            UnityAction OnSceneLoaded = null,
            float animationSpeedForLoadingBar = 1,
            float initalDelayToInvokeOnSceneLoaded = 0)
        {
            if (!_isSceneLoadOperationRunning)
            {
                _isSceneLoadOperationRunning = true;
                ControllerForLoadingScene(sceneName, OnUpdatingProgression, OnSceneLoaded, animationSpeedForLoadingBar, initalDelayToInvokeOnSceneLoaded);
            }
            else {

                CoreDebugger.Debug.LogError("Scene transition already running. Failed to take the request on new loaded scene : " + sceneName);
            }
        }

        #endregion
    }
}
