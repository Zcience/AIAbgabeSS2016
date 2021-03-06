﻿using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// One Field with a reward and accessability. Used as visual representation of a state in the 
/// Reinforcement-Learning-AI
/// </summary>
[RequireComponent (typeof(TextMesh))]
public class Field : MonoBehaviour
{
	public Field[] m_Neighbours{ get; set; }

	private float m_Reward = 0f;
	// only just found out, that get/set works like this......... I'M PRACTICALLY DONE
	public float M_Reward { 
		get {
			return m_Reward;
		}
		set {
			m_Reward = value;
			updateColor ();
		}
	}

	private bool m_IsAccessible = true;

	public bool M_IsAccessible {
		get {
			return m_IsAccessible;
		}
		set {
			m_IsAccessible = value;
			updateColor ();
		}
	}

	private bool m_IsMarkedForDelete = false;

	/// <summary>
	/// Only used, to show the current quality to this state as stored in the QTable
	/// </summary>
	private float m_CurrentQuality;

	[SerializeField]
	private Color m_MarkedForDeleteColor = new Color (1f, 0f, 0f);
	[SerializeField]
	private Color m_SelectedColor = new Color (0f, 1f, 1f);
	[SerializeField]
	private Color m_InitalColor = new Color (1f, 1f, 1f);
	[SerializeField]
	private Color m_InaccessibleColor = new Color (0f, 0f, 0f);

	private Color m_CurrentColor = new Color (1f, 1f, 1f);

	private bool m_IsSelected = false;

	public TextMesh m_QualityText{ get; set; }

	public Field () : this (0, true)
	{
	}

	public Field (float reward, bool accessible)
	{
		m_Reward = reward;
		m_IsAccessible = accessible;

		m_Neighbours = new Field[FieldManager.AVAILABLE_ACTION_IDS.Length];
	}

	void Start ()
	{
		m_QualityText = gameObject.GetComponentInChildren<TextMesh> ();
		updateColor ();
	}

	public Field getNeighbour (int actionID)
	{
		try {
			return m_Neighbours [actionID];
		} catch (IndexOutOfRangeException e) {
			Debug.Log (e.Message);
			return null;
		}
	}

	public bool registerNeighbour (Field newNeighbour, int actionID)
	{
		try {
			// register neighbour
			m_Neighbours [actionID] = newNeighbour;
		} catch (ArgumentOutOfRangeException e) {
			Debug.Log (e.Message);
			return false;
		}

		return true;
	}

	public void OnMarkForDelete ()
	{
		m_IsMarkedForDelete = true;
		updateColor ();
	}

	public void OnUnmarkForDelete ()
	{
		m_IsMarkedForDelete = false;
		updateColor ();
	}

	public void setSelected (bool selected)
	{
		m_IsSelected = selected;
		updateColor ();
	}

	/// <summary>
	/// This should just be used to display the current quality to the corresponding state of this field
	///  on the surface of the field. This quality could differ to the quality stored in the actual AI-Quality-Table
	/// </summary>
	/// <returns>The current quality.</returns>
	public float getCurrentQuality ()
	{
		return m_CurrentQuality;
	}

	public void setCurrentQuality (float currentQuality)
	{
		m_CurrentQuality = currentQuality;
		m_QualityText.text = currentQuality.ToString ("0.000");
	}

	void OnMouseUpAsButton ()
	{
		FieldModifier.onClickField (this);
	}

	void OnDestroy ()
	{
		m_Neighbours = null;
	}


	// dis ugly
	private void updateColor ()
	{
		if (m_IsAccessible) {
			float colorValue = Mathf.Min (1.0f, Mathf.Abs (m_Reward));
			// low values shouldn't be black --> map value from 0.5f to 1.f
			colorValue = 0.3f + colorValue * 0.7f;
			if (m_Reward > 0f) {
				m_CurrentColor = new Color (0f, colorValue, 0f);
			} else if (m_Reward < 0f) {
				m_CurrentColor = new Color (colorValue, 0f, 0f);
			} else {
				m_CurrentColor = m_InitalColor;
			}
		} else {
			m_CurrentColor = m_InaccessibleColor;
		}

		if (m_IsSelected) {
			m_CurrentColor = m_SelectedColor;
		}
		if (m_IsMarkedForDelete) {
			m_CurrentColor = m_MarkedForDeleteColor;
		}
		GetComponent<Renderer> ().material.color = m_CurrentColor;
	}
}
