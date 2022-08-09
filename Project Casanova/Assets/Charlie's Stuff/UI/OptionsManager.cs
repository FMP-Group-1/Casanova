using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
	[SerializeField]
	private Slider xCameraSensitivity;
	[SerializeField]
	private Slider yCameraSensitivity;
	[SerializeField]
	private Slider audioVolume;

	private void Start()
	{

	}
	private void OnEnable()
	{

		xCameraSensitivity.minValue = Settings.g_minXCameraSensitiviy;
		xCameraSensitivity.maxValue = Settings.g_maxXCameraSensitiviy;
		yCameraSensitivity.minValue = Settings.g_minYCameraSensitiviy;
		yCameraSensitivity.maxValue = Settings.g_maxYCameraSensitiviy;

		RefreshSliders();
	}

	public void RefreshSliders()
	{

		xCameraSensitivity.value = Settings.g_currentXCameraSensitiviy;
		yCameraSensitivity.value = Settings.g_currentYCameraSensitiviy;
	}

	public Vector2 GetSensitivity()
	{
		return new Vector2(xCameraSensitivity.value, yCameraSensitivity.value);
	}

}
