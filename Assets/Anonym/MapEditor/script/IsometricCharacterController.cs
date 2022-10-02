using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Anonym.Isometric
{
    using Util;    

    [RequireComponent(typeof(CharacterController))]
    public class IsometricCharacterController : IsometricMovement
    {
        public static string defaultIsoMapPrefabPat = "Assets/Anonym/MapEditor/Scene/Tutorials/7.1 CharacterControllers/Iso CharacterController Sample.prefab";

        #region Character
        [Header("CharacterController")]
        [SerializeField]
        CharacterController CC;

        override public bool isOnGround { get { return CC.isGrounded; } }
        override public Transform cTransform { get { return CC.transform; } }
        override public Collider cCollider
        {
            get
            {
                if (characterCollider == null)
                    characterCollider = GetComponentInChildren<CharacterController>();

                return characterCollider;
            }
        }

        [SerializeField]
        bool bUseCustomColliderSize = false;

        [SerializeField, Util.ConditionalHide("bUseCustomColliderSize", hideInInspector:false)]
        Vector2 CCSize;
        #endregion Character
        override public void Jump()
        {
            if (bJumpWithMove)
            {
                // In order to ensure the bottom check
                CC.Move(Vector3.down * 1.25f * CC.minMoveDistance);

                if (isOnGround)
                    jumpStart();
            }
            else
                EnQueueDirection(InGameDirection.Jump_Move);

            return;
        }

        override public int SortingOrder_Adjustment()
        {
            // 땅에서 떨어진 정도가 CCSize.x 이상일 때 CCSize.y, CCSize.x 이하일 때 CCSize.x ~ CCSize.y 리턴
            float fXweight = 0f;
            //if ((CC.collisionFlags & CollisionFlags.Below) == 0)
            {
                RaycastHit _hit;
                float fOffset = CC.height * 0.5f + CC.skinWidth;
                if (Physics.Raycast(cTransform.position + CC.center, Vector3.down, out _hit,
                        CCSize.x + fOffset, CollisionLayerMask))
                {
                    fXweight = Mathf.Lerp(CCSize.x, 0f,
                        (_hit.distance - fOffset * 0.25f) / CCSize.x);
                }
            }
            Vector3 iv3Resolution = IsoMap.instance.fResolutionOfIsometric;
            return Mathf.RoundToInt(fXweight * CCSize.x * Mathf.Min(iv3Resolution.z, iv3Resolution.x) +
                (1f - fXweight) * CCSize.y * iv3Resolution.y);
        }

        #region MoveFunction
        override protected void ApplyMovement(Vector3 vMovement)
        {
            if (!vMovement.Equals(Vector3.zero))
            {
                CC.Move(vMovement);

                if ((CC.collisionFlags & CollisionFlags.Below) != 0)
                {
                    Grounding(transform.localPosition, 1f);
                }
                if ((CC.collisionFlags & CollisionFlags.Sides) != 0)
                {
                    if (bSnapToGroundGrid && bRevertPositionOnCollision)
                        SetHorizontalMovement(GetRevertVector());
                }
            }
        }
        #endregion

        #region GameObject
        override public void Start()
        {

            if (CC == null)
                CC = gameObject.GetComponent<CharacterController>();
            CC.enabled = true;

            base.Start();

            if (CCSize.Equals(Vector2.zero) && bUseCustomColliderSize)
            {
                CCSize = new Vector2(Mathf.Max(IsoGrid.fGridTolerance, CC.radius * 2f),
                    Mathf.Max(IsoGrid.fGridTolerance, CC.height + CC.center.y));
            }

            SetMinMoveDistance(Mathf.Min(CC.minMoveDistance, fGridTolerance));            

            vDestinationCoordinates.Set(Mathf.RoundToInt(cTransform.localPosition.x), 0, Mathf.RoundToInt(cTransform.localPosition.z));
        }
        #endregion

        #region Pathfinder
        [Header("Pathfinder")]
        [SerializeField]
        SimplePathfinder pathFinder = null;
        private bool hasActivePathFinder
        {
            get
            {
                return pathFinder != null && pathFinder.isActive;
            }
        }

        protected void resetPathFinder()
        {
            if (hasActivePathFinder)
                pathFinder.Reset();
        }

        override protected void Arrival()
        {
            resetPathFinder();
            base.Arrival();
        }
        
        override protected Vector3 GetHorizontalMovementVector()
        {
            if (UpdatePathFinder(out Vector3 vResult))
                return vResult;

            if (bSnapToGroundGrid)
                UpdateAnimatorParams(bOnMoving, vHorizontalMovement.x, vHorizontalMovement.z);

            return base.GetHorizontalMovementVector();
        }

        override public void DirectTranslate(Vector3 vTranslate)
        {
            resetPathFinder();
            base.DirectTranslate(vTranslate);
        }

        Vector3 HorizontalMovement_OnMoving()
        {
            Vector3 vMovementTmp = Vector3.zero;
            if (vHorizontalMovement.magnitude <= fMinMovement)
            {
                bOnMoving = false;
            }
            else
            {
                vMovementTmp = GetTickMovementVector(vHorizontalMovement);
                vHorizontalMovement -= vMovementTmp;
            }
            return vMovementTmp;
        }

        void HorizontalMovement_NotMoving(Vector3 vFeet)
        {
            bOnMoving = GetTDFromPath(vFeet, pathFinder.vRoutes.ToArray(), out vHorizontalMovement, out var vDestination, Vector3.up * CC.stepOffset);
            if (!bOnMoving)
            {
                if (pathFinder.vRoutes.Count > 2)
                    bOnMoving = Vector3.Distance(vFeet, vDestination) > IsoGrid.fGridTolerance;
                else
                    Arrival();
            }
        }

        bool UpdatePathFinder(out Vector3 vResult)
        {
            if (hasActivePathFinder)
            {
                var vHorizontal = Vector3.zero;
                var vFeet = vFeetPosition;

                pathFinder.bHoldRefresh = isOnJumping || !CC.isGrounded;
                pathFinder.TryToRefresh(vFeet);

                if (pathFinder.hasPath)
                {
                    if (bSnapToGroundGrid)
                    {
                        if (bOnMoving)
                        {
                            vResult = HorizontalMovement_OnMoving();
                            return true;
                        }
                        else
                            HorizontalMovement_NotMoving(vFeet);
                    }
                    else
                    {
                        vHorizontal = pathFinder.GetTransition(vFeet, fTickSpeed);
                        base.DirectTranslate(vHorizontal);
                    }

                    if (SimplePathfinder.JumpCheck(cTransform, vFeet, vHorizontal, CC.stepOffset, CC.bounds.center, CC.height, CC.radius))
                        Jump();
                }
                else
                    Arrival();

                if (!bOnMoving)
                {
                    vResult = vHorizontal;
                    return true;
                }
            }

            vResult = Vector3.zero;
            return false;
        }

        override public bool EnQueueDirection(InGameDirection dir)
        {
            resetPathFinder();
            return base.EnQueueDirection(dir);
        }
        #endregion
    }
}