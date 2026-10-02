using System;

#if CRUST
//crust has no exception objects: a failure is an integer code raised with `throw (int)FutileError.X;`
//(see CSRUST.md, "throw / catch"). Message text lives only in the Unity build.
public enum FutileError
{
	None = 0,
	NoResolutionLevel,
	DuplicateElementName,
	MissingFont,
	MissingElement,
	MissingAtlasData,
	BadAtlasData,
	BadFont,
	BadTouchableNode,
	ParticleAtlasMismatch,
	NeedsSingleImage,
	BadTileMap
}
#else
public class FutileException : Exception
{
	public FutileException (string message) : base(message)
	{
	}
	
	public FutileException () : base()
	{
	}
}
#endif

