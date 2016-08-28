﻿using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// Data-class for a Field on a Board.
/// </summary>
[RequireComponent (typeof(TextMesh))]
public class Field : MonoBehaviour
{
	private float m_Reward = 0f;
	// only just found out, that get/set works like this......... I'M PRACTICALLY DONE
	public float M_Reward { 
		get {
			return m_Reward;
		}
		set {
			updateColor (value);
			m_Reward = value;
		}
	}

	public bool m_IsAccessible{ get; set; }

	public Field[] m_Neighbours{ get; set; }

	private float m_CurrentQuality;

	[SerializeField]
	private Color m_MarkedForDeleteColor = new Color (1f, 0f, 0f);

	[SerializeField]
	private Color m_InitialColor = new Color (1f, 1f, 1f);

	[SerializeField]
	private Color m_SelectedColor = new Color (0f, 1f, 1f);

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
		updateColor (m_Reward);
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
		GetComponent<Renderer> ().material.color = m_MarkedForDeleteColor;
	}

	public void OnUnmarkForDelete ()
	{
		GetComponent<Renderer> ().material.color = m_InitialColor;
	}

	public void OnSwitchSelect ()
	{
		m_IsSelected = !m_IsSelected;
		if (m_IsSelected) {
			GetComponent<Renderer> ().material.color = m_SelectedColor;
		} else {
			GetComponent<Renderer> ().material.color = m_InitialColor;
		}
	}

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


	private void updateColor (float value)
	{
		float colorValue = Mathf.Min (1.0f, Mathf.Abs (value));
		// low values shouldn't be black --> map value from 0.5f to 1.f
		colorValue = 0.3f + colorValue * 0.7f;
		if (value > 0f) {
			m_InitialColor = new Color (0f, colorValue, 0f);
		} else if (value < 0f) {
			m_InitialColor = new Color (colorValue, 0f, 0f);
		} else {
			m_InitialColor = Color.white;
		}
		GetComponent<Renderer> ().material.color = m_InitialColor;
	}
}
