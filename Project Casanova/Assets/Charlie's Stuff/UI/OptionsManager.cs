using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
	[SerializeField]
	private Slider m_cameraSensitivitySlider;
	[SerializeField]
	private Slider audioVolume;

	private void Start()
	{

	}
	private void OnEnable()
	{

		m_cameraSensitivitySlider.minValue = Settings.g_minCameraSensitiviy;
		m_cameraSensitivitySlider.maxValue = Settings.g_maxCameraSensitiviy;

		RefreshSliders();
	}

	public void RefreshSliders()
	{

		m_cameraSensitivitySlider.value = Settings.g_currentXCameraSensitiviy;
	}

	public Vector2 GetSensitivity()
	{
		return new Vector2(m_cameraSensitivitySlider.value, m_cameraSensitivitySlider.value/100);
	}

}
