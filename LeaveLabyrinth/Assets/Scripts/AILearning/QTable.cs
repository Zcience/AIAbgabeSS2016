﻿using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Quality Table. Maps states to actions and their corresponding qualities.
/// </summary>
public class QTable
{
	public Dictionary<uint , List<AQTuple>> m_DataTable{ get; private set; }

	public int m_NumberOfUpdates{ get; private set; }

	public string m_Name { get; set; }

	private Random m_Random;

	private const float EPSILON = 0.00001f;

	public QTable (string name)
	{
		m_DataTable = new Dictionary<uint,List<AQTuple>> ();
		m_Name = name;

		m_Random = new Random ();
	}

	/// <summary>
	/// Adds the state.
	/// </summary>
	/// <returns><c>true</c>, if state was added, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	public bool addState (uint state)
	{
		try {
			m_DataTable.Add (state, new List<AQTuple> ());
			return true;
		} catch (ArgumentException) {
			return false;
		}
	}

	/// <summary>
	/// Adds the action to the specified state
	/// </summary>
	/// <returns><c>true</c>, if action was added, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="actionID">Action</param>
	public bool addAction (uint state, int actionID)
	{
		return addAction (state, actionID, 0);
	}

	/// <summary>
	/// Adds the action to the specified state with the specified quality
	/// </summary>
	/// <returns><c>true</c>, if action was added, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="actionID">Action I.</param>
	/// <param name="quality">Quality.</param>
	public bool addAction (uint state, int actionID, float quality)
	{
		List<AQTuple> aqList;
		bool hasState = m_DataTable.TryGetValue (state, out aqList);
		if (hasState) {
			if (!containsAction (actionID, aqList)) {
				aqList.Add (new AQTuple (actionID, quality));
			}
		}
		return hasState;
	}

	/// <summary>
	/// Gets the quality of the specified action in the specified state
	/// </summary>
	/// <returns><c>true</c>, if action quality could be found, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="actionID">Action I.</param>
	/// <param name="quality">Quality.</param>
	public bool getActionQuality (uint state, int actionID, out float quality)
	{
		AQTuple aq;
		bool foundAction = findActionQuality (state, actionID, out aq);
		if (foundAction) {
			quality = aq.m_Quality;
		} else {
			quality = 0f;
		}
		return foundAction;
	}

	/// <summary>
	/// Set the action-quality at the specified state.
	/// </summary>
	/// <returns><c>true</c>, if action quality was set, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="actionID">Action I.</param>
	/// <param name="quality">Quality.</param>
	public bool setActionQuality (uint state, int actionID, float quality)
	{
		m_NumberOfUpdates++;

		// add the state, if it doesn't exist yet
		bool hasState = m_DataTable.ContainsKey (state);
		if (!hasState) {
			addState (state);
		}

		AQTuple aq;
		bool ableToSetAQ = findActionQuality (state, actionID, out aq);
		// Action was already added to state
		if (ableToSetAQ) {
			aq.m_Quality = quality;
		} else {
			// try to add action to state
			ableToSetAQ = addAction (state, actionID, quality);
		}
		return ableToSetAQ;
	}

	/// <summary>
	/// Return a random state in the Table as reference
	/// </summary>
	/// <returns><c>true</c>, if a random state could be found, <c>false</c> otherwise.</returns>
	/// <param name="state">State reference.</param>
	public bool randomState (out uint state)
	{
		state = 0U;

		int stateCount = m_DataTable.Count;
		if (0 == stateCount) {
			return false; // no states to return
		}
		;
		int randomStateNumber = m_Random.Next (stateCount);

		int i = 0;
		foreach (uint s in m_DataTable.Keys) {
			if (i == randomStateNumber) {
				state = s;
				break;
			}
			i++;
		}
		return true; // found a random state
	}

	/// <summary>
	/// Returns a random action, that's possible in the specified state, as reference
	/// </summary>
	/// <returns><c>true</c>, if a random action could be returned, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="actionID">Action I.</param>
	public bool randomAction (uint state, out int actionID)
	{
		actionID = -1; // init with invalid actionID

		List<AQTuple> aqList;
		bool hasState = m_DataTable.TryGetValue (state, out aqList);
		if (hasState) {
			int actionCount = aqList.Count;

			if (0 == actionCount) {
				return false; // no actions available
			}

			int randomActionNumber = m_Random.Next (actionCount);

			actionID = aqList [randomActionNumber].m_ActionID;
		}
		return hasState;
	}

	/// <summary>
	/// Returns the best action in the state as reference
	/// </summary>
	/// <returns><c>true</c>, if best action could be found, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="actionID">Action I.</param>
	public bool getBestAction (uint state, out int actionID)
	{
		float unusedQuality;
		return getBestAction (state, out actionID, out unusedQuality);
	}

	/// <summary>
	/// Returns best quality of all the actions in the specified state as reference
	/// </summary>
	/// <returns><c>true</c>, if best action quality could be found, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="quality">Quality.</param>
	public bool getBestActionQuality (uint state, out float quality)
	{
		int unusedActionID;
		return getBestAction (state, out unusedActionID, out quality);
	}

	/// <summary>
	/// Returns the best action in the state and the corresponding quality as reference
	/// </summary>
	/// <returns><c>true</c>, if best action quality could be found, <c>false</c> otherwise.</returns>
	/// <param name="state">State.</param>
	/// <param name="actionID">Action I.</param>
	/// <param name="quality">Quality.</param>
	public bool getBestAction (uint state, out int actionID, out float quality)
	{
		
		actionID = -1; // init with invalid value
		quality = 0f; // return quality 0, if we don't find a best action

		List<AQTuple> aqList;
		bool hasState = m_DataTable.TryGetValue (state, out aqList);
		if (hasState) {
			int actionCount = aqList.Count;

			if (0 == actionCount) {
				return false; // no actions available
			}
				
			// Get all the AQ Tuples with the best quality
			List<AQTuple> bestQualities = new List<AQTuple> ();
			bestQualities.Add (aqList [0]);

			foreach (AQTuple aq in aqList) {
				// if we found a better quality, clear the current best qualities and add the new best
				if (aq.m_Quality > bestQualities [0].m_Quality) {
					bestQualities.Clear ();
					bestQualities.Add (aq);
				} 
				// if we found an equally good quality, add it to the list
				else {
					float aqDiff = (aq.m_Quality - bestQualities [0].m_Quality);
					if (aqDiff < EPSILON && aqDiff > -EPSILON) {
						bestQualities.Add (aq);
					}
				}
			}

			// Get a random action out of the best qualities
			int randomBestQualityIndex = m_Random.Next (bestQualities.Count);
			actionID = bestQualities [randomBestQualityIndex].m_ActionID;
			quality = bestQualities [randomBestQualityIndex].m_Quality;
		}
		return hasState;
	}

	/// <summary>
	/// Reset everything, that was learned
	/// </summary>
	public void reset ()
	{
		m_NumberOfUpdates = 0;
		m_DataTable.Clear ();
	}


	/** >>> convenience-methods <<<  */

	private bool containsAction (int actionID, List<AQTuple> aqList)
	{
		foreach (AQTuple aq in aqList) {
			if (actionID == aq.m_ActionID) {
				return true;
			}
		}
		return false;
	}

	private bool findActionQuality (uint state, int actionID, out AQTuple actionQuality)
	{
		actionQuality = null;

		List<AQTuple> aqList;
		bool hasState = m_DataTable.TryGetValue (state, out aqList);

		if (hasState) {
			if (0 == aqList.Count) {
				return false;
			}

			foreach (AQTuple aq in aqList) {
				if (actionID == aq.m_ActionID) {
					actionQuality = aq;
					return true; // found action with specified id
				}
			}
		}
		actionQuality = null;
		return false; // didn't find state or the specified action
	}

	public override string ToString ()
	{
		// init
		short row, column;
		List<AQTuple> aqList;

		string toReturn = "Qtable: " + m_Name + "\n";
		foreach (uint state in m_DataTable.Keys) {
			StateConversion.convertFromState (state, out row, out column);
			toReturn += "(" + row + "," + column + ") "; // print state

			m_DataTable.TryGetValue (state, out aqList); // get actions
			aqList.Sort ();
			foreach (AQTuple aq in aqList) {
				toReturn += aq.ToString () + " "; // print actions
			}
			toReturn += "\n"; // next line
		}

		toReturn += "\nNumber of Updates: " + m_NumberOfUpdates;
		return toReturn;
	}
}
