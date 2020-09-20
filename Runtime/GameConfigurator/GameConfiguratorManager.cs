﻿namespace com.faith.core
{
    using UnityEngine;

    public class GameConfiguratorManager : MonoBehaviour
    {

        #region Public Varaibles

#if UNITY_EDITOR

        public bool isGameConfiguratorAssetVisible;


#endif

        public static GameConfiguratorManager Instance;

        public CoreEnums.InstanceBehaviour  instanceBehaviour;
        public GameConfiguratorAsset        gameConfiguratorAsset;

        #endregion

        #region Mono Behaviour

        private void Awake()
        {
            Initialization();
        }

        #endregion

        #region Configuretion

        private void Initialization() {

            switch (instanceBehaviour)
            {

                case CoreEnums.InstanceBehaviour.UseAsReference:

                    break;
                case CoreEnums.InstanceBehaviour.CashedAsInstance:

                    Instance = this;

                    break;
                case CoreEnums.InstanceBehaviour.Singleton:

                    if (Instance == null)
                    {
                        Instance = this;
                        DontDestroyOnLoad(gameObject);
                    }
                    else
                    {

                        Destroy(gameObject);
                    }

                    break;
            }

            if (gameConfiguratorAsset == null)
            {
                CoreDebugger.Debug.LogError("'gameConfiguretorAsset' has not been assigned!");
                return;
            }

            CoreDebugger.logType = gameConfiguratorAsset.logType;
        }

        #endregion
    }
}

