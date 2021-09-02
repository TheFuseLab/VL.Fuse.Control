using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuse.Controls
{
    public class BezierControlPoint : ControlPoint
    {

	public BezierControlPoint() : base(ControlPointType.BEZIER) {
		
	}

	public BezierControlPoint(float theTime, float theValue) : base(theTime, theValue, ControlPointType.BEZIER){
		
	}
	
	public BezierControlPoint(ControlPoint theControlPoint) : this(theControlPoint.Time, theControlPoint.Value) {
	}

	public override bool HasHandles() {
		return true;
	}

	public HandleControlPoint InHandle { get; set; }


	public HandleControlPoint OutHandle { get; set; }

	
	/**
	 * Returns the bezier blend between 0 and 1 that would result in the given x
	 * @param theTime0 time of the first key
	 * @param theTime1 time of the first control point
	 * @param theTime2 time of the second control point
	 * @param theTime3 time of the second key
	 * @param theTime 
	 * @return bezier blend for the given time
	 */
	private static float BezierBlend(float theTime0, float theTime1, float theTime2, float theTime3, float theTime) {
		var a = -theTime0 + 3 * theTime1 - 3 * theTime2 + theTime3;
		var b = 3 * theTime0 - 6 * theTime1 + 3 * theTime2;
		var c = -3 * theTime0 + 3 * theTime1;
		var d = theTime0 - theTime;

		var myResult = CubicSolver.SolveCubic(a, b, c, d);
		var i = 0;
		while(i < myResult.Length - 1 && (myResult[i] < 0 || myResult[i] > 1)) {
			i++;
		}
		return myResult[i];
	}
	
	private static float BezierValue(float theValue0, float theValue1, float theValue2, float theValue3, float theBlend) {
		var a = -theValue0 + 3 * theValue1 - 3 * theValue2 + theValue3;
		var b = 3 * theValue0 - 6 * theValue1 + 3 * theValue2;
		var c = -3 * theValue0 + 3 * theValue1;
		var d = theValue0;
		
		return 
		a * theBlend * theBlend * theBlend + 
		b * theBlend * theBlend	+ 
		c * theBlend + 
		d;
	}

	public float SampleBezierSegment(ControlPoint p0, ControlPoint p1, ControlPoint p2, ControlPoint p3, float theTime) {
		var myBezierBlend = BezierBlend(p0.Time, p1.Time, p2.Time, p3.Time, theTime);
		return BezierValue(p0.Value, p1.Value, p2.Value, p3.Value, myBezierBlend);
	}

	
	public override float InterpolateValue(float theTime, AnimationCurve theData) {
		try{
			var mySample = new ControlPoint(theTime, 0);
			var myHeadSet = theData.HeadSet(mySample, false);
	
			ControlPoint p1 = null;
			ControlPoint p2 = null;
	
			if (myHeadSet.Count() != 0) {
				p1 = theData.GetLastOnSamePosition(myHeadSet.Last());
			}
			
			if(p1.GetType() == typeof(BezierControlPoint)) {
				p2 = ((BezierControlPoint)p1).OutHandle;
			}else {
				p2 = p1;
			}
	
			return SampleBezierSegment(p1, p2, InHandle, this, theTime);
		}catch(Exception){
			return 0;
		}
	}
	
	public override ControlPoint Clone() {
		var myResult = new BezierControlPoint(Time, Value) {InHandle = (HandleControlPoint) InHandle.Clone()};
		myResult.InHandle.Parent = myResult;
		
		myResult.OutHandle = (HandleControlPoint)OutHandle.Clone();
		myResult.OutHandle.Parent = myResult;
		
		return myResult;
	}
	
	/* (non-Javadoc)
	 * @see de.artcom.timeline.model.points.ControlPoint#setTime(float)
	 */
	public override float Time {
		set
		{
			var myDifference = value - Time;
			base.Time = value;

			InHandle.Time += myDifference;
			OutHandle.Time += myDifference;
		}
	}
	
	/*
	@Override
	public CCDataObject data(float theStartTime, float theEndTime) {
		CCDataObject myResult = super.data(theStartTime, theEndTime);
		myResult.put("in", _myInHandle.data(theStartTime, theEndTime));
		myResult.put("out", _myOutHandle.data(theStartTime, theEndTime));
		return myResult;
	}
	
	@Override
	public void data(CCDataObject theData) {
		super.data(theData);
		CCDataObject myInHandleData = theData.getObject("in");
		_myInHandle = new HandleControlPoint(
			this, 
			HandleType.BEZIER_IN_HANDLE, 
			myInHandleData.getfloat(TIME_ATTRIBUTE), 
			myInHandleData.getfloat(VALUE_ATTRIBUTE)
		);
		
		CCDataObject myOutHandleData = theData.getObject("out");
		_myOutHandle = new HandleControlPoint(
			this, 
			HandleType.BEZIER_OUT_HANDLE, 
			myOutHandleData.getfloat(TIME_ATTRIBUTE), 
			myOutHandleData.getfloat(VALUE_ATTRIBUTE)
		);
	}*/
    }
}