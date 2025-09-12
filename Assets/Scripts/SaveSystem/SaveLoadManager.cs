using System.Collections.Generic;
using Assets.Scripts.Misc;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.SaveSystem
{
    public class SaveLoadManager : SingletonMonoBehaviour<SaveLoadManager>
    {
        public List<ISaveble> iSaveableObjectList;

        protected override void Awake()
        {
            base.Awake();

            iSaveableObjectList = new List<ISaveble>();
        }

        public void StoreCurrentSceneData()
        {
            foreach (ISaveble iSaveableObject in iSaveableObjectList)
            {
                iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
            }
        }

        public void RestoreCurrentSceneData()
        {
            foreach (ISaveble iSaveableObject in iSaveableObjectList)
            {
                iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}