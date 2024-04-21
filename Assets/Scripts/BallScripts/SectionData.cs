using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionData {

	public SortedDictionary<int, int> ballSections;

	public SectionData()
	{
		ballSections = new SortedDictionary<int, int>();
		ballSections.Add(int.MaxValue, 0);
	}

	public int GetSectionKey(int front)
	{
		int key = int.MaxValue;

		foreach(KeyValuePair<int, int> entry in ballSections)
		{
			if (front >= entry.Value && front <= entry.Key)
				key =  entry.Key;
		}

		return key;
	}

	public void OnAddModifySections(int atIndex)
	{
		List<KeyValuePair<int, int>> modSectionList = new List<KeyValuePair<int, int>>();
		int sectionKey = GetSectionKey(atIndex);

		int sectionKeyVal;
		ballSections.TryGetValue(sectionKey, out sectionKeyVal);

		if (sectionKey != int.MaxValue) {
			int newSectionKey = sectionKey + 1;
			ballSections.Add(newSectionKey, sectionKeyVal);
			ballSections.Remove(sectionKey);

			foreach (KeyValuePair<int, int> entry in ballSections)
			{
				if (entry.Key > newSectionKey)
					modSectionList.Add(entry);
			}

			foreach(KeyValuePair<int, int> entry in modSectionList)
			{
				if (entry.Key != int.MaxValue)
				{
					if (entry.Value == 0)
						ballSections.Add(entry.Key + 1, entry.Value);
					else
						ballSections.Add(entry.Key + 1, entry.Value + 1);

					ballSections.Remove(entry.Key);
				}
				else
					ballSections[entry.Key] = entry.Value + 1;
			}
		}
	}

	public void DeleteEntireSection(int atIndex, int range, int sectionKey, int ballListCount)
	{
		List<KeyValuePair<int, int>> modSectionList = new List<KeyValuePair<int, int>>();

		if (atIndex + range != ballListCount + range)
		{
			ballSections.Remove(sectionKey);

			foreach (KeyValuePair<int, int> entry in ballSections)
			{
				if (entry.Key > sectionKey)
					modSectionList.Add(entry);
			}

			ballSections.Add(modSectionList[0].Key - range, atIndex);
			ballSections.Remove(modSectionList[0].Key);
			modSectionList.RemoveAt(0);

			foreach(KeyValuePair<int, int> entry in modSectionList)
			{
				if (entry.Key != int.MaxValue)
				{
					ballSections.Add(entry.Key - range, entry.Value - range);
					ballSections.Remove(entry.Key);
				}
				else
					ballSections[entry.Key] = entry.Value - range;
			}
		}
		else
		{
			KeyValuePair<int, int> getLastButOne = new KeyValuePair<int, int>();

			if (ballSections.Count > 1)
			{
				foreach (KeyValuePair<int, int> entry in ballSections)
				{
					if (entry.Key < sectionKey)
						getLastButOne = entry;
				}

				ballSections.Remove(getLastButOne.Key);
				ballSections[int.MaxValue] = getLastButOne.Value;
			}
		}
	}

	public void DeletePartialSection(int atIndex, int range, int sectionKey, int sectionKeyVal, int ballListCount)
	{
		List<KeyValuePair<int, int>> modSectionList = new List<KeyValuePair<int, int>>();

		int end = sectionKey == int.MaxValue? ballListCount + range - 1: sectionKey;
		if (atIndex == sectionKeyVal || atIndex + range - 1 == end)
		{
			if (sectionKey == int.MaxValue)
				return;

			int newSectionKey = sectionKey - range;
			ballSections.Add(newSectionKey, sectionKeyVal);
			ballSections.Remove(sectionKey);

			foreach (KeyValuePair<int, int> entry in ballSections)
			{
				if (entry.Key > newSectionKey)
					modSectionList.Add(entry);
			}
		}
		else
		{
			int newSectionKey = atIndex - 1;
			ballSections.Add(newSectionKey, sectionKeyVal);

			if (sectionKey != int.MaxValue)
			{
				int nextSectionKey = sectionKey - range;

				ballSections.Remove(sectionKey);
				ballSections.Add( nextSectionKey, atIndex);

				foreach (KeyValuePair<int, int> entry in ballSections)
				{
					if (entry.Key > nextSectionKey)
						modSectionList.Add(entry);
				}
			}
			else
			{
				ballSections[int.MaxValue] = atIndex;
				return;
			}
		}

		modSectionList.Sort((entryA, entryB) => entryA.Key.CompareTo(entryB.Key));

		foreach(KeyValuePair<int, int> entry in modSectionList)
		{
			if (entry.Key != int.MaxValue)
			{
				if (entry.Value == 0)
					ballSections.Add(entry.Key - range, entry.Value);
				else
					ballSections.Add(entry.Key - range, entry.Value - range);

				ballSections.Remove(entry.Key);
			}
			else
				ballSections[entry.Key] = entry.Value - range;
		}
	}
}
