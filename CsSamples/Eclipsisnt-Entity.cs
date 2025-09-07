using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    #region Static Variables

    public static Material[] stateMats;
    public static Color finishedBlueprintColor = new Color(14f / 255f, 148f / 255f, 217f / 255f, 0.75f);
    public static int buildingMask = 10;
    public static int blueprintMask = 11;

    #endregion

    #region Instance Variables

    [SerializeField]
    private int _state;
    public int state {
        get => _state;
        set {
            if (value == _state) return; // Don't update if value isn't changing

            _state = value;

            UpdateMaterial();
            UpdateLayer();
        }
    } // 0: Building, 1: Blueprint, 2: Ghost

    [SerializeField]
    public int health;// { get; private set; }
    [SerializeField]
    public int maxHealth;// { get; private set; }

    private float __iridiumFilled;

    [SerializeField]
    public float iridiumFilled
    { 
        get { return __iridiumFilled; }
        set
        {
            __iridiumFilled = value;
            if (state == 1)
                setMatColor(transform, Construction.BlueprintFillMat(value / buildCost));
        }
    }
    [SerializeField]
    public int buildCost;// { get; private set; }

    [SerializeField]
    public string entityType;// { get; private set; }

    #endregion

    #region MonoBehaviour Implementation

    public static void Initialize()
    {
        stateMats = new Material[]
        {
            Resources.Load("Materials/Custom/Buildings/Building") as Material,
            Resources.Load("Materials/Custom/Buildings/Blueprint") as Material,
            Resources.Load("Materials/Custom/Buildings/GhostValid") as Material,
            Resources.Load("Materials/Custom/Buildings/GhostInvalid") as Material
        };

        Color __color = stateMats[1].color;
        stateMats[1].color = new Color(__color.r, __color.g, __color.b, 0.2f);
        __color = stateMats[2].color;
        stateMats[2].color = new Color(__color.r, __color.g, __color.b, 0.5f);

        /*(new Vector3[] {
            new Vector3(1, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(3, 0, 0)
        }).Join(new Vector3[]
        {
            new Vector3(4, 0, 0),
            new Vector3(5, 0, 0),
            new Vector3(6, 0, 0)
        }).Log();*/
    }

    #endregion

    #region NetworkBehaviour Implementation

    #endregion

    #region Methods

    public virtual void Setup(string _entityType, int _maxHealth, int _buildCost, int _state = 0)
    {
        entityType = _entityType;
        health = _maxHealth;
        maxHealth = _maxHealth;
        iridiumFilled = 0;
        buildCost = _buildCost;
        state = _state;
    }

    // Recursive method to set the material of all descendants to mat
    protected void setMat(Transform obj, Material mat)
    {
        Renderer renderer;
        obj.TryGetComponent(out renderer);

        //Debug.Log((renderer!=null) + " Attempt set mat " + obj.name);

        if (renderer)
            renderer.material = mat;

        for (int i = 0; i < obj.childCount; i++)
        {
            Transform child = obj.GetChild(i);

            setMat(child, mat);
        }
    }

    // Updates material based on 'state'
    public virtual void UpdateMaterial()
    {
        Debug.Log("Updating material: " + state);

        if (state != 0)
        {
            Material mat = stateMats[state];

            setMat(transform, mat);
        }
        else
        {
            Construction.RestoreMaterials(this);
        }
    }

    public void ForceSetMaterial(Material mat)
    {
        setMat(transform, mat);
    }

    protected void setLayer(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;

        for (int i = 0; i < obj.childCount; i++)
        {
            setLayer(obj.GetChild(i), layer);
        }
    }

    public void UpdateLayer()
    {
        if (state == 0)
            setLayer(transform, buildingMask);
        else if (state == 1)
            setLayer(transform, blueprintMask);
        //else if (state == 2)
        //    setLayer(transform, ghostMask);
    }

    // Recursive method to set the material of all descendants to mat
    protected void setMatColor(Transform obj, Color color)
    {
        Renderer renderer;
        obj.TryGetComponent<Renderer>(out renderer);

        if (renderer)
            renderer.material.color = color;

        for (int i = 0; i < obj.childCount; i++)
        {
            Transform child = obj.GetChild(i);

            setMatColor(child, color);
        }
    }

    // Returns total iridium used
    public float FillIridium(float cost)
    {
        if (iridiumFilled + cost < 0)
            cost -= iridiumFilled; // Don't let blueprint go below zero fill
        else if (iridiumFilled + cost > buildCost)
            cost = buildCost - iridiumFilled; // Don't let the blueprint be over filled

        iridiumFilled += cost;

        if (iridiumFilled == buildCost)
            Construction.main.CompleteBlueprint(this);

        return cost;
    }

    public virtual void Destroy()
    {
        Construction.main.DestroyEntity(this);
    }

    #endregion

    public static explicit operator Entity(Type v)
    {
        throw new NotImplementedException();
    }
}