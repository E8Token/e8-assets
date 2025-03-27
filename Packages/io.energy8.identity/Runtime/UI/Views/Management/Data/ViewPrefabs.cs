using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Energy8.Identity.Views.Management.Data
{
    [CreateAssetMenu(menuName = "Identity/View Prefabs")]
    public class ViewPrefabs : ScriptableObject
    {
        [SerializeField] private List<GameObject> prefabs = new();

        public T GetPrefab<T>()
        {
            return prefabs.FirstOrDefault((p) => p.GetComponent<T>() != null).GetComponent<T>();
        }
    }
}