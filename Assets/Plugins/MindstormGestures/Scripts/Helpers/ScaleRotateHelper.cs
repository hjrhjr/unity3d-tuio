/*
Unity3d-TUIO connects touch tracking from a TUIO to objects in Unity3d.

Copyright 2011 - Mindstorm Limited (reg. 05071596)

Author - Bertrand Nouvel and Simon Lerpiniere

This file is part of Unity3d-TUIO.

Unity3d-TUIO is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Unity3d-TUIO is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser Public License for more details.

You should have received a copy of the GNU Lesser Public License
along with Unity3d-TUIO.  If not, see <http://www.gnu.org/licenses/>.

If you have any questions regarding this library, or would like to purchase 
a commercial licence, please contact Mindstorm via www.mindstorm.com.
*/

using UnityEngine;
using System.Collections;

/// <summary>
/// Applies scale, rotate and translate to transform based on an initial position.
/// </summary>
public class ScaleRotateHelper : MonoBehaviour
{
	/// <summary>
	/// Whether or not a move has been started (and not ended)
	/// </summary>
	public bool IsMoving = false;
	
	/// <summary>
	/// Restrict scaling to specific axis.
	/// </summary>
	public Vector3 scaleAxis = Vector3.up.InvertAxis();
	
	/// <summary>
	/// Restrict movement to a specific axis (by default, x and z).
	/// </summary>
	public Vector3 moveAxis = Vector3.up.InvertAxis();
	
	/// <summary>
	/// The axis around which the object will be rotated.
	/// </summary>
	public Vector3 rotateAxis = Vector3.up;
	
	/// <summary>
	/// Minimum scale the object can be (absolute)
	/// </summary>
	public Vector3 maxScale = Vector3.one * 3f;
	
	/// <summary>
	/// Maximum scale the object can be (absolute)
	/// </summary>
	public Vector3 minScale = Vector3.one * 0.5f;
	
	/// <summary>
	/// How much damping is applied to smooth the movement.
	/// </summary>
	public float DampingSpeed = 0.05f;
		
	Vector3 A; // first touch group
	Vector3 B; // second touch group
	
	Vector3 A0Pos;
	Vector3 A0B0;
	
	Vector3 iscale;
	Vector3 iposition;
	Quaternion irotation;
		
	Vector3 targetPos = Vector3.zero;
	Quaternion targetRot = Quaternion.identity;
	float targetScale = 1f;
	
	Vector3 cPos = Vector3.zero;
	Quaternion cRot = Quaternion.identity;
	float cScale = 1f;
	
	/// <summary>
	/// Save initial configuration of control & manipulated objects
	/// </summary>
	public void StartMove(Vector3 pos1) 
	{
		StartMove(pos1, pos1);
	}
	
	/// <summary>
	/// Save initial configuration of control & manipulated objects
	/// </summary>
	public void StartMove(Vector3 pos1, Vector3 pos2) 
	{
		A = pos1;
		B = pos2;
		
		A0Pos = new Vector3(A.x, transform.position.y, A.z);
		A0B0=B-A;
		
		iscale=transform.localScale;
		irotation=transform.rotation;
		iposition=transform.position;
		
		resetTargets();
		
		IsMoving = true;
	}
	
	/// <summary>
	/// Look at the position of A & B and apply the same transformation to T
	/// </summary>
	public void UpdateMove(Vector3 pos1) 
	{
		A = pos1;
		B = pos1;
		
		Vector3 trans; 
		Quaternion rotat;
		float scale;
		
		ComputeParameters(out trans, out  rotat, out scale);
		targetPos = trans;
		targetScale = 1f;
		targetRot = Quaternion.identity;
	}
	
	/// <summary>
	/// Look at the position of A & B and apply the same transformation to T
	/// </summary>
	public void UpdateMove(Vector3 pos1, Vector3 pos2) 
	{
		A = pos1;
		B = pos2;
		
		Vector3 trans; 
		Quaternion rotat;
		float scale;
		
		ComputeParameters(out trans, out  rotat, out scale);
		targetPos = trans;
		targetRot = rotat;
		targetScale = scale;
	}
	
	/// <summary>
	/// Performs interpolation to the target values
	/// </summary>
	public void LateUpdate()
	{
		if (!IsMoving) return;
		
		transform.position=iposition;
		transform.rotation=irotation;
		transform.localScale=iscale;
		
		cPos = Vector3.Lerp(cPos, targetPos, Time.deltaTime / DampingSpeed);
		cRot = Quaternion.Lerp(cRot, targetRot, Time.deltaTime / DampingSpeed);
		float tmpScale = Mathf.Lerp(cScale, targetScale, Time.deltaTime / DampingSpeed);
		
		float newScale = (transform.localScale * tmpScale).magnitude;
		if (newScale <= maxScale.magnitude && newScale >= minScale.magnitude) cScale = tmpScale;
		
		transform.RotateAround(A0Pos, rotateAxis, cRot.eulerAngles.y);		
		transform.ScaleAround(A0Pos, cScale, scaleAxis);
		transform.position += cPos.Constrain(moveAxis);
	}
	
	/// <summary>
	/// Stop all movement.
	/// </summary> 
	public void EndMove() 
	{
		resetTargets();
		IsMoving = false;
	}
	
	void resetTargets()
	{
		targetPos = Vector3.zero;
		targetScale = 1f;
		targetRot = Quaternion.identity;	
		
		cPos = Vector3.zero;
		cScale = 1f;
		cRot = Quaternion.identity;
	}
	
	/// <summary>
	/// Computes a scale, rotate and transform as an absolute from the initial position.
	/// </summary>
	void ComputeParameters(out Vector3 trans, out Quaternion rotat, out float scale) 
	{
		Vector3 A1B1 = B - A;
		trans = A - A0Pos;
		rotat = Quaternion.FromToRotation(A0B0, A1B1);
		scale = A1B1.magnitude / A0B0.magnitude;
	}
}