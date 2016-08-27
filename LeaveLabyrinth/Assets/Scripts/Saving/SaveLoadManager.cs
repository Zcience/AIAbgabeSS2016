﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoadManager
{
	private static string boardSaveDirectory = "/BoardSaves/";
	private static string boardSaveFileEnding = ".bs";

	public static void saveBoard (BoardSave toSave, string boardName)
	{
		string fileName = boardSaveDirectory + boardName + boardSaveFileEnding;
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.dataPath + fileName);
		bf.Serialize (file, toSave);
		file.Close ();
	}

	public static BoardSave loadBoard (string boardName)
	{
		string fileName = boardSaveDirectory + boardName + boardSaveFileEnding;
		if (File.Exists (Application.dataPath + fileName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + fileName, FileMode.Open);
			BoardSave toLoad = (BoardSave)bf.Deserialize (file);
			file.Close ();
			return toLoad;
		}
		return null;
	}

	public static string[] getBoardFileNames ()
	{
		checkDirectories ();
		string[] boardFileNames = Directory.GetFiles (Application.dataPath + boardSaveDirectory, "*" + boardSaveFileEnding);
		for (int i = 0; i < boardFileNames.Length; i++) {
			// truncate data path beginning
			int dataPathBeginningLength = (Application.dataPath + boardSaveDirectory).Length;
			boardFileNames [i] = boardFileNames [i].Substring (dataPathBeginningLength);
			// truncate filename ending
			int fileNameLength = boardFileNames [i].Length - boardSaveFileEnding.Length;
			boardFileNames [i] = boardFileNames [i].Substring (0, fileNameLength);
		}
		return boardFileNames;
	}

	private static void checkDirectories ()
	{
		if (!Directory.Exists (Application.dataPath + boardSaveDirectory)) {
			Directory.CreateDirectory (Application.dataPath + boardSaveDirectory);
		}
	}
}
