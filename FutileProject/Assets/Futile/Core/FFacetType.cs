using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FFacetType
{
	static public FFacetType defaultFacetType;
	
	//facetType types
	public static FFacetType Quad;
	public static FFacetType Triangle;
	
	private static int _nextFacetTypeIndex = 0;
	private static List<FFacetType> _facetTypes = new List<FFacetType>();
	
	public int index;
#if !CRUST
	public string name;
#endif
	
	public int initialAmount;
	public int expansionAmount;
	public int maxEmptyAmount;
	
	public delegate FFacetRenderLayer CreateRenderLayerDelegate(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader);
	
	public CreateRenderLayerDelegate createRenderLayer;
	
#if CRUST
	//crust: no names. A facet type is its index, and CreateFacetType is told which one it is.
	private FFacetType (int index, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer) //only to be constructed by using CreateFacetType()
	{
		this.index = index;
#else
	private FFacetType (string name, int index, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer) //only to be constructed by using CreateFacetType()
	{
		this.index = index;
		this.name = name;
#endif
		
		this.initialAmount = initialAmount;
		this.expansionAmount = expansionAmount;
		this.maxEmptyAmount = maxEmptyAmount;
		
		this.createRenderLayer = createRenderLayer;
	}
	
	public static void Init() //called by Futile
	{
#if CRUST
		Quad = CreateFacetType(0, 10, 10, 60, CreateQuadLayer);	
		Triangle = CreateFacetType(1, 16, 16, 64,CreateTriLayer);	
#else
		Quad = CreateFacetType("Quad", 10, 10, 60, CreateQuadLayer);	
		Triangle = CreateFacetType("Triangle", 16, 16, 64,CreateTriLayer);	
#endif
		
		defaultFacetType = Quad;
	}
	
	//create your own FFacetTypes by creating them here
#if CRUST
	public static FFacetType CreateFacetType(int facetTypeId, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer)
	{
		for(int s = 0; s<_facetTypes.Count; s++)
		{
			if(_facetTypes[s].index == facetTypeId) return _facetTypes[s]; //don't add it if we have it already
		}
		
		FFacetType newFacetType = new FFacetType(_nextFacetTypeIndex++, initialAmount, expansionAmount, maxEmptyAmount, createRenderLayer);
#else
	public static FFacetType CreateFacetType(string facetTypeShortName, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer)
	{
		for(int s = 0; s<_facetTypes.Count; s++)
		{
			if(_facetTypes[s].name == facetTypeShortName) return _facetTypes[s]; //don't add it if we have it already	
		}
		
		FFacetType newFacetType = new FFacetType(facetTypeShortName, _nextFacetTypeIndex++, initialAmount, expansionAmount, maxEmptyAmount, createRenderLayer);
#endif
		_facetTypes.Add (newFacetType);
		
		return newFacetType;
	}
	
	static private FFacetRenderLayer CreateQuadLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
	{
		return new FQuadRenderLayer(stage,facetType,atlas,shader);
	}
	
	static private FFacetRenderLayer CreateTriLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
	{
		return new FTriangleRenderLayer(stage,facetType,atlas,shader);
	}
	
}


