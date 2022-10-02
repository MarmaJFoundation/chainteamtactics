using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

namespace Anonym.Util
{
    using Isometric;

    public class SimplePathfinder : MonoBehaviour
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        static readonly float fTolerance = 0.01f;

        [SerializeField]
        bool _isActive = false;
        public bool isActive {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
            }
        }

        [HideInInspector]
        public bool bIgnoreYAxis = true;

        [SerializeField]
        Transform followTransform = null;
        [SerializeField]
        float fKeepRangeFromTarget = 1f;
        bool bClearFollowTransform = false;

        [SerializeField]
        GameObject AnchorInstance = null;
        [SerializeField]
        public GameObject AnchorPrefab = null;
        void CreateAnchorInstance()
        {
            ClearAnchorInstance();
            if (AnchorPrefab)
            {
                if (followTransform != null)
                {
                    AnchorInstance = Instantiate(AnchorPrefab, followTransform);
                    var colliders = followTransform.GetComponentsInChildren<Collider>().Where(c => !c.isTrigger).GetEnumerator();
                    if (colliders != null && colliders.MoveNext())
                    {
                        var collider = colliders.Current as Collider;
                        Bounds bounds = collider.bounds;
                        while (colliders.MoveNext())
                        {
                            bounds.Encapsulate((colliders.Current as Collider).bounds);
                        }
                        AnchorInstance.transform.localPosition += Vector3.up * bounds.extents.y;
                    }
                }
                else
                    AnchorInstance = Instantiate(AnchorPrefab, LastPosition() + AnchorPrefab.transform.position, AnchorPrefab.transform.rotation);
            }
        }
        void ClearAnchorInstance()
        {
            if (AnchorInstance != null)
            {
                Destroy(AnchorInstance);
                AnchorInstance = null;
            }
        }

        public bool bKeepFollowing { get { return followTransform != null && !bClearFollowTransform; } }

        [SerializeField]
        public Queue<Vector3> vRoutes = new Queue<Vector3>();

        [SerializeField]
        protected bool bUseRefresh = true;

        [SerializeField]
        public bool bHoldRefresh = false;

        [SerializeField]
        protected float fRefreshInterval = 1f;
        protected float fLastRefreshTime = 0f;

        private float fRemainLength(Vector3 nowPosition)
        {
            if (vRoutes.Count == 0)
                return float.MaxValue;

            float fLength = 0;
            var enumerator = vRoutes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                fLength += Vector3.Distance(enumerator.Current, nowPosition);
                nowPosition = enumerator.Current;
            }
            return fLength;
        }

        protected void UpdateLastPathTime()
        {
            fLastRefreshTime = Time.time;
        }
        protected bool isTimeToUpdate()
        {
            return !bHoldRefresh && Time.time >= fLastRefreshTime + fRefreshInterval;
        }
        protected bool isInKeepRange(Vector3 nowPosition)
        {
            return followTransform != null && DistatanceToTarget(nowPosition) <= fKeepRangeFromTarget;
        }

        public bool hasPath { get { return isActive && vRoutes.Count > 0 || followTransform != null; } }

        public Vector3 GetTransition(Vector3 nowPosition, float fSpeed)
        {
            var vHorizontal = NextPosition(nowPosition, fSpeed) - nowPosition;
            if (bIgnoreYAxis)
                vHorizontal.y = 0;
            return vHorizontal;
        }

        public Vector3 NextPosition(Vector3 nowPosition, float fSpeed)
        {
            if (hasPath)
            {
                if (isInKeepRange(nowPosition))
                    return nowPosition;

                while (vRoutes.Count > 0 && fSpeed > 0)
                {
                    nowPosition = nextPosFromPath(nowPosition, ref fSpeed);
                }
            }

            return nowPosition;
        }

        public Vector3 LastPosition()
        {
            if (vRoutes.Count > 0)
                return vRoutes.Last();
            else if (followTransform != null)
                return followTransform.position;

            return Vector3.zero;
        }

        public void Reset()
        {
            ClearAnchorInstance();
            vRoutes.Clear();
            bHoldRefresh = false;
            if (!bKeepFollowing)
            {
                isActive = false;
                followTransform = null;
            }
        }

        float DistatanceToTarget(Vector3 nowPosition)
        {
            var next = followTransform.position;
            var vTo = next - nowPosition;

            if (bIgnoreYAxis)
                vTo.y = 0;

            return vTo.magnitude;
        }
        Vector3 nextPosFromPath(Vector3 nowPosition, ref float fSpeed)
        {
            var next = vRoutes.Peek();
            var vGap = next - nowPosition;

            if (bIgnoreYAxis)
                vGap.y = 0;

            next = nowPosition + vGap;

            float fDistance = vGap.magnitude;
            if (fSpeed > fDistance)
            {
                fSpeed -= fDistance;
                if (fDistance <= fTolerance)
                {
                    vRoutes.Dequeue();
                    if (hasPath)
                        next = nextPosFromPath(next, ref fSpeed);
                }
            }
            else
            {
                next = Vector3.MoveTowards(nowPosition, next, fSpeed);
                fSpeed = 0;
            }

            return next;
        }

        void addPath(params Vector3[] vPos)
        {
            for (int i = 0; i < vPos.Length; ++i)
                vRoutes.Enqueue(vPos[i]);

            UpdateLastPathTime();
            isActive = true;
        }
        public void SetNewPath(params Vector3[] vPos)
        {
            Reset();
            addPath(vPos);
            CreateAnchorInstance();
        }
        public void Follow(Transform _target)
        {
            Reset();
            followTransform = _target;
            CreateAnchorInstance();
        }

        public void ReBuildAll()
        {
            if (!NavMeshUtilForCollider.IsNull)
                NavMeshUtilForCollider.instance.ReBuildAll(transform);
        }
        public bool TryToRefresh(Vector3 nowPosition, bool bDropOldPath = true)
        {
            Vector3[] _vArray;

            if (hasPath && isTimeToUpdate() && !isInKeepRange(nowPosition))
            {
                ReBuildAll();
                ClosestPoisitionOnNavMesh(nowPosition, ref nowPosition, 1f);

                float fPathLenght = fRemainLength(nowPosition);
                if (CalcPathWithNavMesh(transform, nowPosition, followTransform ? followTransform.position : vRoutes.Last(), out _vArray, ref fPathLenght))
                    SetNewPath(_vArray);
                else if (bDropOldPath)
                    vRoutes.Clear();

                return true;
            }

            return false;
        }
        public bool ClickToMoveWithNavMesh(Vector3 nowPosition)
        {
            Vector3[] _vArray = null;
            Vector3 _dest = Vector3.zero;
            const float fMinDistance = 0.125f;
            const float fMaxPriorityDistance = 1f;
            float fPathLenght;
            bool bFound = false;
            gameObject.TryGetComponent<IsometricMovement>(out var isoMovement);

            ReBuildAll();
            ClosestPoisitionOnNavMesh(nowPosition, ref nowPosition, 1f);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1000, -1, QueryTriggerInteraction.Ignore);

            var enumerator = hits.
                // OrderBy(hit => hit.distance).
                Select(hit => {
                    NavMesh.SamplePosition(hit.point, out var navHit, 0.2f, NavMesh.AllAreas);
                    return navHit;
                }).
                Where(nH => nH.hit).
                Select(nH => isoMovement != null ? isoMovement.GetXZPositionWithSnapOption(nH.position) : nH.position).
                GetEnumerator();

            fPathLenght = fRemainLength(nowPosition);

            while (enumerator.MoveNext())
            {
                if (CalcPathWithNavMesh(transform, nowPosition, (Vector3)enumerator.Current, out _vArray, ref fPathLenght))
                {
                    bool bFoundMaxPriorityWay = fPathLenght < fMaxPriorityDistance;
                    if (bFoundMaxPriorityWay || (!bFound && fPathLenght > fMinDistance))
                    {
                        SetNewPath(_vArray);
                        followTransform = null;
                        bFound = true;

                        if (bFoundMaxPriorityWay)
                            break;
                    }
                }
            }

            if (bFound)
                CreateAnchorInstance();

            return bFound;
        }

        static public bool ClosestPoisitionOnNavMesh(Vector3 nowPosition, ref Vector3 closestPosition, float fRange)
        {
            NavMeshHit nHit;
            closestPosition = nowPosition;

            if (NavMesh.SamplePosition(nowPosition, out nHit, fRange, NavMesh.AllAreas))
            {
                if (nHit.distance > fTolerance)
                    closestPosition = nHit.position;
                return true;
            }

            return false;
        }
        static public bool CalcPathWithNavMesh(Transform transform, Vector3 nowPosition, Vector3 destPosition, out Vector3[] vOut, ref float fTotalDistance)
        {
            bool bFound = false;
            //NavMeshHit nHit;

            fTotalDistance = float.MaxValue;
            vOut = null;
            
            if (ClosestPoisitionOnNavMesh(destPosition, ref destPosition, 0.125f))
            {
                float _fTotalDistance = 0;
                float _fMinDiffForChange = 2f;
                NavMeshPath _path = new NavMeshPath();

                if (NavMesh.CalculatePath(nowPosition, destPosition, NavMesh.AllAreas, _path))
                {
                    Vector3 vLastPos = nowPosition;
                    for (int i = 0; i < _path.corners.Length; ++i)
                    {
                        vLastPos.y = _path.corners[i].y;
                        _fTotalDistance += Vector3.Distance(vLastPos, _path.corners[i]);
                        vLastPos = _path.corners[i];
                    }
                }

                if (_path.corners.Count() > 0 && _fTotalDistance < fTotalDistance - _fMinDiffForChange)
                {
                    bFound = true;
                    vOut = _path.corners;
                    fTotalDistance = _fTotalDistance;
                }
            }

            return bFound;
        }

        //private void LateUpdate()
        //{
        //    if (bUseAutoRefresh)
        //        TryToRefresh(transform.position);
        //}

        static public bool JumpCheck(Transform characterTransform, Vector3 vFeet, Vector3 vHorizontal, float fSetpOffset, Vector3 vCapsuleCenter, float vCapsuleHeight, float vCapsuleRadius)
        {
            // 점프 판단을 점프 관련 데이터를 기반으로 스마트하게 결정할 수 없을까? 안되면 관련 값을 노출 시켜서 직관적으로 쉽게 수정할 수 있게 하자. 
            // 현재 너무 하드 코딩: 전방 0.75 체크, 전상*0.1 에서 아래로 0.5 체크. FrontObstacleCheckRange, 
            bool bJump = false;
            float fBlankCheck = 0.5f;
            float fBlankCheckOffset = 0.1f;
            float fVOffset = vCapsuleHeight * 0.5f - vCapsuleRadius;
            Vector3 vCenter = Vector3.up * fSetpOffset + vCapsuleCenter;

            var allhits = Physics.CapsuleCastAll(vCenter + fVOffset * Vector3.down, vCenter + fVOffset * Vector3.up, vCapsuleRadius, vHorizontal, Mathf.Min(vHorizontal.magnitude, 0.75f), -1, QueryTriggerInteraction.Ignore);
            var hits = from hit in allhits where !hit.collider.transform.IsChildOf(characterTransform) select hit;

            if (!(bJump = hits.Count() > 0))
            {
                bJump = Physics.RaycastAll(new Ray(vFeet + (vHorizontal.normalized + Vector3.up) * fBlankCheckOffset, Physics.gravity), fBlankCheck, -1, QueryTriggerInteraction.Ignore).All(e => e.collider.transform.IsChildOf(characterTransform));
            }
            // 캐릭터 판별은 레이어나 다른 방법으로 변경가능
            else if (hits.Any(e => {
                var isometricMovement = e.collider.gameObject.GetComponentInParent<Isometric.IsometricMovement>();
                return isometricMovement != null;
            }))
            {
                bJump = false;
            }
            return bJump;
        }
        #region Editor
#if UNITY_EDITOR
        [Header("For UnityEditor")]
        [SerializeField]
        Vector3[] ReadOnly_NextPositions;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            ReadOnly_NextPositions = vRoutes.ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }

        private void OnDrawGizmos()
        {
            Color baseColor = Color.gray;
            Color brightColor = Color.yellow;

            if (hasPath)
            {
                Vector3 lastPos = transform.position;
                if (this.TryGetComponent<Isometric.IsometricMovement>(out var Character))
                {
                    lastPos = Character.vFeetPosition;
                }

                Vector3 vLayer = Vector3.down * 0.01f;
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(lastPos, 0.1f);
                for (int i = 0; i < vRoutes.Count; ++i)
                {
                    Vector3 nowPos = vRoutes.ElementAt(i);
                    Gizmos.color = baseColor;
                    Gizmos.DrawSphere(nowPos, 0.14f);
                    Gizmos.DrawLine(lastPos + vLayer, nowPos + vLayer);

                    Gizmos.color = brightColor;
                    Gizmos.DrawWireSphere(nowPos, 0.15f);
                    Gizmos.DrawLine(lastPos, nowPos);
                    lastPos = nowPos;
                }
            }
        }
#endif
        #endregion Editor
    }
}
