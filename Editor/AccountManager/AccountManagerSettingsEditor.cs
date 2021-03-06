﻿namespace com.faith.core
{
    using System.IO;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;


    [CustomEditor(typeof(AccountManagerSettings))]
    public class AccountManagerSettingsEditor : BaseEditorClass
    {

        #region Private Variables
        private AccountManagerSettings  _reference;

        private SerializedProperty      _listOfCurrencyInfos;

        private static bool _flagedForGeneratingEnum    = false;
        private static bool _flagedForRegeneratingEnum  = false;
        private static string _currencyName = "DEFAULT";
        private static List<AccountManagerSettings.CurrecnyInfo> _listOfCurrencyToBeAdded;

        #endregion

        #region OnEditor

        public override void OnEnable()
        {
            base.OnEnable();

            _reference = (AccountManagerSettings)target;

            if (_reference == null)
                return;

            if (_reference.listOfCurrencyInfos == null)
            {
                _reference.listOfCurrencyInfos = new List<AccountManagerSettings.CurrecnyInfo>();
                _reference.listOfCurrencyInfos.Add(new AccountManagerSettings.CurrecnyInfo() { enumName = "DEFAULT" });
            }

            if (_listOfCurrencyToBeAdded == null)
                _listOfCurrencyToBeAdded = new List<AccountManagerSettings.CurrecnyInfo>();

            _listOfCurrencyInfos = serializedObject.FindProperty("listOfCurrencyInfos");
            if (_reference.listOfCurrencyInfos == null) {

                _reference.listOfCurrencyInfos = new List<AccountManagerSettings.CurrecnyInfo>();
                _listOfCurrencyInfos.serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {

            CoreEditorModule.ShowScriptReference(serializedObject);

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            {

                if (!EditorApplication.isCompiling) {

                    _currencyName = EditorGUILayout.TextField(_currencyName);
                    if (!_flagedForRegeneratingEnum && GUILayout.Button("+Add"))
                    {

                        AddNewCurrency(_currencyName);
                    }
                    if (_flagedForGeneratingEnum && GUILayout.Button("GenerateEnum", GUILayout.Width(125)))
                    {

                        _flagedForGeneratingEnum = false;
                        GenerateEnum();
                    }
                }

                
            }
            EditorGUILayout.EndHorizontal();

            //Section   :   Currency To Be Added
            if (_listOfCurrencyToBeAdded.Count > 0) {

                CoreEditorModule.DrawHorizontalLine();
                EditorGUILayout.LabelField("Currency to be Added :", EditorStyles.boldLabel);
                EditorGUI.indentLevel += 1;
                foreach (AccountManagerSettings.CurrecnyInfo currencyInfo in _listOfCurrencyToBeAdded) {

                    EditorGUILayout.LabelField(currencyInfo.enumName);
                }
                EditorGUI.indentLevel -= 1;
            }

            CoreEditorModule.DrawHorizontalLine();
            //Section   :   Added Currency

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("ListOfCurrencyInfo", EditorStyles.boldLabel);
                if (_flagedForRegeneratingEnum) {

                    if (!EditorApplication.isCompiling && GUILayout.Button("RegenerateEnum", GUILayout.Width(125f))) {

                        GenerateEnum();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel += 1;
            int numberOfCurrencyInfo = _reference.listOfCurrencyInfos.Count;
            for (int  i = 0; i < numberOfCurrencyInfo; i++) {

                EditorGUILayout.BeginHorizontal();
                {
                    _reference.listOfCurrencyInfos[i].showOnEditor = EditorGUILayout.Foldout(
                        _reference.listOfCurrencyInfos[i].showOnEditor,
                        _reference.listOfCurrencyInfos[i].enumName,
                        true
                    );

                    if (!_reference.listOfCurrencyInfos[i].showOnEditor) {

                        if (!EditorApplication.isCompiling && i > 0 && GUILayout.Button("Remove", GUILayout.Width(100f))) {
                            _reference.listOfCurrencyInfos.RemoveAt(i);
                            GenerateEnum();
                            return;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (_reference.listOfCurrencyInfos[i].showOnEditor) {

                    EditorGUI.indentLevel += 1;

                    EditorGUILayout.PropertyField(_listOfCurrencyInfos.GetArrayElementAtIndex(i).FindPropertyRelative("currencyName"));
                    EditorGUILayout.PropertyField(_listOfCurrencyInfos.GetArrayElementAtIndex(i).FindPropertyRelative("currencyIcon"));
                    EditorGUILayout.PropertyField(_listOfCurrencyInfos.GetArrayElementAtIndex(i).FindPropertyRelative("currencydefaultAmount"));
                    EditorGUILayout.PropertyField(_listOfCurrencyInfos.GetArrayElementAtIndex(i).FindPropertyRelative("currencyAnimationDuration"));
                    EditorGUILayout.PropertyField(_listOfCurrencyInfos.GetArrayElementAtIndex(i).FindPropertyRelative("animationCurve"));

                    EditorGUI.indentLevel -= 1;

                    EditorGUILayout.Space();
                }
            }
            EditorGUI.indentLevel -= 1;

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Configuretion

        private void AddNewCurrency(string newCurrencyName) {


            bool isUniqueCurrencyName = true;

            foreach (AccountManagerSettings.CurrecnyInfo currencyInfo in _reference.listOfCurrencyInfos)
            {
                if (StringOperation.IsSameString(currencyInfo.enumName, newCurrencyName))
                {
                    isUniqueCurrencyName = false;
                    break;
                }
            }

            foreach (AccountManagerSettings.CurrecnyInfo currencyInfo in _listOfCurrencyToBeAdded)
            {
                if (StringOperation.IsSameString(currencyInfo.enumName, newCurrencyName))
                {
                    isUniqueCurrencyName = false;
                    break;
                }
            }

            if (isUniqueCurrencyName) {

                AccountManagerSettings.CurrecnyInfo newCurrency = new AccountManagerSettings.CurrecnyInfo()
                {
                    enumName = newCurrencyName
                };

                _listOfCurrencyToBeAdded.Add(newCurrency);
                _flagedForGeneratingEnum = true;

                return;
            }

            Debug.LogError("The following currency name is already in used : " + newCurrencyName);
        }

        private void GenerateEnum() {

            int numberOfCurrencyAlreadyInList   = _reference.listOfCurrencyInfos.Count;
            int numberOfCurrencyToBeAdded       = _listOfCurrencyToBeAdded.Count;

            string path = "";
            if (_packageStatus == CoreEnums.CorePackageStatus.InDevelopment)
            {
                path += "Assets/com.faith.core/Runtime/AccountManager";
            }
            else {

                path += "Packages/com.faith.core/Runtime/AccountManager";
            }

            path += "/AccountManagerCurrencyEnum.cs";

            using (StreamWriter streamWriter = new StreamWriter(path)) {

                string scriptData = "namespace com.faith.core";
                scriptData += "\n{";
                scriptData += "\n\tpublic enum AccountManagerCurrencyEnum";
                scriptData += "\n\t{";
                //Starting

                for (int i = 0; i < numberOfCurrencyAlreadyInList; i++) {

                    scriptData += "\n\t\t" + _reference.listOfCurrencyInfos[i].enumName.ToUpper();

                    if (i == (numberOfCurrencyAlreadyInList - 1))
                    {
                        if(numberOfCurrencyToBeAdded > 0)
                            scriptData += ",";
                    }
                    else {

                        scriptData += ",";
                    }
                }

                for (int i = 0; i < numberOfCurrencyToBeAdded; i++) {

                    scriptData += "\n\t\t" + _listOfCurrencyToBeAdded[i].enumName.ToUpper();
                    if (i == 0 && numberOfCurrencyToBeAdded > 0 && numberOfCurrencyAlreadyInList > 0) {

                        scriptData += ",";
                    }
                    else if (i < (numberOfCurrencyToBeAdded - 1)){
                        
                            scriptData += ",";
                    }
                }


                //Ending
                scriptData += "\n\t}";
                scriptData += "\n}";

                streamWriter.Write(scriptData);

            }

            AssetDatabase.Refresh();

            for (int i = 0; i < numberOfCurrencyToBeAdded; i++) {

                AccountManagerSettings.CurrecnyInfo currencyInfo = _listOfCurrencyToBeAdded[0];
                _reference.listOfCurrencyInfos.Add(currencyInfo);
                _listOfCurrencyToBeAdded.RemoveAt(0);
               
            }
            _listOfCurrencyInfos.serializedObject.ApplyModifiedProperties();

        }

        #endregion

    }
}

