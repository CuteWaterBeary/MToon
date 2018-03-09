﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.Rendering;

public class MToonInspector : ShaderGUI
{
	public enum DebugMode
	{
		None,
		Normal,
	}

	public enum OutlineMode
	{
		None,
		Colored,
	}

	public enum RenderMode
	{
		Opaque,
		Cutout,
		Transparent,
	}

	private MaterialProperty _debugMode;
	private MaterialProperty _outlineMode;
	private MaterialProperty _blendMode;
	private MaterialProperty _cullMode;
	private MaterialProperty _cutoff;
	private MaterialProperty _color;
	private MaterialProperty _shadeColor;
	private MaterialProperty _mainTex;
	private MaterialProperty _shadeTexture;
	private MaterialProperty _bumpScale;
	private MaterialProperty _bumpMap;
	private MaterialProperty _receiveShadowRate;
	private MaterialProperty _receiveShadowTexture;
	private MaterialProperty _shadeShift;
	private MaterialProperty _shadeToony;
	private MaterialProperty _lightColorAttenuation;
	private MaterialProperty _sphereAdd;
	private MaterialProperty _outlineWidthTexture;
	private MaterialProperty _outlineWidth;
	private MaterialProperty _outlineColor;
	private MaterialProperty _outlineLightingMix;

	private bool _firstTimeApply = true;


	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		_debugMode = FindProperty("_DebugMode", properties);
		_outlineMode = FindProperty("_OutlineMode", properties);
		_blendMode = FindProperty("_BlendMode", properties);
		_cullMode = FindProperty("_CullMode", properties);
		_cutoff = FindProperty("_Cutoff", properties);
		_color = FindProperty("_Color", properties);
		_shadeColor = FindProperty("_ShadeColor", properties);
		_mainTex = FindProperty("_MainTex", properties);
		_shadeTexture = FindProperty("_ShadeTexture", properties);
		_bumpScale = FindProperty("_BumpScale", properties);
		_bumpMap = FindProperty("_BumpMap", properties);
		_receiveShadowRate = FindProperty("_ReceiveShadowRate", properties);
		_receiveShadowTexture = FindProperty("_ReceiveShadowTexture", properties);
		_shadeShift = FindProperty("_ShadeShift", properties);
		_shadeToony = FindProperty("_ShadeToony", properties);
		_lightColorAttenuation = FindProperty("_LightColorAttenuation", properties);
		_sphereAdd = FindProperty("_SphereAdd", properties);
		_outlineWidthTexture = FindProperty("_OutlineWidthTexture", properties);
		_outlineWidth = FindProperty("_OutlineWidth", properties);
		_outlineColor = FindProperty("_OutlineColor", properties);
		_outlineLightingMix = FindProperty("_OutlineLightingMix", properties);

		if (_firstTimeApply)
		{
			_firstTimeApply = false;
			foreach (var obj in materialEditor.targets)
			{
				var mat = (Material) obj;
				SetupBlendMode(mat, (RenderMode) mat.GetFloat(_blendMode.name));
				SetupNormalMode(mat, mat.GetTexture(_bumpMap.name));
				SetupOutlineMode(mat, (OutlineMode) mat.GetFloat(_outlineMode.name));
				SetupDebugMode(mat, (DebugMode) mat.GetFloat(_debugMode.name));
				SetupCullMode(mat, (CullMode) mat.GetFloat(_cullMode.name));
			}
		}

		EditorGUI.BeginChangeCheck();
		{
			EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
				EditorGUI.showMixedValue = _blendMode.hasMixedValue;
				EditorGUI.BeginChangeCheck();
				var bm = (RenderMode) EditorGUILayout.Popup("Rendering Type", (int) _blendMode.floatValue,
					Enum.GetNames(typeof(RenderMode)));
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("RenderType");
					_blendMode.floatValue = (float) bm;

					foreach (var obj in materialEditor.targets)
					{
						SetupBlendMode((Material) obj, bm);
					}
				}
				EditorGUI.showMixedValue = false;

				EditorGUI.showMixedValue = _cullMode.hasMixedValue;
				EditorGUI.BeginChangeCheck();
				var cm = (CullMode) EditorGUILayout.Popup("Cull Mode", (int) _cullMode.floatValue,
					Enum.GetNames(typeof(CullMode)));
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("CullType");
					_cullMode.floatValue = (float) cm;

					foreach (var obj in materialEditor.targets)
					{
						SetupCullMode((Material) obj, cm);
					}
				}
				EditorGUI.showMixedValue = false;
				EditorGUILayout.Space();

				if (bm != RenderMode.Opaque)
				{
					EditorGUILayout.LabelField("Alpha", EditorStyles.boldLabel);
					{
						if (bm == RenderMode.Transparent)
						{
							EditorGUILayout.TextField("Ensure your lit color and texture have alpha channels.");
						}

						if (bm == RenderMode.Cutout)
						{
							EditorGUILayout.TextField("Ensure your lit color and texture have alpha channels.");
							materialEditor.ShaderProperty(_cutoff, "Cutoff");
						}
					}
					EditorGUILayout.Space();
				}

				EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
				{
					// Color
					materialEditor.TexturePropertySingleLine(new GUIContent("Lit & Alpha", "Lit (RGB), Alpha (A)"), _mainTex, _color);
					materialEditor.TexturePropertySingleLine(new GUIContent("Shade", "Shade (RGB)"), _shadeTexture, _shadeColor);
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Shade", EditorStyles.boldLabel);
				{
					// Shade
					materialEditor.ShaderProperty(_shadeShift, "Shift");
					materialEditor.ShaderProperty(_shadeToony, "Toony");
					materialEditor.ShaderProperty(_lightColorAttenuation, "LightColor Attenuation");
				}
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Shadow", EditorStyles.boldLabel);
				{
					// Shadow
					if (((Material) materialEditor.target).GetFloat("_ShadeShift") < 0f)
					{
						EditorGUILayout.LabelField("Receive rate should be lower value when Shade Shift is lower than 0.",
							EditorStyles.wordWrappedLabel);
					}

					materialEditor.TexturePropertySingleLine(new GUIContent("Receive Rate", "Receive Shadow Rate Map (A)"),
						_receiveShadowTexture, _receiveShadowRate);
				}
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Rim", EditorStyles.boldLabel);
				{
					// Rim Light
					materialEditor.TexturePropertySingleLine(new GUIContent("Additive", "Rim Additive Texture (RGB)"), _sphereAdd);
				}
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Normal", EditorStyles.boldLabel);
				{
					// Normal
					EditorGUI.BeginChangeCheck();
					materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"), _bumpMap, _bumpScale);
					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("BumpEnabledDisabled");

						foreach (var obj in materialEditor.targets)
						{
							var mat = (Material) obj;
							SetupNormalMode(mat, mat.GetTexture(_bumpMap.name));
						}
					}
				}
			}
			EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

			EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);
				{
					// Outline
					EditorGUI.showMixedValue = _outlineMode.hasMixedValue;
					EditorGUI.BeginChangeCheck();
					var om = (OutlineMode) EditorGUILayout.Popup("Mode", (int) _outlineMode.floatValue,
						Enum.GetNames(typeof(OutlineMode)));
					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("OutlineType");
						_outlineMode.floatValue = (float) om;

						foreach (var obj in materialEditor.targets)
						{
							SetupOutlineMode((Material) obj, om);
						}
					}

					EditorGUI.showMixedValue = false;

					if (om != OutlineMode.None)
					{
						materialEditor.TexturePropertySingleLine(new GUIContent("Width", "Outline Width Texture (RGB)"),
							_outlineWidthTexture, _outlineWidth);
						materialEditor.ShaderProperty(_outlineColor, "Color");
						materialEditor.DefaultShaderProperty(_outlineLightingMix, "Lighting Mix");
					}
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Texture Options", EditorStyles.boldLabel);
				{
					EditorGUI.BeginChangeCheck();
					materialEditor.TextureScaleOffsetProperty(_mainTex);
					if (EditorGUI.EndChangeCheck())
					{
						_shadeTexture.textureScaleAndOffset = _mainTex.textureScaleAndOffset;
						_bumpMap.textureScaleAndOffset = _mainTex.textureScaleAndOffset;
						_receiveShadowTexture.textureScaleAndOffset = _mainTex.textureScaleAndOffset;
					}
				}
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Debugging Options", EditorStyles.boldLabel);
				{
					EditorGUI.showMixedValue = _debugMode.hasMixedValue;
					EditorGUI.BeginChangeCheck();
					var dm = (DebugMode) EditorGUILayout.Popup("Visualize", (int) _debugMode.floatValue,
						Enum.GetNames(typeof(DebugMode)));
					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("DebugType");
						_debugMode.floatValue = (float) dm;

						foreach (var obj in materialEditor.targets)
						{
							SetupDebugMode((Material) obj, dm);
						}
					}

					EditorGUI.showMixedValue = false;
				}
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
				{
					materialEditor.EnableInstancingField();
					materialEditor.DoubleSidedGIField();
					materialEditor.RenderQueueField();
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
		}
		EditorGUI.EndChangeCheck();
	}

	private void SetupDebugMode(Material material, DebugMode debugMode)
	{
		switch (debugMode)
		{
			case DebugMode.None:
				SetKeyword(material, "MTOON_DEBUG_NORMAL", false);
				break;
			case DebugMode.Normal:
				SetKeyword(material, "MTOON_DEBUG_NORMAL", true);
				break;
		}
	}

	private void SetupBlendMode(Material material, RenderMode renderMode)
	{
		switch (renderMode)
		{
			case RenderMode.Opaque:
				material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int) BlendMode.One);
                material.SetInt("_DstBlend", (int) BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                SetKeyword(material, "_ALPHATEST_ON", false);
                SetKeyword(material, "_ALPHABLEND_ON", false);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                material.renderQueue = -1;
				break;
			case RenderMode.Cutout:
				material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int) BlendMode.One);
                material.SetInt("_DstBlend", (int) BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                SetKeyword(material, "_ALPHATEST_ON", true);
                SetKeyword(material, "_ALPHABLEND_ON", false);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
				material.renderQueue = (int) RenderQueue.AlphaTest;
				break;
			case RenderMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int) BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                SetKeyword(material, "_ALPHATEST_ON", false);
                SetKeyword(material, "_ALPHABLEND_ON", true);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                material.renderQueue = (int) RenderQueue.Transparent;
				break;
		}
	}

	private void SetupOutlineMode(Material material, OutlineMode outlineMode)
	{
		switch (outlineMode)
		{
			case OutlineMode.None:
                SetKeyword(material, "MTOON_OUTLINE_COLORED", false);
				break;
			case OutlineMode.Colored:
                SetKeyword(material, "MTOON_OUTLINE_COLORED", true);
				break;
		}
	}

	private void SetupNormalMode(Material material, bool requireNormalMapping)
	{
		SetKeyword(material, "_NORMALMAP", requireNormalMapping);
	}

	private void SetupCullMode(Material material, CullMode cullMode)
	{
		switch (cullMode)
		{
			case CullMode.Back:
                material.SetInt("_CullMode", (int) CullMode.Back);
				break;
			case CullMode.Front:
                material.SetInt("_CullMode", (int) CullMode.Front);
				break;
			case CullMode.Off:
                material.SetInt("_CullMode", (int) CullMode.Off);
				break;
		}
	}

	private void SetKeyword(Material mat, string keyword, bool required)
	{
		if (required)
		{
			mat.EnableKeyword(keyword);
		}
		else
		{
			mat.DisableKeyword(keyword);
		}
	}
}
