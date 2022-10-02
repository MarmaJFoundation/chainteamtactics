using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Anonym.Isometric
{
    using Util;

    [RequireComponent(typeof(NavMeshAgent))]
    public class IsometricNavMeshAgent : IsometricMovement
    {
        public static string defaultIsoMapPrefabPat = "Assets/Anonym/MapEditor/Scene/Tutorials/7.1 CharacterControllers/Iso NavMeshAgent Sample.prefab";

        [SerializeField]
        bool IsCustomTravalOffMesh = true;
        public bool isOnCustomOffMeshAction { get { return OffMeshAction != null; } }
        Coroutine OffMeshAction;

        override public bool isOnGround { get { return NMAgent.isOnNavMesh; } }
        bool isRemainingDistanceForNMA { get { return NMAgent.remainingDistance > fMinMovement; } }
        bool isControllable
        {
            get
            {
                return bSnapToGroundGrid || OffMeshAction == null;
            }
        }

        override protected void jumpReset()
        {
            base.jumpReset();
            NMAgent.baseOffset = _baseOffset;
        }

        [SerializeField]
        public NavMeshAgent NMAgent;

        float _baseOffset;

        #region Events and Delegates
        public delegate void DestinationArrival(IsometricNavMeshAgent isometricNavMeshAgent, GameObject destination);

        public event DestinationArrival OnDestinationArrival;
        #endregion

        public virtual void SetDestinationGameObject(GameObject destinationObject)
        {
            destination = destinationObject;
            bOnMoving = true;
            NMAgent.isStopped = bSnapToGroundGrid;
            NMAgent.destination = destination.transform.position;
        }
        
        void ResetPath()
        {
            if (NMAgent.hasPath)
                NMAgent.ResetPath();

            if (!KeyInputAssist.IsNull)
                KeyInputAssist.instance.ClearAnchor();
        }

        bool SetDestination(ref Vector3 vDest, bool bDirectly = false)
        {
            if (!bDirectly)
            {
                var fTmp = vDest.y;
                vDest = SnapPosition(vDest, bSnapToGroundGrid);
                vDest.y = fTmp;
            }
            
            if ((cTransform.position - vDest).sqrMagnitude <= fSqrtMinMovement)
                return false;

            bOnMoving = true;
            NMAgent.isStopped = bSnapToGroundGrid;
            NMAgent.destination = vDest;
            return true;
        }

        override protected bool RayTest(Vector3 from, Vector3 vTranslate)
        {
            NavMeshHit nHit;
            return NavMesh.Raycast(from, from + vTranslate, out nHit, NavMesh.AllAreas);
        }

        bool EnQueueDirectionWithoutReset(InGameDirection dir)
        {
            if (isControllable)
                return base.EnQueueDirection(dir);

            return false;
        }

        IEnumerator CustomOnOffMeshAction_JumpOver()
        {
            OffMeshLinkData data = NMAgent.currentOffMeshLinkData;
            Vector3 startPos = NMAgent.transform.position + _baseOffset * Vector3.up;
            Vector3 endPos = data.endPos + _baseOffset * Vector3.up;

            UpdateAnimatorParams(true, endPos.x - startPos.x, endPos.z - startPos.z);

            float normalizedJumpingTime = 0;
            float inverseJumpingDuration = 1 / fTotalJumpingDuration;
            while (normalizedJumpingTime <= 1)
            {                
                float yOffset = JumpingHeight(normalizedJumpingTime);
                NMAgent.transform.position = Vector3.Lerp(startPos, endPos, normalizedJumpingTime) + yOffset * -Physics.gravity.normalized;
                normalizedJumpingTime += Time.deltaTime * inverseJumpingDuration;
                yield return null;
            }
            NMAgent.CompleteOffMeshLink();
            OffMeshAction = null;
        }

        public bool ClickToMove(out Vector3 destination)
        {
            destination = Vector3.zero;
            if (NMAgent.isOnNavMesh)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, 1000, -1, QueryTriggerInteraction.Ignore);

                const float fAllowClickGap = 0.125f;
                const float fMinDistanceOrigin = 0.75f;
                float fMinDistance = fMinDistanceOrigin;
                Vector3 _dest = Vector3.zero;
                bool bFound = false;

                var enumerator = hits.OrderBy(h => h.distance).GetEnumerator();

                while (enumerator.MoveNext())
                {
                    NavMeshHit nHit;
                    RaycastHit current = (RaycastHit)enumerator.Current;
                    if (NavMesh.SamplePosition(current.point, out nHit, 0.2f, NavMesh.AllAreas))
                    {
                        float fPathLenght = float.MaxValue;
                        NavMeshPath _path = new NavMeshPath();
                        if (NavMesh.CalculatePath(NMAgent.nextPosition, nHit.position, NavMesh.AllAreas, _path))
                        {
                            if (nHit.distance <= fAllowClickGap)
                            {
                                fPathLenght = 0;
                                Vector3 vLastPos = NMAgent.nextPosition;
                                for (int i = 0; i < _path.corners.Length; ++i)
                                {
                                    vLastPos.y = _path.corners[i].y;
                                    fPathLenght += Vector3.Distance(vLastPos, _path.corners[i]);
                                    vLastPos = _path.corners[i];
                                }
                            }
                        }
                        bool bClosestDestination = fPathLenght < fMinDistance;

                        if (bFound == false || bClosestDestination)
                        {
                            bFound = true;
                            _dest = nHit.position;

                            if (bClosestDestination)
                            {
                                fMinDistance = nHit.distance;
                            }
                        }
                    }
                }

                if (bFound)
                {
                    ResetPath();
                    Vector3 vTmp = _dest;
                    if (SetDestination(ref vTmp))
                    {
                        destination = vTmp;
                        return true;
                    }
                }
            }
            return false;
        }

        override public Bounds GetBounds()
        {
            Vector3 vSize = new Vector3(NMAgent.radius, NMAgent.height, NMAgent.radius);
            return new Bounds(NMAgent.nextPosition + NMAgent.baseOffset * Vector3.up, vSize);
        }

        override public bool EnQueueDirection(InGameDirection dir)
        {
            if (EnQueueDirectionWithoutReset(dir))
            {
                ResetPath();
                return true;
            }
            return false;
        }

        override public void DirectTranslate(Vector3 vTranslate)
        {
            if (isControllable)
            {
                ResetPath();
                var vDir = vTranslate.normalized;
                vTranslate = vTranslate * fSpeed;
                if (Grounding(NMAgent.nextPosition + vTranslate + vDir * NMAgent.radius, fMaxDropHeight))
                    SetHorizontalMovement(vTranslate);
            }
        }

        override public bool Grounding(Vector3 position, float fRayLength)
        {
            return isOnGround;
            //NavMeshHit rayHit;
            //return NMAgent.Raycast(position + Vector3.down * fRayLength, out rayHit);
        }

        override public int SortingOrder_Adjustment()
        {
            return IsometricSortingOrderUtility.IsometricSortingOrder(vJumpOffset);
        }

        override protected Vector3 GetHorizontalMovementVector()
        {
            if (isOnCustomOffMeshAction)
                return Vector3.zero;
            
            // Remaing Movement First
            Vector3 vMovementTmp = Vector3.zero;
            if (bOnMoving && vHorizontalMovement.magnitude > fMinMovement)
            {
                vMovementTmp = GetTickMovementVector(vHorizontalMovement);
                vHorizontalMovement -= vMovementTmp;
                return vMovementTmp;
            }

            // Get Next Direction based on NavMeshPath
            if (bSnapToGroundGrid)
            {
                if (NMAgent.hasPath && NMAgent.pathStatus != NavMeshPathStatus.PathInvalid && isRemainingDistanceForNMA)
                {
                    Vector3 vNext, vDestination;
                    bool bStop = !GetTDFromPath(NMAgent.nextPosition, NMAgent.path.corners, out vNext, out vDestination);

                    if (bStop)
                    {
                        if (NMAgent.path.corners.Length > 2)
                            SetDestination(ref vDestination, true);
                        else
                            Arrival();
                    }
                    else if (isControllable)
                    {
                        SetHorizontalMovement(vNext);
                        vMovementTmp = GetTickMovementVector(vHorizontalMovement);
                        vHorizontalMovement -= vMovementTmp;
                    }
                }
            }

            if (bOnMoving && vHorizontalMovement.Equals(Vector3.zero) &&
                (!NMAgent.enabled || !(NMAgent.pathPending || NMAgent.hasPath || isRemainingDistanceForNMA)))
            {
                Arrival();
            }
            else if (NMAgent.hasPath && !bSnapToGroundGrid)
                UpdateAnimatorParams(bOnMoving, NMAgent.desiredVelocity.x, NMAgent.desiredVelocity.z);

            return vMovementTmp;
        }

        override protected Vector3 GetMovementVector()
        {
            if (isOnJumping)
                NMAgent.baseOffset = GetVerticalMovementVector().magnitude + _baseOffset;

            return GetHorizontalMovementVector();
        }

        override protected void ApplyMovement(Vector3 vMovement)
        {
            if (!vMovement.Equals(Vector3.zero))
            {
                NMAgent.Move(vMovement);
                if (Vector3.Distance(NMAgent.nextPosition, NMAgent.path.corners.Last()) < fMinMovement)
                    ResetPath();
            }

            if (NMAgent.isOnOffMeshLink && NMAgent.currentOffMeshLinkData.valid)
            {
                if (IsCustomTravalOffMesh)
                {
                    if (!isOnCustomOffMeshAction)
                        OffMeshAction = StartCoroutine(CustomOnOffMeshAction_JumpOver());
                }
                else
                {
                    if (NMAgent.isOnOffMeshLink && NMAgent.currentOffMeshLinkData.valid)
                    {
                        if (NMAgent.isStopped)
                            NMAgent.isStopped = false;
                    }
                    else if (!NMAgent.isOnOffMeshLink && !NMAgent.isStopped)
                    {
                        NMAgent.isStopped = true;
                    }
                }
            }
        }

        override protected void Arrival()
        {
            base.Arrival();
            if (NMAgent.isOnNavMesh) // && !isRemainingDistanceForNMA)
                ResetPath();

            // Dispatch event
            if (OnDestinationArrival != null)
                OnDestinationArrival(this, destination);

        }

        #region GameObject
        void Init()
        {
            if (NMAgent == null)
                NMAgent = GetComponentInChildren<NavMeshAgent>();

            NMAgent.autoTraverseOffMeshLink = !IsCustomTravalOffMesh;
            NMAgent.updateRotation = bRotateToDirection;

            _baseOffset = NMAgent.baseOffset;

            base.Start();
            SetMinMoveDistance(Mathf.Min(NMAgent.stoppingDistance, fGridTolerance));

            NMAgent.speed = fSpeed;

            if (NMAgent.isOnNavMesh && destination)
                NMAgent.SetDestination(destination.transform.position);
            else
                Arrival();

            if (isOnCustomOffMeshAction)
            {
                StopCoroutine(OffMeshAction);
                OffMeshAction = null;
            }
        }
        override public void Start()
        {
            Init();
        }

        private void OnDestroy()
        {
            if (isOnCustomOffMeshAction)
            {
                StopCoroutine(OffMeshAction);
                OffMeshAction = null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (isActiveAndEnabled)
                Init();
        }
#endif
        #endregion
    }
}