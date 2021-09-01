﻿using System;
using System.Collections.Generic;
using Stride.Core.Mathematics;

namespace Fuse.Controls
{
    
    public readonly struct GradientPoint : IComparable<GradientPoint>
    {
	    public float Position { get; }
	    public Color4 Color { get; }

        public GradientPoint(float thePosition, Color4 theColor) {
	        Position = thePosition;
	        Color = theColor;
        }

        public int CompareTo(GradientPoint o) {
            return Position.CompareTo(o.Position);
        }
	
        public override string ToString() {
            return Position + " : " + Color;
        }
    }
    
    public class Gradient : List<GradientPoint>
    {
	    public void Add(float thePosition, Color4 theColor){
		Add(new GradientPoint(thePosition, theColor));
	}

	public Gradient Clone(){
		var myResult = new Gradient();
		ForEach(point => myResult.Add(point));
		return myResult;
	}
	
	public void Set(Gradient theGradient){
		if(theGradient == this)return;
		Clear();
		theGradient.ForEach(point => Add(point));
	}
	
	
	public new void Add(GradientPoint e) {
		base.Add(e);
		Sort();
	}
	
	private static float SmoothStep( float theEdge0,  float theEdge1,  float theValue) {
		if (theValue <= theEdge0)
			return 0;
		if (theValue >= theEdge1)
			return 1;
		
		return 3 * (float)Math.Pow((theValue-theEdge0)/(theEdge1-theEdge0), 2) - 2 * (float)Math.Pow((theValue-theEdge0)/(theEdge1-theEdge0), 3);
	}
	
	/*
	 * Interpolate a color on the gradient
	 * @param theBlend a value from 0 to 1 that represent the position between the first control point and the last one
	 * @return the position
	 */
	public Color4 Interpolate (float thePosition){
		
		switch (Count)
		{
			case 0:
				return new Color4();
			case 1:
				return this[0].Color;
		}

		var myIndex = -1;
		
		for(var i = 0; i < Count;i++)
		{
			if (!(this[i].Position > thePosition)) continue;
			myIndex = i;
			break;
		}
		
		switch (myIndex)
		{
			case -1:
				return this[Count - 1].Color;
			case 0:
				return this[0].Color;
		}

		var myPos0 = this[myIndex - 1].Position;
		var myPos1 = this[myIndex].Position;
		var myBlend = SmoothStep(myPos0, myPos1, thePosition);
		return Color4.Lerp(this[myIndex - 1].Color, this[myIndex].Color, myBlend);
	}

	public Gradient Blend(Gradient theB, float theScalar) {
		if(theScalar <= 0)return  Clone();
		if(theScalar >= 1)return theB.Clone();
		
		var myResult = new Gradient();
		
		ForEach(myPoint => myResult.Add(new GradientPoint(myPoint.Position, Color4.Lerp(Interpolate(myPoint.Position),theB.Interpolate(myPoint.Position), theScalar))));
		theB.ForEach(myPoint => myResult.Add(new GradientPoint(myPoint.Position, Color4.Lerp(Interpolate(myPoint.Position),theB.Interpolate(myPoint.Position), theScalar))));

		return myResult;
	}
	
	/*
	@Override
	public Map<String, Object> data() {
		List<Map<String, Object>> myPoints = new ArrayList<>();
		for(GradientPoint myPoint:this) {
			Map<String, Object>myPointData = myPoint.color().data();
			myPointData.put("position", myPoint.position());
			myPoints.add(myPointData);
		}
		
		Map<String, Object> myResult = new HashMap<>();
		myResult.put(CCBlendable.BLENDABLE_TYPE_ATTRIBUTE, getClass().getName());
		myResult.put("points", myPoints);
		return myResult;
	}
	
	@SuppressWarnings("unchecked")
	@Override
	public void data(Map<String, Object> theData) {
		List<Map<String, Object>> myPoints = (List<Map<String, Object>>)theData.get("points");

		for(Map<String, Object> myPointData:myPoints) {
			double myPosition = (Double)myPointData.get("position");
			Vector4 myColor = new Vector4();
			myColor.data(myPointData);
			add(new GradientPoint(myPosition, myColor));
		}
	}*/
    }

}