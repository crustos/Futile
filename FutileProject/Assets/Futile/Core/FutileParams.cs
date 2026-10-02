using System;
using UnityEngine;
using System.Collections.Generic;

public class FResolutionLevel
{
	public float maxLength = 1000f;
	public float displayScale = 1f;
	public float resourceScale = 1f;
#if !CRUST
	public string resourceSuffix = "";
#endif
}

public class FutileParams
{
	public List<FResolutionLevel> resLevels = new List<FResolutionLevel>();
	
	public Vector2 origin = new Vector2(0.5f,0.5f);
	
	public int targetFrameRate = 60;
	
	public ScreenOrientation singleOrientation = ScreenOrientation.AutoRotation;
	
	public bool supportsLandscapeLeft;
	public bool supportsLandscapeRight;
	public bool supportsPortrait;
	public bool supportsPortraitUpsideDown;
	
	public Color backgroundColor = Color.black;
	
	public bool shouldLerpToNearestResolutionLevel = true;
	public FResolutionLevelPickMode resolutionLevelPickMode = FResolutionLevelPickMode.Upwards;

	public FResolutionLevelPickDimension resolutionLevelPickDimension = FResolutionLevelPickDimension.Longest;

    public Func<int,int,FResolutionLevel> resolutionLevelPicker = null; //can optionally specify this for exact control over resolutionlevel
	
	public FutileParams(bool supportsLandscapeLeft, bool supportsLandscapeRight, bool supportsPortrait, bool supportsPortraitUpsideDown)
	{
		this.supportsLandscapeLeft = supportsLandscapeLeft;
		this.supportsLandscapeRight = supportsLandscapeRight;
		this.supportsPortrait = supportsPortrait;
		this.supportsPortraitUpsideDown = supportsPortraitUpsideDown;
	}

#if CRUST
	//crust: no resourceSuffix -- resource paths are resolved when the game is packed
	public FResolutionLevel AddResolutionLevel (float maxLength, float displayScale, float resourceScale)
#else
	public FResolutionLevel AddResolutionLevel (float maxLength, float displayScale, float resourceScale, string resourceSuffix)
#endif
	{
		FResolutionLevel resLevel = new FResolutionLevel();
		
		resLevel.maxLength = maxLength;
		resLevel.displayScale = displayScale;
		resLevel.resourceScale = resourceScale;
#if !CRUST
		resLevel.resourceSuffix = resourceSuffix;
#endif
		
		bool wasAdded = false;
		
		//we've gotta have the resLevels sorted low to high by maxLength
		for(int r = 0; r<resLevels.Count; ++r)
		{
			if(resLevel.maxLength < resLevels[r].maxLength)
			{
				resLevels.Insert(r,resLevel);	
				wasAdded = true;
				break;
			}
		}
		
		if(!wasAdded)
		{
			resLevels.Add(resLevel);	
		}
		
		return resLevel;
	}

}

public enum FResolutionLevelPickMode
{
	Upwards, //default behavior, rounds upwards, won't take a resolution level with a maxlength LOWER than the screen size
	Downwards, //rounds downwards, the moment max is equal or bigger than the reslevel, it takes it
	Closest //gets the closest resolution level by comparing the deltas of (reslevel.maxlength - longestScreenDimension)
}

public enum FResolutionLevelPickDimension
{
	Shortest, //compares against the shortest dimension
	Longest //compares against the longest dimension
}

