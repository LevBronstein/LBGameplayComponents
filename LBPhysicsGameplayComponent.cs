using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LBGameplay
{
	public static class LBMath
	{
		public static float TruncFloat(float f, byte numbers = 3)
		{
			return (float)System.Math.Truncate(f * (int)System.Math.Pow(10, numbers)) / (int)System.Math.Pow(10, numbers);
		}

		public static Vector3 TruncVector3(Vector3 v, byte numbers = 3)
		{
			return new Vector3(TruncFloat(v.x), TruncFloat(v.y), TruncFloat(v.z));
		}
		
		public static float LerpFloat(float current, float target, float step, float dt)
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

		public static int FindClosetVector(Vector3 vect, params Vector3[] vectors)
		{
			float dot, max_dot;
			int i, max_idx;

			if (vectors.Length == 0)
				return -1; //unidentified

			max_dot = Vector3.Dot(vect.normalized, vectors[0]);
			max_idx = 0;

			for (i = 1; i < vectors.Length; i++)
			{
				dot = Vector3.Dot(vect.normalized, vectors[i].normalized);

				if (dot > max_dot) // < 0?
				{
					max_dot = dot;
					max_idx = i;
				}
			}

			return max_idx;
		}

		public static int FindNonZeroPlaneProjection(Vector3 plane_normal, params Vector3[] vectors)
		{
			float proj;
			int i;

			for (i = 0; i < vectors.Length; i++)
			{
				proj = Vector3.ProjectOnPlane(vectors[i], plane_normal).magnitude;

				if (proj > 0)
					return i;
			}

			return -1;
		}

		public static int FindBiggestPlaneProjection(Vector3 plane_normal, params Vector3[] vectors)
		{
			float proj, max_proj;
			int i, max_idx;

			if (vectors.Length == 0)
				return -1; //unidentified

			max_proj = Vector3.ProjectOnPlane(vectors[0], plane_normal).magnitude;
			max_idx = 0;

			for (i = 1; i < vectors.Length; i++)
			{
				proj = Vector3.ProjectOnPlane(vectors[i], plane_normal).magnitude;

				if (proj > max_proj) // < 0?
				{
					max_proj = proj;
					max_idx = i;
				}
			}

			return max_idx;
		}

		/// <summary>
		/// Returns three vectors right-up-forward as a projection of gloabl coordinate system
		/// </summary>
		//public static Vector3[] GlobalProjection(Vector3 surface_normal)
		//{
		//	Vector3 v;
		//	Vector3[] basis = new Vector3[3];

		//	basis[1] = surface_normal;

		//	v = Vector3.ProjectOnPlane(Vector3.forward, surface_normal);

		//	if (LBMath.TruncFloat(v.magnitude) > 0) // normalized?
		//	{
		//		basis[0] = v.normalized;
		//	}
		//	else
		//	{
		//		v = Vector3.ProjectOnPlane(Vector3.right, surface_normal);

		//		if (LBMath.TruncFloat(v.magnitude) > 0)
		//		{
		//			basis[0] = v.normalized;
		//		}
		//		else
		//		{
		//			v = Vector3.ProjectOnPlane(Vector3.up, surface_normal);

		//			if (LBMath.TruncFloat(v.magnitude) > 0)
		//			{
		//				basis[0] = v.normalized;
		//			}
		//			else
		//			{
		//				// what?!
		//			}
		//		}
		//	}

		//	//basis[2] = v;

		//	basis[2] = Vector3.Cross(basis[1], basis[0]).normalized;

		//	return basis;
		//}

		public static Vector3[] GlobalProjection(Vector3 surface_normal)
		{
			Vector3 local_up, local_forward, local_right;
			//List<Vector3> vectors = new List<Vector3>();
			//Vector3[] basis = new Vector3[3];
			//Vector3 v;
			//int i;

			//vectors.AddRange(new Vector3[] { Vector3.right, Vector3.up });

			//i = FindNonZeroPlaneProjection(surface_normal, vectors.ToArray());

			//basis[0] = vectors[i];
			//basis[1] = surface_normal;
			//basis[2] = Vector3.Cross(basis[0], basis[1]).normalized;

			//return basis;

			local_up = surface_normal.normalized;

			local_forward = Vector3.ProjectOnPlane(Vector3.forward, surface_normal);

			if (local_forward.magnitude >= 0.05f) // if projection is not ~zero
			{
				local_right = Vector3.Cross(local_up, local_forward);

				if (Vector3.Dot(local_up, Vector3.up) < 0) // upper hemisphere (all directions inverted)
				{
					local_forward = -local_forward;
					local_right = -local_right;
				}
			}
			else
			{
				if (Vector3.Dot(local_up, Vector3.up) < 0) // upper hemisphere (all directions inverted)
				{
					local_forward = -Vector3.up;
					local_right = -Vector3.right;
				}
				else
				{
					local_forward = Vector3.up;
					local_right = Vector3.right;
				}
			}

			return new Vector3[] { local_right.normalized, local_up, local_forward.normalized };
		}

		public static Vector3 TransformPointToGlobal(Vector3 v, Vector3[] axes)
		{ 
			return v.x * axes[0] + v.y * axes[1] + v.z * axes[2];
		}


	}

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

		public float ClampValue(float value)
		{
			float a, b;

			if (b_inner_interval)
			{
				a = IntervalStart;

				if (b_strict_a)
					a = a + 0.001f;

				b = IntervalEnd;

				if (b_strict_b)
					b = b - 0.001f;

				return Mathf.Clamp(value, a, b);
			}
			else
			{
				if (value < IntervalStart)
				{
					return value;
				}
				else if (value == IntervalStart)
				{
					if (b_strict_a)
						return IntervalStart - 0.001f;
					else
						return value;
				}
				else if (value > IntervalStart && value < IntervalEnd)
				{
					if (Mathf.Abs(value - IntervalStart) <= Mathf.Abs(value - IntervalEnd))
					{
						if (b_strict_a)
							return IntervalStart - 0.001f; 
						else
							return IntervalStart;
					}
					else
					{
						if (b_strict_b)
							return IntervalEnd + 0.001f;
						else
							return IntervalEnd;
					}
				}
				else if (value == IntervalEnd)
				{
					if (b_strict_b)
						return IntervalEnd + 0.001f;
					else
						return value;
				}
				else // value > IntervalEnd
				{
					return value;
				}
			}
		}

		public float Percentage(float value)
		{
			if (CheckValue(value))
			{
				return value / IntervalEnd;
			}
			else
				return 0;
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
				return (bracket_a + IntervalStart.ToString() + "; " + IntervalEnd.ToString() + bracket_b);
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
		private ContactPoint[] wall_points;

		protected LBFloatInverval floor_normal = new LBFloatInverval(-120, 120, false);

		protected override bool Init()
		{
			if (!base.Init())
				return false;

			target_rigidbody = GetComponent<Rigidbody>();
			target_collider = GetComponent<Collider>(); // what if we've got several of them?

			floor_points = new ContactPoint[0];
			wall_points = new ContactPoint[0];

			if (target_rigidbody == null)
				return false;

			return true;
		}

		protected virtual void PerformPhysics() { }

		// Update is called once per physics tick
		void FixedUpdate()
		{
			if (state == LBGameplayComponentState.Active)
			{

				// update floor info each physics tick
				//floor_points = GetAllFloorPoints(collisions.ToArray());
				FilterCollisions();

				//PerformPhysics();

				Perform();
			}
		}

		void Update()
		{
			//Perform();
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

			Debug.Log(">>>>>>>>>>> GEMOR: got new collision OnCollisionStay! Are we rotating? <<<<<<<<<<<<<<<<");
			return false;
		}

		protected void DrawContactPoint(ContactPoint pt)
		{
			Debug.DrawLine(pt.point, pt.point + pt.normal.normalized * 0.1f, Color.blue); 
		}

		// Gets the closest (by angle) floor point
		protected ContactPoint GetFloorPoint(ContactPoint[] pts)
        {
			int i, min = 0;
			float angle, min_angle;
			ContactPoint pt;

			pt = new ContactPoint();

			if (pts == null || pts.Length == 0)
				return pt;

			min_angle = Vector3.SignedAngle(pts[0].normal, Gravity, Vector3.up);
			DrawContactPoint(pts[0]);

			for (i = 1; i < pts.Length; i++)
            {
				angle = Vector3.SignedAngle(pts[i].normal, Gravity, Vector3.up);

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
			int CompareByAngle(ContactPoint pt1, ContactPoint pt2)
			{
				float a1, a2;

				a1 = Vector3.SignedAngle(pt1.normal, Gravity, Vector3.up);
				a2 = Vector3.SignedAngle(pt2.normal, Gravity, Vector3.up);

				if (a1 - a2 > 0)
					return 1;
				else if (a1 - a2 == 0)
					return 0;
				else
					return -1;
			}

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
			
			floor_pts.Sort(CompareByAngle);

			//if (floor_pts.Count > 1)
			//{
			//	min_angle = Vector3.SignedAngle(floor_pts[0].normal, Gravity, Vector3.up);
			//	min = 0;

			//	for (i = 0; i < floor_pts.Count; i++)
			//	{
			//		for (j = i; j < floor_pts.Count; j++)
			//		{
			//			angle = Vector3.SignedAngle(floor_pts[j].normal, Gravity, Vector3.up);

			//			if (angle < min_angle)
			//			{
			//				min = j;
			//				min_angle = angle;
			//			}
			//		}

			//		c = floor_pts[i];
			//		floor_pts[i] = floor_pts[min];
			//		floor_pts[min] = c;
			//	}
			//}

			return floor_pts.ToArray();
        }

		protected virtual ContactPoint[] SelectFloorPoints(ContactPoint[] all_contacts)
		{
			void debug_draw_contactpoint(ContactPoint pt, Color clr)
			{
				Debug.DrawLine(pt.point, pt.point + pt.normal.normalized * 0.1f, clr);
			}

			int compare_by_sangle_asc(ContactPoint pt1, ContactPoint pt2)
			{
				float a1, a2;

				a1 = Vector3.SignedAngle(pt1.normal, Gravity, Vector3.up);
				a2 = Vector3.SignedAngle(pt2.normal, Gravity, Vector3.up);

				if (a1 - a2 > 0)
					return 1;
				else if (a1 - a2 == 0)
					return 0;
				else
					return -1;
			}

			int i;
			float angle;
			List<ContactPoint> points_list;

			points_list = new List<ContactPoint>();

			if (all_contacts == null || all_contacts.Length == 0)
				return new ContactPoint[0];

			// first -- we select all points, that comply to the @floor_normal condition
			for (i = 0; i < all_contacts.Length; i++)
			{
				angle = Vector3.SignedAngle(all_contacts[i].normal, Gravity, Vector3.up);

				if (floor_normal.CheckValue(angle))
				{
					points_list.Add(all_contacts[i]);
					debug_draw_contactpoint(all_contacts[i], Color.green);
				}
			}

			points_list.Sort(compare_by_sangle_asc);

			return points_list.ToArray();
		}

		protected virtual ContactPoint[] SelectWallPoints(ContactPoint[] all_contacts)
		{
			void debug_draw_contactpoint(ContactPoint pt, Color clr)
			{
				Debug.DrawLine(pt.point, pt.point + pt.normal.normalized * 0.1f, clr);
			}

			int compare_by_sangle_asc(ContactPoint pt1, ContactPoint pt2)
			{
				float a1, a2;

				a1 = Vector3.SignedAngle(pt1.normal, Gravity, Vector3.up);
				a2 = Vector3.SignedAngle(pt2.normal, Gravity, Vector3.up);

				if (a1 - a2 > 0)
					return 1;
				else if (a1 - a2 == 0)
					return 0;
				else
					return -1;
			}

			int i;
			float angle;
			List<ContactPoint> points_list;

			points_list = new List<ContactPoint>();

			if (all_contacts == null || all_contacts.Length == 0)
				return new ContactPoint[0];

			// first -- we select all points, that DO NOT comply to the @floor_normal condition
			for (i = 0; i < all_contacts.Length; i++)
			{
				angle = Vector3.SignedAngle(all_contacts[i].normal, Gravity, Vector3.up);

				// basic scheme -- detect ANY wallk (including ceiling)
				if (!floor_normal.CheckValue(angle))
				{
					points_list.Add(all_contacts[i]);
					debug_draw_contactpoint(all_contacts[i], Color.blue);
				}
			}

			// do we really need to sort the array?
			points_list.Sort(compare_by_sangle_asc);

			return points_list.ToArray();
		}

		protected void FilterCollisions()
		{
			void debug_draw_contactpoint(ContactPoint pt, Color clr)
			{
				Debug.DrawLine(pt.point, pt.point + pt.normal.normalized * 0.1f, clr);
			}

			int compare_by_sangle_asc(ContactPoint pt1, ContactPoint pt2)
			{
				float a1, a2;

				a1 = Vector3.SignedAngle(pt1.normal, Gravity, Vector3.up);
				a2 = Vector3.SignedAngle(pt2.normal, Gravity, Vector3.up);

				if (a1 - a2 > 0)
					return 1;
				else if (a1 - a2 == 0)
					return 0;
				else
					return -1;
			}

			ContactPoint find_floor_point(ContactPoint[] pts)
			{
				int j;
				float angle;
				ContactPoint pt;
				List<ContactPoint> points_list;

				points_list = new List<ContactPoint>();

				if (pts == null || pts.Length == 0)
					return new ContactPoint();

				// first -- we select all points, that comply to the @floor_normal condition
				for (j = 0; j < pts.Length; j++)
				{
					angle = Vector3.SignedAngle(pts[j].normal, Gravity, Vector3.up);
					
					if (floor_normal.CheckValue(angle))
						points_list.Add(pts[j]);
				}

				if (points_list.Count == 0)
					return new ContactPoint();

				points_list.Sort(compare_by_sangle_asc);

				return points_list.ToArray()[0];
			}

			ContactPoint[] contacts;
			ContactPoint c;
			List<ContactPoint> all_pts;
			int i;

			all_pts = new List<ContactPoint>();
			contacts = new ContactPoint[0];

			//for (i = 0; i < collisions.Count; i++)
			//{
			//	if (collisions[i].contactCount > 0)
			//	{
			//		contacts = new ContactPoint[0];
			//		System.Array.Resize(ref contacts, collisions[i].contactCount);
			//		collisions[i].GetContacts(contacts);
			//		c = find_floor_point(contacts);

			//		if (c.otherCollider != null)
			//		{
			//			floor_pts.Add(c);
			//			debug_draw_contactpoint(c, Color.blue);
			//		}
			//	}
			//}

			// first we merge all contact points into one big list
			for (i = 0; i < collisions.Count; i++)
			{
				if (collisions[i].contactCount > 0)
				{
					contacts = new ContactPoint[0];
					System.Array.Resize(ref contacts, collisions[i].contactCount);
					collisions[i].GetContacts(contacts);
					all_pts.AddRange(contacts);
				}
			}

			floor_points = SelectFloorPoints(all_pts.ToArray());
			wall_points = SelectWallPoints(all_pts.ToArray());
		}

		public virtual Vector3 Gravity
		{
			get
			{
				return Physics.gravity;
			}
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

		public ContactPoint FloorPoint
		{
			get
			{
				// return only the most suitable floor point's normal
				if (floor_points != null && floor_points.Length > 0)
					return floor_points[0];
				else
					return new ContactPoint();
			}
		}

		public ContactPoint[] AllFloorPoints
		{
			get
			{
				if (floor_points != null && floor_points.Length > 0)
					return floor_points;
				else
					return new ContactPoint[0];
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

		public ContactPoint[] AllWallPoints
		{
			get
			{
				// return only the most suitable floor point's normal
				if (wall_points != null && wall_points.Length > 0)
					return wall_points;
				else
					return new ContactPoint[0];
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

		protected Vector3 TruncVector3(Vector3 v, byte numbers = 3)
		{
			return new Vector3(TruncFloat(v.x),TruncFloat(v.y),TruncFloat(v.z));
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

		protected Vector3 GetNonParallelVector(Vector3 v, params Vector3[] vectors)
		{
			int i;
			float eps = 0.01f;

			if (v == Vector3.zero)
			{
				if (vectors.Length > 0)
					return vectors[0];
				else
					return Vector3.up;
			}

			for (i = 0; i < vectors.Length; i++)
			{
				if (Vector3.Angle(v, vectors[i]) > eps)
					return vectors[i];
			}

			if (Vector3.Angle(v, Vector3.up) > eps)
				return Vector3.up;
			else if (Vector3.Angle(v, Vector3.right) > eps)
				return Vector3.right;
			else
				return Vector3.forward;
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