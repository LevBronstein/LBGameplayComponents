using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LBGameplay
{
    [System.Serializable]
    public struct LBCoordinateSystem
    {
        private Vector3 up, right, forward;
        bool bisortho;

        public LBCoordinateSystem(Vector3 _up, Vector3 _right, Vector3 _forward, bool _ortho = true)
        {
            up = _up.normalized;
            right = _right.normalized;
            forward = _forward.normalized;
            bisortho = _ortho;

            if (_ortho)
                Vector3.OrthoNormalize(ref up, ref right, ref forward);
        }

        public bool bIsValid
        {
            get
            {
                if (bisortho)
                    return (Vector3.Angle(up, right) - 90 <= 0.05) && (Vector3.Angle(forward, right) - 90 <= 0.05) && (Vector3.Angle(up, forward) - 90 <= 0.05);
                else
                    return true;
            }
        }
   
        public Vector3 Up
        {
            get
            {
                return up;
            }
        }

        public Vector3 Right
        {
            get
            {
                return right;
            }
        }

        public Vector3 Forward
        {
            get
            {
                return forward;
            }
        }

        public void ChangeUp(Vector3 new_up)
        {
            if (LBMath.TruncVector3(new_up.normalized) == LBMath.TruncVector3(right.normalized))
            {
                right = forward;
                forward = up;
                up = LBMath.TruncVector3(new_up.normalized);
                return;
            }
            else if (LBMath.TruncVector3(new_up.normalized) == LBMath.TruncVector3(forward))
            {
                forward = right;
                right = up;
                up = LBMath.TruncVector3((new_up.normalized));
                return;
            }
            else if (LBMath.TruncVector3(new_up.normalized) == LBMath.TruncVector3(up))
                return;
            else
            {
                up = LBMath.TruncVector3(new_up.normalized);
                right = LBMath.TruncVector3(Vector3.ProjectOnPlane(right, new_up).normalized);
                forward = LBMath.TruncVector3((Vector3.ProjectOnPlane(forward, new_up).normalized));
                if (bisortho)
                    Vector3.OrthoNormalize(ref up, ref right, ref forward);
            }
        }
    
        public Vector3 TransformVectorIn(Vector3 v)
        {
            //Vector3 _up, _right, _forward;

            //_up = LBMath.TruncVector3(Vector3.Project(v, up));
            //_right = LBMath.TruncVector3(Vector3.Project(v, right));
            //_forward = LBMath.TruncVector3(Vector3.Project(v, forward));

            return v.x * right + v.y * up + v.z * forward;
            //return v.x * _right + v.y * _up + v.z * _forward;
            //return Vector3.Dot(v,up) * up + Vector3.Dot(v,right) * right + Vector3.Dot(v, forward) * forward;
        }
    }

    [AddComponentMenu("LBGameplay/Pedstrain Surface Movement")]
    public class LBPedestrianSurfaceMovementGameplayComponent : LBPhysicsGameplayComponent
    {
        const int STAND = 1;
        const int WALK = 2;
        const int TURNINPLACE = 4;
        const int NOFLOOR = 3;

        const int MAXNOFLOORMS = 3000;
        const float MAXIDLETHRESH = 0.005f;

        [SerializeField]
        private LBFloatInverval speed_restraint;
        [SerializeField]
        private LBFloatInverval direction_restraint;
        [SerializeField]
        private float movement_speed;
        [SerializeField]
        private float rotation_speed;
        [SerializeField]
        private float movement_accel;
        [SerializeField]
        private float rotation_accel;
        [SerializeField]
        private Vector3 direction;
        private float desired_speed;
        private float current_direction;
        private float desired_direction; //[-180; 180] from Vector3.forward
        private float nofloor_counter;

        protected override bool Init()
        {
            int i;

            if (base.Init())
            {
                floor_normal = new LBFloatInverval(-100, 100, false);
                //floor_normal = new LBFloatInverval(-180, 180);
                speed_restraint = new LBFloatInverval(0, 5);



                return true;
            }

            return false;
        }

        protected void ProcessState(int state)
        {
            if (InternalStateID != state)
            {
                if (!SwitchState(state))
                {
                    Debug.Log(">>>>>>GEMOR: Cannot switch state " + InternalState.Name + " to " + AllInternalStates[state].Name);
                    return; 
                }
            }

            switch (InternalStateID)
            {
                case 0:
                    ProcessDefaultState();
                    break;
                case STAND:
                    ProcessStandState();
                    break;
                case WALK:
                    ProcessWalkState();
                    break;
                case NOFLOOR:
                    ProcessNoFloorState();
                    break;
                case TURNINPLACE:
                    ProcessTurnInPlaceState();
                    break;
                default:
                    break;
            }
        }

        protected override void PerformState()
        {
            base.PerformState();

            //switch (InternalStateID)
            //{
            //    case 0:
            //        ProcessDefaultState();
            //        break;
            //    case STAND:
            //        ProcessStandState();
            //        break;
            //    case WALK:
            //        ProcessWalkState();
            //        break;
            //    case NOFLOOR:
            //        ProcessNoFloorState();
            //        break;
            //    case TURNINPLACE:
            //        ProcessTurnInPlaceState();
            //        break;
            //    default:
            //        break;
            //}

            if (InternalStateID == 0)
            {
                //if (bHasFloor)
                //{
                //    if (bIsWalking()) // if we're currently moving (walking)
                //    {
                //        if (bCanWalkInDirection(NewWalkDirection())) // if we can walk in this direction
                //        {
                //            if (bWantsToWalk()) // we're walking and we want to walk
                //            {
                //                ProcessState(WALK);
                //            }
                //            else // we're walking, but we want to stop
                //            {
                //                ProcessState(WALK); // handle braking in the same func
                //            }
                //        }
                //        else // we cannot walk in this direction (i.e. we hit wall)
                //        {
                //            // if speed > 0 -- if the hit was significant
                //            ProcessState(STAND); // go to standing state
                //        }

                //    }
                //    else // we're not moving
                //    {
                //        if (bWantsToTurn()) // if we want to turn in place
                //        {
                //            ProcessState(TURNINPLACE);
                //        }
                //        else // if we don't want to turn in place
                //        {
                //            if (bWantsToWalk()) // if we want to walk
                //            {
                //                if (bCanWalkInDirection(NewWalkDirection())) // if only we can walk in that direction
                //                {
                //                    ProcessState(WALK);
                //                }
                //                else
                //                {
                //                    ProcessState(STAND); // else -- just stand still
                //                }
                //            }
                //            else
                //            {
                //                ProcessState(STAND);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    ProcessState(NOFLOOR);
                //}
            }
            else if (InternalStateID == WALK)
            {
                if (bHasFloor)
                {
                    if (bCanWalkInDirection(NewWalkDirection()))
                    {
                        if (bIsWalking())
                        {
                            if (bWantsToWalk()) // we're walking and we want to walk
                            {
                                ProcessWalkState();
                            }
                            else // we're walking, but we want to stop
                            {
                                ProcessWalkState(); // handle braking in the same func
                            }
                        }
                        else
                        {
                            if (bWantsToWalk()) // we're walking and we want to walk
                            {
                                ProcessWalkState();
                            }
                            else
                            {
                                SwitchState(STAND); // WTF, we should be walking at this point!
                            }
                        }
                    }
                    else // we cannot walk in this direction (i.e. we hit wall)
                    {
                        if (bIsWalking()) // we were walking (non-zero velocity), but suddenly a wall appeared :(
                        {
                            // if speed > 0 -- if the hit was significant
                            SwitchState(STAND);
                        }
                        else
                        {
                            SwitchState(STAND); // go to standing state
                        }
                        
                    }
                }
                else
                {
                    SwitchState(NOFLOOR);
                }
            }
            else if (InternalStateID == STAND)
            {
                if (bHasFloor)
                {
                    if (bWantsToTurn()) // if we want to turn in place
                    {
                        SwitchState(TURNINPLACE);
                    }
                    else // if we don't want to turn in place
                    {
                        if (bWantsToWalk()) // if we want to walk
                        {
                            if (bCanWalkInDirection(NewWalkDirection())) // if only we can walk in that direction
                            {
                                SwitchState(WALK);
                            }
                            else
                            {
                                ProcessStandState(); // else -- just stand still
                            }
                        }
                        else
                        {
                            ProcessStandState();
                        }
                    }
                }
                else
                {
                    SwitchState(NOFLOOR);
                }
            }
            else if (InternalStateID == TURNINPLACE)
            {
                if (bHasFloor)
                {
                    if (bWantsToTurn())
                    {
                        ProcessTurnInPlaceState();
                    }
                    else
                    {
                        SwitchState(STAND);
                    }
                }
                else
                {
                    SwitchState(NOFLOOR);
                }
            }
            else if (InternalStateID == NOFLOOR)
            {
                if (bHasFloor)
                {
                    //seems like we've just landed
                    //SwitchState(LAND)
                    SwitchState(WALK);
                }
                else
                {
                    ProcessNoFloorState();
                }
            }
            
        }

        protected void ProcessDefaultState()
        {
            if (bHasFloor)
                SwitchState(WALK);
            else
                SwitchState(NOFLOOR);
        }

        //protected bool bCanProcessWalkState()
        //{
        //    if (bHasFloor && LBMath.TruncFloat(desired_speed) > MAXIDLETHRESH)
        //        return true;
        //    else
        //        return false;
        //}

        protected bool bIsWalking()
        {
            if (bHasFloor && LBMath.TruncFloat(RBSpeedVectorFlat.magnitude) > MAXIDLETHRESH)
                return true;
            else
                return false;
        }

        protected bool bWantsToWalk()
        {
            if (bHasFloor && LBMath.TruncFloat(desired_speed) > MAXIDLETHRESH)
                return true;
            else
                return false;
        }

        protected bool bIsStanding()
        {
            if (bHasFloor && LBMath.TruncFloat(RBSpeedVectorFlat.magnitude) <= MAXIDLETHRESH)
                return true;
            else
                return false;
        }

        protected bool bWantsToStand()
        {
            if (bHasFloor && LBMath.TruncFloat(desired_speed) <= MAXIDLETHRESH)
                return true;
            else
                return false;
        }

        //protected bool bIsTurning()
        //{
        //    if (bHasFloor && LBMath.TruncFloat(desired_speed) <= MAXIDLETHRESH)
        //        return true;
        //    else
        //        return false;
        //}

        protected bool bWantsToTurn()
        {
            if (bHasFloor && desired_direction != current_direction)
                return true;
            else
                return false;
        }

        protected bool bCanWalkInDirection(Vector3 dir)
        {
            if (!bHasFloor)
                return false;

            LBFloatInverval ang;
            int i;

            ang = new LBFloatInverval(-90, 90, false);

            for (i = 0; i < AllWallPoints.Length; i++)
            {
                if (ang.CheckValue(Vector3.SignedAngle(dir.normalized, AllWallPoints[i].normal, Vector3.up)))
                    return false;
            }

            return true;
        }

        protected bool bHasBlockingWall()
        {
        //    int i;
        //    LBFloatInverval ang;

        //    ang = new LBFloatInverval(-90, 90, false);

        //    for (i=0;i<AllWallPoints.Length;i++)
        //    {
        //        if (LBMath.TruncFloat(RBSpeedVectorFlat.magnitude) != 0) // we're not moving
        //        {
        //            if (ang.CheckValue(Vector3.SignedAngle(RBSpeedVectorFlat.normalized, AllWallPoints[i].normal, Vector3.up)))
        //                return true;
        //        }
        //        else
        //        {
        //            if (ang.CheckValue(Vector3.SignedAngle(target_rigidbody.transform.forward.normalized, AllWallPoints[i].normal, Vector3.up)))
        //                return true;
        //        }
               
        //    }

           return false;
        }

        protected Quaternion NewRotation()
        {
            current_direction = desired_direction;
            return Quaternion.FromToRotation(Vector3.up, FloorNormal) * Quaternion.LookRotation(Quaternion.Euler(0, current_direction, 0) * Vector3.forward, Vector3.up);
        }

        protected Vector3 NewWalkDirection()
        {
            Vector3 v;
            v = NewRotation() * Vector3.forward;
            Debug.DrawLine(target_rigidbody.transform.position, target_rigidbody.transform.position + v, Color.red);
            return v;
        }

        protected Vector3 NewWalkSpeed()
        {
            float _res_spd;
            _res_spd = LerpFloat(LBMath.TruncFloat(RBSpeedVectorFlat.magnitude), desired_speed, movement_accel, Time.fixedDeltaTime);
            return _res_spd * target_rigidbody.transform.forward - FloorNormal * 0.1f * _res_spd;
        }

        protected void ProcessStandState()
        {
            //if (bHasFloor)
            //{
            //    if (LBMath.TruncFloat(desired_speed) > MAXIDLETHRESH && !bHasBlockingWall())
            //    {
            //        SwitchState(WALK);
            //    }
            //    else
            //    {
            //        if (current_direction != desired_direction) // should start turn in place
            //        {
            //            SwitchState(TURNINPLACE);
            //        }
            //    }
            //}
            //else
            //{
            //    SwitchState(NOFLOOR);
            //}
        }

        protected void ProcessWalkState()
        {
            Vector3 new_up, new_forward, floor_fwd;
            Quaternion new_rotation;

            //Quaternion get_new_dir()
            //{
            //    return Quaternion.FromToRotation(Vector3.up, FloorNormal) * Quaternion.LookRotation(Quaternion.Euler(0, desired_direction, 0) * Vector3.forward, Vector3.up);
            //}

            //Vector3 get_new_speed()
            //{
            //    float _res_spd;
            //    _res_spd = LerpFloat(LBMath.TruncFloat(RBSpeedVectorFlat.magnitude), desired_speed, movement_accel, Time.fixedDeltaTime);
            //    return _res_spd * target_rigidbody.transform.forward - FloorNormal * 0.1f * _res_spd;
            //}

            //void walk()
            //{
            //    float res_spd;
            //    res_spd = LerpFloat(LBMath.TruncFloat(RBSpeedVectorFlat.magnitude), desired_speed, movement_accel, Time.fixedDeltaTime);
            //    current_direction = LerpFloat(current_direction, desired_direction, rotation_speed, Time.fixedDeltaTime);
            //    //target_rigidbody.transform.up = FloorNormal;
            //    //target_rigidbody.transform.rotation = target_rigidbody.transform.rotation * Quaternion.Euler(0, current_direction, 0);
            //    new_rotation = Quaternion.FromToRotation(Vector3.up, FloorNormal) * Quaternion.LookRotation(Quaternion.Euler(0, desired_direction, 0) * Vector3.forward, Vector3.up); //Quaternion.Euler(0, desired_direction, 0);
            //    target_rigidbody.transform.rotation = new_rotation;
            //    target_rigidbody.velocity = res_spd * target_rigidbody.transform.forward - FloorNormal * 0.1f * res_spd;
            //    UpdateSlider(0, speed_restraint.Percentage(LBMath.TruncFloat(RBSpeedVectorFlat.magnitude)));
            //}

            //void walk()
            //{
            //    target_rigidbody.transform.rotation = NewRotation();
            //    target_rigidbody.velocity = NewWalkSpeed();
            //    UpdateSlider(0, speed_restraint.Percentage(LBMath.TruncFloat(RBSpeedVectorFlat.magnitude)));
            //}

            //if (bHasFloor)
            //{
            //    if (bIsWalking())
            //    {
            //        if (bCanWalkInDirection(NewWalkDirection())) // if we can walk the direction (no walls, etc)
            //        {
            //            if (bWantsToWalk()) // if we're currently walking and still want to walk
            //            {
            //                walk(); // accelerating
            //            }
            //            else // we may need to perform full stop
            //            {
            //                walk(); // braking is handled in the same func                      
            //            }
            //        }
            //        else // if we hit blocking a wall
            //        {
            //            SwitchState(STAND);
            //        }
            //    }
            //    else
            //    {
            //        if (bWantsToWalk()) // should accelerate from static state
            //        {
            //            if (!bHasBlockingWall())
            //                walk(); // we're accelerating now
            //            else
            //                SwitchState(STAND);
            //            // current_direction != desired_direction -- can start moveing in opposite direction (need special state)!
            //        }
            //        else
            //        {
            //            if (current_direction != desired_direction) // should start turn in place
            //            {
            //                SwitchState(TURNINPLACE);
            //            }
            //            else
            //            {
            //                SwitchState(STAND);
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    SwitchState(NOFLOOR);
            //}

            target_rigidbody.transform.rotation = Quaternion.RotateTowards(target_rigidbody.transform.rotation, NewRotation(), RotationSpeed);
            target_rigidbody.velocity = NewWalkSpeed();
            UpdateSlider(0, speed_restraint.Percentage(LBMath.TruncFloat(RBSpeedVectorFlat.magnitude)));
        }

        protected void ProcessNoFloorState()
        {
            //if (!bHasFloor)
            //{
            //    if (nofloor_counter < MAXNOFLOORMS)
            //    {
            //        nofloor_counter += Time.fixedDeltaTime;

            //        target_rigidbody.velocity = Gravity;
            //    }
            //    else
            //    {
            //        nofloor_counter = 0;
            //    }
            //}
            //else
            //{
            //    SwitchState(WALK);
            //}

            if (nofloor_counter < MAXNOFLOORMS)
            {
                nofloor_counter += Time.fixedDeltaTime;

                target_rigidbody.velocity = Gravity;
            }
            else
            {
                nofloor_counter = 0;
            }
        }

        protected void ProcessTurnInPlaceState()
        {
            //if (bIsStanding() && !bWantsToWalk() && bWantsToTurn())
            //{
            //    current_direction = LerpFloat(current_direction, desired_direction, rotation_speed, Time.fixedDeltaTime);
            //    target_rigidbody.transform.up = FloorNormal;
            //    target_rigidbody.transform.rotation = target_rigidbody.transform.rotation * Quaternion.Euler(0, current_direction, 0);
            //}
            //else if (bIsStanding() && bWantsToWalk())
            //    SwitchState(WALK);
            //else if (bIsStanding() && !bWantsToTurn())
            //    SwitchState(STAND);
            //else if (!bHasFloor)
            //    SwitchState(NOFLOOR);

            target_rigidbody.transform.rotation = NewRotation();
        }

        //public float MovementSpeed
        //{
        //    get
        //    {
        //        return speed;
        //    }

        //    set
        //    {
        //        speed = speed_restraint.ClampValue(value);
        //    }
        //}

        public float MovementAcceleration
        {
            get
            {
                return movement_accel;
            }
            set
            {
                movement_accel = value; 
            }
        }

        public float RotationSpeed
        {
            get
            {
                return rotation_speed;
            }
            set
            {
                rotation_speed = value;
            }
        }

        //public Vector3 MovementDirection
        //{
        //    get
        //    {
        //        return direction;
        //    }
        //    set
        //    {
        //        direction = value;
        //    }
        //}

        public float InputSpeed
        {
            get
            {
                return desired_speed;
            }
            set
            {
                desired_speed = value;
            }
        }

        public float InputDirection
        {
            get
            {
                return desired_direction; 
            }
            set
            {
                desired_direction = Mathf.Clamp(value,-180,180);
            }
        }

        public LBFloatInverval SpeedRestraint
        {
            get
            {
                return speed_restraint;
            }
        }

        public override Vector3 Gravity
        {
            get
            {
                if (bHasFloor)
                    return -FloorNormal * Physics.gravity.magnitude;
                else
                    return Physics.gravity;
            }
        }

        //coords.ChangeUp(TruncVector3(FloorNormal));
        //v = coords.TransformVectorIn(Vector3.forward).normalized;
        //v = v -coords.Up;
        //target_rigidbody.transform.up = coords.Up;
    }

    namespace Editors
    {
        [CustomEditor(typeof(LBPedestrianSurfaceMovementGameplayComponent))]
        public class LBPedstrainSurfaceMovementGameplayComponent_ED : LBPhysicsGameplayComponent_ED
        {
            protected virtual void DisplayMovementProperties_Editor()
            {
                LBPedestrianSurfaceMovementGameplayComponent psm;

                psm = (LBPedestrianSurfaceMovementGameplayComponent)target;

                MakeCenteredLabel("------ Movement info ------", 8, 8);

                MakeCenteredLabel("------ Movement parameters ------", 8, 8);
                EditorGUILayout.BeginVertical();
                //psm.MovementSpeed = EditorGUILayout.FloatField("Base movement speed", psm.MovementSpeed);
                psm.MovementAcceleration = EditorGUILayout.FloatField("Movement acceleration", psm.MovementAcceleration);
                psm.RotationSpeed = EditorGUILayout.FloatField("Rotation speed", psm.RotationSpeed);
                //psm.MovementDirection = EditorGUILayout.Vector3Field("Movemet direction", psm.MovementDirection);
                psm.InputSpeed = EditorGUILayout.Slider(psm.InputSpeed, psm.SpeedRestraint.IntervalStart, psm.SpeedRestraint.IntervalEnd);
                //psm.InputDirection = EditorGUILayout.Vector3Field("Input direction", psm.InputDirection);
                psm.InputDirection = EditorGUILayout.Slider(psm.InputDirection, -180, 180);
                EditorGUILayout.EndVertical();
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                DisplayMovementProperties_Editor();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
