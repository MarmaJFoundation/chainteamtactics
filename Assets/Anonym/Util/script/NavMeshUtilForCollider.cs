using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Anonym.Util
{
    /// <summary>
    /// NavMeshUtilForCollider can convert data from collider to NavMeshBuildSource and Build NavMesh at runtime.
    /// But Unity Default has not support full NavMesh features. If you need some high level features, check below:
    /// https://docs.unity3d.com/Manual/NavMesh-BuildingComponents.html
    /// 
    /// and this almost from https://forum.unity.com/threads/navmesh-baking-at-runtime-example.507446/
    /// </summary>

    public delegate List<NavMeshBuildSource> NavMeshBuildSources();

    public class NavMeshUtilForCollider : Singleton<NavMeshUtilForCollider>
    {
        [SerializeField]
        private Transform centerTransform = null;

        [SerializeField]
        private Vector3 vCentor = Vector3.zero;

        [SerializeField]
        private Vector3 vSize = Vector3.one * 10;

        [SerializeField]
        private LayerMask targetLayerMask = -1;

        [SerializeField]
        private int iSettingID = 0;

        //[SerializeField]
        //private int defaultLayer = 0;

        [SerializeField]
        private NavMeshData navData = null;

        [SerializeField]
        private NavMeshDataInstance dataInstance;

        [SerializeField]
        public List<NavMeshBuildMarkup> buildMarkups = new List<NavMeshBuildMarkup>();

        [SerializeField]
        private List<NavMeshBuildSource> baseBuildSources = new List<NavMeshBuildSource>();

        public NavMeshBuildSources ExternSources = null;

        public bool bBaseBuildSourcesCorrupted = true;
        public bool bExternBuildSourcesCorrupted = true;

        private Coroutine coroutine;

        public static void TryToBuildORUpdate(bool ExternSourceUpdate = false, Transform exceptionalExtern = null)
        {
            if (!IsNull)
            {
                if (ExternSourceUpdate)
                    instance.bExternBuildSourcesCorrupted = true;

                instance.BuildORUpdate(exceptionalExtern);
            }
        }

        void BuildORUpdate(Transform exceptionalExtern = null)
        {
            if (navData == null)
                ReBuildAll(exceptionalExtern);
            else if (coroutine == null)
                UpdateNavmeshData(exceptionalExtern);
        }

        public void ReBuildAll(Transform exceptionalExtern = null)
        {
            navData = BuildNavMeshData(GetBuildSources(exceptionalExtern));
            AddNavMeshData();
        }

        public NavMeshData BuildNavMeshData(List<NavMeshBuildSource> buildSource)
        {
            return NavMeshBuilder.BuildNavMeshData(GetSettings(), buildSource, GetBounds(), Vector3.zero, Quaternion.identity);
        }

        public NavMeshData BuildNavMeshData(List<NavMeshBuildSource> buildSource, Vector3 position, float fSightRange)
        {
            return NavMeshBuilder.BuildNavMeshData(GetSettings(), buildSource,
                new Bounds(position, Vector3.one * fSightRange), Vector3.zero, Quaternion.identity);
        }

        public NavMeshDataInstance AddNavMeshData(NavMeshData newData)
        {
            return NavMesh.AddNavMeshData(newData);
        }

        public void AddNavMeshData()
        {
            if (navData != null)
            {
                RemoveInstanceData();
                dataInstance = AddNavMeshData(navData);
            }
        }

        public void UpdateNavmeshData(Transform exceptionalExtern = null)
        {
            coroutine = StartCoroutine(UpdateNavmeshDataAsync(exceptionalExtern));
        }

        private void Start()
        {
            bBaseBuildSourcesCorrupted = true;
        }

        public void RemoveInstanceData()
        {
            if (dataInstance.valid)
            {
                dataInstance.Remove();
            }
        }

        private void OnDestroy()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            RemoveInstanceData();
        }
        
        private IEnumerator UpdateNavmeshDataAsync(Transform exceptionalExtern = null)
        {
            AsyncOperation op = NavMeshBuilder.UpdateNavMeshDataAsync(navData, GetSettings(), GetBuildSources(exceptionalExtern), GetBounds());
            yield return op;

            AddNavMeshData();
            Debug.Log("Navmesh update complete");
        }        

        private List<NavMeshBuildSource> GetBuildSources(Transform exceptionalExtern = null)
        {
            List<NavMeshBuildSource> result = new List<NavMeshBuildSource>();

            if (bBaseBuildSourcesCorrupted)
            {
                baseBuildSources.Clear();
                NavMeshBuilder.CollectSources(GetBounds(), targetLayerMask, NavMeshCollectGeometry.PhysicsColliders, 0, buildMarkups, baseBuildSources);
                bBaseBuildSourcesCorrupted = false;
                baseBuildSources = baseBuildSources.
                    Where(s => (targetLayerMask & (1 << s.component.gameObject.layer)) != 0).
                    Where(s => !(s.component as Collider).isTrigger).
                    Where(s => s.component. gameObject.activeInHierarchy).ToList();
                bBaseBuildSourcesCorrupted = false;
            }

            result.AddRange(baseBuildSources);


            if (ExternSources != null && (exceptionalExtern != null || bExternBuildSourcesCorrupted))
            {
                result.AddRange(ExternSources().Where(e => exceptionalExtern == null || (e.component != null && !e.component.transform.IsChildOf(exceptionalExtern))));
                bExternBuildSourcesCorrupted = false;
            }

            return result;
        }

        private NavMeshBuildSettings GetSettings()
        {
            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(iSettingID);
            return settings;
        }

        private Bounds GetBounds()
        {
            if (centerTransform != null)
                return new Bounds(centerTransform.position, vSize);

            return new Bounds(vCentor, vSize);
        }

#if UNITY_EDITOR
        [SerializeField]
        Color sourceFieldColor = new Color(0, 0, 0, 0.075f);

        [SerializeField]
        bool bShadowOnSourceObject = false;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(vCentor, vSize);

            if (bShadowOnSourceObject)
            {
                Gizmos.color = sourceFieldColor;
                foreach (var one in baseBuildSources)
                {
                    var b = (one.component as Collider).bounds;
                    Gizmos.DrawCube(b.center, b.size);
                }
            }
        }
#endif
    }
}
