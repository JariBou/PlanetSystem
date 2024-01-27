using System;
using System.Collections;
using System.Collections.Generic;
using GraphicsLabor.Scripts.Attributes.LaborerAttributes.InspectedAttributes;
using UnityEngine;

public class GalaxyConfigurator : MonoBehaviour
{
    public static GalaxyConfigurator Instance { get; private set; }
    [SerializeField] private Transform _planetsParent;
    [SerializeField] private GameObject _planetPrefab;
    [SerializeField] private GameObject _starPrefab;

    public Transform PlanetsParent => _planetsParent;
    public GameObject PlanetPrefab => _planetPrefab;
    public GameObject StarPrefab => _starPrefab;

    private void Awake()
    {
        Instance ??= this;
    }
    
}
