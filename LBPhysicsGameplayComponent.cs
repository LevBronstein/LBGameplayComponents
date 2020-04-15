using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LBGameplay
{
	[System.Serializable]
	public struct LBFloatInverval
	{
		[SerializeField]
		private float value_a;
		[SerializeField]
		private float value_b;
		[SerializeField]
		private bool b_inner_interval;
		[SerializeField]
		private bool b_strict_a;
		[SerializeField]
		private bool b_strict_b;

		public float IntervalStart
		{
			get
			{
				return Mathf.Min(value_a, value_b);
			}
		}

		public float IntervalEnd
		{
			get
			{
				return Mathf.Max(value_a, value_b);
			}
		}

		public bool bIsInnerInterval
		{
			get
			{
				return b_inner_interval;
			}
		}

		public LBFloatInverval(float a, float b, bool inside = true, bool strict_a = false, bool strict_b = false)
		{
			value_a = a;
			value_b = b;
			b_inner_interval = inside;
			b_strict_a = strict_a;
			b_strict_b = strict_b;
		}

		public bool CheckValue(float value)
		{
			float min, max;

			if (b_inner_interval)
			{
				if (value >= IntervalStart && !b_strict_a || value > IntervalStart && b_strict_a)
				{
					if (value <= IntervalEnd && !b_strict_b || value < IntervalEnd && b_strict_b)
						return true;
				}
			}
			else
			{
				if (value <= IntervalStart && !b_strict_a || value < IntervalStart && b_strict_a)
					return true;

				if (value >= IntervalEnd && !b_strict_b || value > IntervalEnd && b_strict_b)
					return true;
			}

			return false;
		}

		public static LBFloatInverval UnitInterval
		{
			get
			{
				return new LBFloatInverval(0, 1);
			}
		}

		public override string ToString()
		{
			char bracket_a, bracket_b;
			string res;

			bracket_a = b_strict_a ? '(' : '[';
			bracket_b = b_strict_b ? ')' : ']';

			if (IntervalStart != IntervalEnd)
				return (bracket_a + IntervalStart.ToString() + ';' + IntervalEnd.ToString() + bracket_b);
			else
				return (bracket_a + IntervalStart.ToString() + bracket_b);
		}
	}

	[AddComponentMenu("LBGameplay/Physics Gameplay Component (Dummy)")]
	public class LBPhysicsGameplayComponent : LBAnimatedGameplayComponent
    {
        protected Rigidbody target_rigidbody;
		protected Collider target_collider;

		private List<Collision> collisions = new List<Collision>();
		private ContactPoint[] floor_points;

		protected LBFloatInverval floor_normal = new LBFloatInverval(-120, 120, false);

		protected override bool Init()
		{
			if (!base.Init())
				return false;

			target_rigidbody = GetComponent<Rigidbody>();
			target_collider = GetComponent<Collider>(); // what if we've got several of them?

			floor_points = new ContactPoint[0];

			if (target_rigidbody == null)
				return false;

			return true;
		}

		protected virtual void PerformPhysics() { }

		// Update is called once per physics tick
		void FixedUpdate()
		{
			// if (!enabled) return;

			// update floor info each physics tick
			floor_points = GetAllFloorPoints(collisions.ToArray());

			PerformPhysics();
		}

		protected virtual void HandleCollisionEnterEvent(Collision collision) { }

		protected virtual void HandleCollisionStayEvent(Collision collision) { }

		protected virtual void HandleCollisionExitEvent(Collision collision) { }

		void OnCollisionEnter(Collision collision)
        {
			// it is a good idea to notify when we get absolutely new collision (this means, it hit our object)
			RegisterCollision(collision);

			HandleCollisionEnterEvent(collision);
		}
		
		void OnCollisionStay(Collision collision)
		{
			UpdateCollision(collision);

			HandleCollisionStayEvent(collision);
		}

		void OnCollisionExit(Collision collision)
		{
			UnregisterCollision(collision);

			HandleCollisionExitEvent(collision);
		}

		private void RegisterCollision(Collision c)
		{
			int i;

			for (i = 0; i < collisions.Count; i++)
			{
				// if colliding with same collider -- just update it
				if (collisions[i].collider == c.collider)
                {
					collisions[i] = c;
					return;
                }
			}

			// maybe should give some notify, it it is a new collision?
			collisions.Add(c);
		}

		private void UnregisterCollision(Collision c)
		{
			int i;

			for (i = 0; i < collisions.Count; i++)
			{
				// check, if we had this collision
				if (collisions[i].collider == c.collider)
				{
					collisions.RemoveAt(i);
					return;
				}
			}

			Debug.Log(">>>>>>>>>>> GEMOR: trying to unregister non-existent collision <<<<<<<<<<<<<<<<");
		}

		// used for cases like dragging object (without actual hitting anything), retrun flase if we get absolutely new collision
		private bool UpdateCollision(Collision c)
		{
			int i;

			for (i = 0; i < collisions.Count; i++)
			{
				// if colliding with same collider -- just update it
				if (collisions[i].collider == c.collider)
				{
					collisions[i] = c;
					return true;
				}
			}

			Debug.Log(">>>>>>>>>>> GEMOR: got new collision OnCollisionStay! What??!! <<<<<<<<<<<<<<<<");
			return false;
		}

		protected void DrawContactPoint(ContactPoint pt)
		{
			Debug.DrawLine(pt.point, pt.point + pt.normal.normalized * 0.1f, Color.blue); 
		}

		protected ContactPoint GetFloorPoint(ContactPoint[] pts)
        {
			int i, min = 0;
			float angle, min_angle;
			ContactPoint pt;

			pt = new ContactPoint();

			if (pts == null || pts.Length == 0)
				return pt;

			min_angle = Vector3.SignedAngle(pts[0].normal, Physics.gravity, Vector3.up);
			DrawContactPoint(pts[0]);

			for (i = 1; i < pts.Length; i++)
            {
				angle = Vector3.SignedAngle(pts[i].normal, Physics.gravity, Vector3.up);

				if (angle < min_angle)
				{
					min = i;
					min_angle = angle;
				}

				DrawContactPoint(pts[i]);
			}

			//if (min_angle >= 160 || min_angle <= -160)
			//	return pts[min];

			if (floor_normal.CheckValue(min_angle))
				return pts[min];

			return pt;
		}

		protected ContactPoint[] GetAllFloorPoints (Collision[] cols)
        {
			int i,j, min;
			float angle, min_angle;
			ContactPoint c;
			ContactPoint[] contacts;
			List<ContactPoint> floor_pts = new List<ContactPoint>();

			for (i = 0; i < cols.Length; i++)
            {
				if (cols[i].contactCount > 0)
				{
					contacts = new ContactPoint[0];
					System.Array.Resize(ref contacts, cols[i].contactCount);
					cols[i].GetContacts(contacts);
					c = GetFloorPoint(contacts);

					if (c.otherCollider != null)
						floor_pts.Add(c);
				}
            }

			if (floor_pts.Count > 1)
			{
				min_angle = Vector3.SignedAngle(floor_pts[0].normal, Physics.gravity, Vector3.up);
				min = 0;

				for (i = 0; i < floor_pts.Count; i++)
				{
					for (j = i; j < floor_pts.Count; j++)
					{
						angle = Vector3.SignedAngle(floor_pts[j].normal, Physics.gravity, Vector3.up);

						if (angle < min_angle)
						{
							min = j;
							min_angle = angle;
						}
					}

					c = floor_pts[i];
					floor_pts[i] = floor_pts[min];
					floor_pts[min] = c;
				}
			}

			return floor_pts.ToArray();
        }

		public bool bHasFloor
        {
			get
            {
				if (floor_points != null && floor_points.Length > 0)
					return true;
				else
					return false;
			}
        }

		public Vector3 FloorNormal
        {
			get
            {
				// return only the most suitable floor point's normal
				if (floor_points != null && floor_points.Length > 0)
					return floor_points[0].normal;
				else
					return Vector3.zero;
			}
        }

		public GameObject FloorObject
        {
			get
            {
				// return only the most suitable floor object
				if (floor_points != null && floor_points.Length > 0)
					return floor_points[0].otherCollider.gameObject;
				else
					return null;
			}
        }

		public float RBSpeed
		{
			get
			{
				return target_rigidbody.velocity.magnitude;
			}

			protected set
			{
				target_rigidbody.velocity = target_rigidbody.velocity.normalized * value;
			}
		}

		public float RBSpeedFlat
		{
			get
			{
				return RBSpeedVectorFlat.magnitude;
			}

			protected set
			{
				target_rigidbody.velocity = RBSpeedVectorFlat.normalized * value;
			}
		}

		public Vector3 RBSpeedDir
		{
			get
			{
				return target_rigidbody.velocity.normalized;
			}

			protected set
			{
				target_rigidbody.velocity = value.normalized * target_rigidbody.velocity.magnitude;
			}
		}

		public Vector3 RBSpeedDirFlat
		{
			get
			{
				return RBSpeedVectorFlat.normalized;
			}

			protected set
			{
				RBSpeedVectorFlat = value;
			}
		}

		public Vector3 RBSpeedVector
		{
			get
			{
				return target_rigidbody.velocity;
			}

			protected set
			{
				target_rigidbody.velocity = value;
			}
		}

		public Vector3 RBSpeedVectorFlat
		{
			get
			{
				return Vector3.ProjectOnPlane(target_rigidbody.velocity, target_rigidbody.transform.up);
			}

			protected set
			{
				target_rigidbody.velocity = Vector3.ProjectOnPlane(value, target_rigidbody.transform.up);
			}
		}

		public Vector3 RBForwardDir
		{
			get
			{
				return target_rigidbody.transform.forward;
			}

			set
			{
				target_rigidbody.rotation = Quaternion.LookRotation(value);
			}
		}

		public Vector3 RBForwardDirFlat
		{
			get
			{
				return Vector3.ProjectOnPlane(target_rigidbody.transform.forward, target_rigidbody.transform.up);
			}

			set
			{
				target_rigidbody.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(value, target_rigidbody.transform.up));
			}
		}

		protected float TruncFloat(float f, byte numbers = 3)
		{
			return (float)System.Math.Truncate(f * (int)System.Math.Pow(10, numbers)) / (int)System.Math.Pow(10, numbers);
		}

		protected bool CompareVect3(Vector3 a, Vector3 b, byte numbers = 3)
		{
			float sqrmag;

			sqrmag = Vector3.SqrMagnitude(a - b);
			sqrmag = TruncFloat(sqrmag, numbers);

			if (sqrmag == 0)
				return true;
			else
				return false;
		}

		protected float LerpFloat(float current, float target, float step, float dt)
		{
			float value;

			if (Mathf.Abs(current - target) > Mathf.Abs(step))
			{
				if (current < target)
					value = current + Mathf.Abs(step);
				else
					value = current - Mathf.Abs(step);
			}
			else
			{
				if (current < target)
					value = current + Mathf.Abs(current - target);
				else
					value = current - Mathf.Abs(current - target);
			}

			return value;
		}

		protected float LerpAngle(float current, float target, float step, float dt)
		{
			float value;

			if (current < target)
			{
				if (Mathf.Abs(target - current) < 180)
				{
					if (Mathf.Abs(current - target) > Mathf.Abs(step))
						value = current + step;
					else
						value = current + Mathf.Abs(current - target);
				}
				else
				{
					if (Mathf.Abs(current - target) > Mathf.Abs(step))
						value = current - step;
					else
						value = current - Mathf.Abs(current - target);

					if (value < 0)
						value = value + 360;
				}
			}
			else
			{
				if (Mathf.Abs(target - current) < 180)
				{
					if (Mathf.Abs(current - target) > Mathf.Abs(step))
						value = current - step;
					else
						value = current - Mathf.Abs(current - target);
				}
				else
				{
					if (Mathf.Abs(current - target) > Mathf.Abs(step))
						value = current + step;
					else
						value = current + Mathf.Abs(current - target);

					if (value > 360)
						value = value - 360;
				}
			}

			return value;
		}
	}

	namespace Editors
	{
		[CustomEditor(typeof(LBPhysicsGameplayComponent))]
		public class LBPhysicsGameplayComponent_ED : LBAnimatedGameplayComponent_ED
		{
			protected virtual void DisplayPhysicsStats_Game()
			{
				LBPhysicsGameplayComponent pgc;

				pgc = (LBPhysicsGameplayComponent)target;

				GUILayout.BeginVertical(EditorStyles.helpBox);
				EditorGUILayout.LabelField("Floor status", pgc.bHasFloor ? "Has floor" : "No floor");
				EditorGUILayout.LabelField("Floor normal", pgc.FloorNormal.ToString());
				GUILayout.EndVertical();
			}

			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				if (Application.isPlaying)
					DisplayPhysicsStats_Game();

				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}