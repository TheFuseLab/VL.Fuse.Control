using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuse.Controls
{
	public class ControlPointComparer : IComparer<ControlPoint>
	{
		public int Compare(ControlPoint x, ControlPoint y)
		{
			if (x.Time == y.Time) return 0;
			if (x.Time < y.Time) return -1;
			return 1;
		}
	}
	
	public class AnimationCurve : SortedSet<ControlPoint>{


	private int _mySize = 0;
	protected bool _myDirtyFlag = false;

	public AnimationCurve() : base (new ControlPointComparer()){
		
		//base((p1,p2)->float.compare(p1.Time, p2.Time));
	}

	public bool IsDirty() {
		return _myDirtyFlag;
	}
	
	public void SetDirty(bool theFlag) {
		_myDirtyFlag = theFlag;
	}
	
	/**
	 * Reverses the data of the given range
	 */
	public void Reverse(float theStartTime, float theEndTime){
		var myRangeCopy = CopyRange(theStartTime, theEndTime);
		RemoveAll(theStartTime, theEndTime);

		myRangeCopy.ForEach(myPoint =>
		{
			var myNewTime = theEndTime - myPoint.Time + theStartTime;
			myPoint.Time = myNewTime;
			Add(myPoint);
		});
	}

	/**
	 * Returns the greatest element in this set less than or equal to the given element, or null if there is no such element.
	 */
	public ControlPoint Floor(ControlPoint thePoint)
	{
		return new LinearControlPoint();
	}
	
	/**
	 * Returns the least element in this set greater than or equal to the given element, or null if there is no such element.
	 */
	public ControlPoint Ceiling(ControlPoint thePoint)
	{
		return new LinearControlPoint();
	}
	
	/**
	 * Returns the greatest element in this set strictly less than the given element, or null if there is no such element.
	 */
	public ControlPoint Lower(ControlPoint thePoint)
	{
		return new LinearControlPoint();
	}
	
	/**
	 * Returns the least element in this set strictly greater than the given element, or null if there is no such element.
	 */
	public ControlPoint Higher(ControlPoint thePoint)
	{
		return new LinearControlPoint();
	}
	

	/**
	 * Returns a view of the portion of this set whose elements are less than (or equal to, if inclusive is true) toElement.
	 * The returned set is backed by this set, so changes in the returned set are reflected in this set, and vice-versa.
	 * The returned set supports all optional set operations that this set supports.
	 */
	public SortedSet<ControlPoint> HeadSet(ControlPoint theElement, bool inclusive )
	{
		return new SortedSet<ControlPoint>();
	}

	/**
	 * Returns a view of the portion of this set whose elements are greater than (or equal to, if inclusive is true) fromElement.
	 * The returned set is backed by this set, so changes in the returned set are reflected in this set, and vice-versa.
	 * The returned set supports all optional set operations that this set supports
	 *
	 * if the low endpoint is to be included in the returned view
	 */
	public SortedSet<ControlPoint> TailSet(ControlPoint theElement, bool inclusive )
	{
		return new SortedSet<ControlPoint>();
	}
	
	/**
	 * @param thePoint
	 */
	public new bool Add(ControlPoint thePoint) {

		_mySize += 1;
		_myDirtyFlag = true;
		
		if (Contains(thePoint)) {
			var myTreeLeaf = Floor(thePoint);
			if (myTreeLeaf.Equals(thePoint)) {
				_mySize -=1;
				_myDirtyFlag = false;
				return false;
			}
			if (thePoint.isPrevious(myTreeLeaf)) {
				InsertBefore(myTreeLeaf, thePoint);
			} else {
				myTreeLeaf = GetLastOnSamePosition(myTreeLeaf);
				// do not insert into the tree, just update the float linked list
				myTreeLeaf.append(thePoint);
			}
		} else { // new leaf
			base.Add(thePoint);
			thePoint.CutLoose();
			var myLower = Lower(thePoint);
			if (myLower != null) {
				myLower = GetLastOnSamePosition(myLower);
				myLower.append(thePoint);
			}
			var myHigher = Higher(thePoint);
			myHigher?.prepend(thePoint);
		}
		return true;
	}

	private static ControlPoint CreateSamplePoint(float theTime) {
		return new ControlPoint(theTime, 0);
	}

	/**
	 * Returns the least control point with a time greater than or equal to the 
	 * given time, or null if there is no control point after the given time. 
	 * @param theTime
	 * @return
	 */
	public ControlPoint GetFirstPointAt(float theTime) {
		return Ceiling(new ControlPoint(theTime, 0));
	}

	public ControlPoint GetLastOnSamePosition(ControlPoint thePoint)
	{
		if (!Contains(thePoint)) {
			return thePoint;
		}

		var myTreeLeaf = Floor(thePoint);
		if(myTreeLeaf == null)return thePoint;
		while (myTreeLeaf.hasNext()) {
			if (myTreeLeaf.getNext().Time != thePoint.Time) {
				return myTreeLeaf;
			} else {
				myTreeLeaf = myTreeLeaf.getNext();
			}
		}
		return myTreeLeaf;
	}

	public ControlPoint GetLastPointAt(float theTime) {
		return GetLastOnSamePosition(Ceiling(new ControlPoint(theTime, 0)));
	}

	public ControlPoint GetLastPoint() {
		try {
			return GetLastOnSamePosition(Max);
		} catch (Exception e) {
			return null;
		}
	}
	
	/**
	 * Returns the time of the Last point in the dataset
	 * @return
	 */
	
	public float GetLastTime(){
		var myLastPoint = GetLastPoint();
		
		if (myLastPoint != null) {
			return myLastPoint.Time;
		}
		return 0;
	}

    /**
     * Returns the accumulated Value for the given time
     * @param theTime
     * @return
     */
    public float GetAccumulatedValue(float theTime) {
    	return Value(theTime);
    }

	public float Value(float theTime) {
		if (Count == 0) {
			return 0.0f;
		}
		
		var mySample = CreateSamplePoint(theTime);
		var myLower = Lower(mySample);
		var myCeiling = Ceiling(mySample);

		if (myLower == null) {
			if (myCeiling != null) {
				return myCeiling.Value;
			}
			return 0.0f;
		}

		myLower = GetLastOnSamePosition(myLower);

		if (myCeiling != null) {
			return myCeiling.InterpolateValue(theTime, this);
		}

		return myLower?.Value ?? 0.0f;
	}

	private void InsertBefore(ControlPoint theLocation, ControlPoint theInsertion) {
		theLocation.prepend(theInsertion);
		// the first element in the float linked list at the same x position
		// becomes the new tree leaf
		if (theLocation.Time != theInsertion.Time) return;
		
		base.Remove(theLocation);
		base.Add(theInsertion);
	}

	/**
	 * Returns a list of all points in the given range
	 * @param theStartTime
	 * @param theEndTime
	 * @return
	 */
	public List<ControlPoint> RangeList(float theStartTime, float theEndTime) {
		var myRange = new List<ControlPoint>();
		
		var myMinPoint = Ceiling(new ControlPoint(theStartTime, 0));

		if (myMinPoint == null || myMinPoint.Time > theEndTime) {
			return myRange;
		}
		var myMaxPoint = Floor(new ControlPoint(theEndTime, 0));

		myMaxPoint = GetLastOnSamePosition(myMaxPoint);
		var myCurrentPoint = myMinPoint;

		while (myCurrentPoint != null && myCurrentPoint != myMaxPoint) {
			myRange.Add(myCurrentPoint);
			myCurrentPoint = myCurrentPoint.getNext();
		}
		myRange.Add(myMaxPoint);
		return myRange;
	}
	
	

	/**
	 * Returns a list of copied control points inside the given range
	 * @param theStartTime
	 * @param theEndTime
	 * @return
	 */	
	public List<ControlPoint> CopyRange(float theStartTime, float theEndTime) {
		var myRange = RangeList(theStartTime, theEndTime);
		var myCopy = new List<ControlPoint>();
		
		ControlPoint myControlPoint = null;
		myRange.ForEach(myNext =>
		{
			myNext.CutLoose();
			if (myControlPoint != null)
			{
				myControlPoint.setNext(myNext);
				myNext.setPrevious(myControlPoint);
			}

			myCopy.Add(myNext);
			myControlPoint = myNext;
		});

		return myCopy;

	}

	public List<ControlPoint> RangeList(float theMinValue) {
		var myRange = new List<ControlPoint>();
		var myMinPoint = Ceiling(new ControlPoint(theMinValue, 0));

		if (myMinPoint == null) {
			return myRange;
		}

		var myCurrentPoint = myMinPoint;

		while (myCurrentPoint != null) {
			myRange.Add(myCurrentPoint);
			myCurrentPoint = myCurrentPoint.getNext();
		}
		return myRange;
	}

	
	public new void Remove(ControlPoint thePoint) {
		var myResult = false;
		if (Contains(thePoint)) { // we have one ore more points at the location
			var myTreeLeaf = Floor(thePoint); // get the tree leaf
			if (myTreeLeaf == thePoint) { // we are removing the leaf
				
				if (myTreeLeaf.hasNext()) { // maybe Add a new leaf
					base.Remove(myTreeLeaf);
					base.Add(myTreeLeaf.getNext()); // this will replace the old leaf
				} else {
					base.Remove(myTreeLeaf);
				}
			}
			myResult = true;
		}
		// update float linked list
		if (thePoint.hasPrevious()) {
			thePoint.getPrevious().setNext(thePoint.getNext());
		}
		if (thePoint.hasNext()) {
			thePoint.getNext().setPrevious(thePoint.getPrevious());
		}
		_mySize -= 1;
		_myDirtyFlag = true;
	}

	public void Move(ControlPoint thePoint, ControlPoint theTargetLocation) {
		Remove(thePoint);
		thePoint.Time = theTargetLocation.Time;
		thePoint.Value = theTargetLocation.Value;
		Add(thePoint);
	}

	/**
	 * Removes all points in the given range
	 * @param theMinValue
	 * @param theMaxValue
	 */
	public void RemoveAll(float theMinValue, float theMaxValue) {
		var myRange = RangeList(theMinValue, theMaxValue);
		if (myRange.Count == 0) {
			return;
		}
		// connect the point before
		var myLower = Lower(myRange[0]);
		var myHigher = Higher(myRange[myRange.Count - 1]);
		if (myLower != null) {
			myLower = GetLastOnSamePosition(myLower);
			myLower.setNext(myHigher);
			myHigher?.setPrevious(myLower);
		} else
		{
			myHigher?.setPrevious(null);
		}

		myRange.ForEach(p => Remove(p));
	}

	public void ReplaceAll(float theKey, float theRange, List<ControlPoint> theArray) {
		if (theArray.Count == 0) {
			return;
		}
		
		RemoveAll(theKey, theKey + theRange);
		theArray.ForEach(p =>
		{
			p.CutLoose();
			var myClone = p.Clone();
			myClone.Time = p.Time + theKey;
			Add(myClone);
		});
	}

	public void InsertAll(float theInsertTime, float theRange, List<ControlPoint> theArray) {
		if (theArray.Count == 0) {
			return;
		}
		
		InsertTime(theInsertTime, theRange);
		ReplaceAll(theInsertTime, theRange, theArray);
	}
	
	/**
	 * Inserts time into the trackdata, by moving the the points between the
	 * insert time and the time + the range, by the given time
	 * @param theInsertTime
	 * @param theTime
	 * @param theAddRangePoints if this is true points are Added to 
	 */
	public void InsertTime(float theInsertTime, float theTime, bool theAddRangePoints) {
		var myCurrentPoint = Ceiling(new ControlPoint(theInsertTime, 0));
		var myTmpList = new List<ControlPoint>();
		while (myCurrentPoint != null) {
			myCurrentPoint.Time = myCurrentPoint.Time + theTime;
			myTmpList.Add(myCurrentPoint);
			myCurrentPoint = myCurrentPoint.getNext();
		}
	}
	
	public void InsertTime(float theInsertTime, float theTime){
		InsertTime(theInsertTime, theTime, false);
	}
	
	public void CutRangeAndTime(float theInsertTime, float theTime){
		RemoveAll(theInsertTime, theInsertTime + theTime);
		var myCurrentPoint = Ceiling(new ControlPoint(theInsertTime, 0));
		var myTmpList = new List<ControlPoint>();
		float myLastTime = 0;
		while (myCurrentPoint != null) {
			ControlPoint myPoint = myCurrentPoint.Clone();
			myPoint.Time = myCurrentPoint.Time- theInsertTime - theTime;
			myLastTime = myCurrentPoint.Time;
			myPoint.Value = myCurrentPoint.Value;
			myTmpList.Add(myPoint);
			myCurrentPoint = myCurrentPoint.getNext();
		}

		RemoveAll( theInsertTime + theTime, myLastTime);
		ReplaceAll(theInsertTime,theTime, myTmpList);
	}

	public void CutRange(float theMinValue, float theMaxValue) {
		RemoveAll(theMinValue, theMaxValue);
		var myRange = theMaxValue - theMinValue;
		var myCurrentPoint = Ceiling(new ControlPoint(theMaxValue, 0));
		while (myCurrentPoint != null) {
			this.Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time - myRange, myCurrentPoint.Value));
			myCurrentPoint = myCurrentPoint.getNext();
		}
	}
	
	public void ScaleRange(float theStartTime, float theEndTime, float theScale) {
		var myTime = theEndTime - theStartTime;
		var myScaledTime = myTime * theScale;
		
		var myMoveRange = myScaledTime - myTime;
		
		var myCurrentPoint = Ceiling(new ControlPoint(theEndTime, 0));
		while (myCurrentPoint != null) {
			Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myMoveRange, myCurrentPoint.Value));
			myCurrentPoint = myCurrentPoint.getNext();
		}
		
		var myRange = RangeList(theStartTime, theEndTime);
		if (myRange.Count == 0) {
			return;
		}

		myRange.ForEach(p =>
		{
			myTime = p.Time - theStartTime;
			myScaledTime = myTime * theScale;
			myMoveRange = myScaledTime - myTime;

			if (myCurrentPoint == null) return;
			
			Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myMoveRange, myCurrentPoint.Value));
			myCurrentPoint = myCurrentPoint.getNext();
		});
	}

	private static float Blend(float theStart, float theStop, float theBlend) {
		return theStart + (theStop - theStart) * theBlend;
	}

	private static float Norm(float theValue, float theMin, float theMax) {
		return (theValue - theMin) / (theMax - theMin);
	}
	
	public static float Map(float theValue, float theMinSrc, float theMaxSrc, float theMinDst, float theMaxDst) {
		return Blend(theMinDst, theMaxDst, Norm(theValue, theMinSrc, theMaxSrc));
	}

	
	public void ScaleRange(float theStartTime1, float theEndTime1, float theStartTime2, float theEndTime2) {
		if(theEndTime2 - theStartTime2 > theEndTime1 - theStartTime1){
			//Move points at the end
			var myCurrentPoint = Ceiling(new ControlPoint(theEndTime1, 0));
			var myEndMove = theEndTime2 - theEndTime1;
			while (myCurrentPoint != null) {
				Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myEndMove, myCurrentPoint.Value));
				myCurrentPoint = myCurrentPoint.getNext();
			}
			
			//Move points at the start
			myCurrentPoint = Floor(new ControlPoint(theStartTime1, 0));
			var myStartMove = theStartTime2 - theStartTime1;
			while (myCurrentPoint != null) {
				Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myStartMove, myCurrentPoint.Value));
				myCurrentPoint = myCurrentPoint.getPrevious();
			}
			
			var myRange = RangeList(theStartTime1, theEndTime1);

			myRange.ForEach(myControlPoint =>
			{
				var myNewTime = Map(myControlPoint.Time, theStartTime1, theEndTime1, theStartTime2, theEndTime2);
				Move(myControlPoint, new ControlPoint(myNewTime, myControlPoint.Value));
				myControlPoint = myControlPoint.getNext();
			});
		}else{
			var myRange = RangeList(theStartTime1, theEndTime1);

			myRange.ForEach(myControlPoint =>
			{
				var myNewTime = Map(myControlPoint.Time, theStartTime1, theEndTime1, theStartTime2, theEndTime2);
				Move(myControlPoint, new ControlPoint(myNewTime, myControlPoint.Value));
				myControlPoint = myControlPoint.getNext();
			});
			
			//Move points at the end
			var myCurrentPoint = Ceiling(new ControlPoint(theEndTime1, 0));
			var myEndMove = theEndTime2 - theEndTime1;
			while (myCurrentPoint != null) {
				Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myEndMove, myCurrentPoint.Value));
				myCurrentPoint = myCurrentPoint.getNext();
			}
			
			//Move points at the start
			myCurrentPoint = Floor(new ControlPoint(theStartTime1, 0));
			var myStartMove = theStartTime2 - theStartTime1;
			while (myCurrentPoint != null) {
				Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myStartMove, myCurrentPoint.Value));
				myCurrentPoint = myCurrentPoint.getPrevious();
			}
		}
		
	}

	public int Size() {
		return _mySize;
	}

	public bool IsLeaf(ControlPoint thePoint) {
		if (!Contains(thePoint)) return false;
		
		var myTreeLeaf = Floor(thePoint);
		return myTreeLeaf.Equals(thePoint);
	}
	/*
	public static  String TRACKDATA_ELEMENT = "TrackData";
	
	public CCDataObject data(){
		return data(0, GetLastTime);
	}
	
	public CCDataObject data(float theStartTime, float theEndTime) {
		CCDataObject myTrackData = new CCDataObject();
		List<ControlPoint> myPoints = RangeList(theStartTime, theEndTime);
	
		CCDataArray myPointDataArray = new CCDataArray();
		for (ControlPoint myPoint:myPoints) {
			CCDataObject myControlPointData = myPoint.data(theStartTime, theEndTime);
			myPointDataArray.Add(myControlPointData);
		}
		myTrackData.put("points", myPointDataArray);
		return myTrackData;
	}*/
	/*
	private ControlPoint createPoint(CCDataObject theData){
		ControlPoint myPoint;
		ControlPointType myType = ControlPointType.valueOf(theData.getString(ControlPoint.CONTROL_POINT_TYPE_ATTRIBUTE));
			
		switch(myType) {
		case STEP:
			myPoint = new StepControlPoint();
			break;
		case LINEAR:
			myPoint = new LinearControlPoint();
			break;
		case BEZIER:
			myPoint = new BezierControlPoint();
			break;
		case MARKER:
			myPoint = new MarkerPoint();
			break;
		case TIMED_EVENT:
			myPoint = new TimedEventPoint();
			break;
		default:
			myPoint = new LinearControlPoint();
		}
		myPoint.data(theData);
		return myPoint;
	}
	
	public void data(CCDataObject theData) {
		if(theData == null)return;
		CCDataArray myPointDataArray = theData.getArray("points");
		for (Object myControlPointObject:myPointDataArray) {
			CCDataObject myControlPointData = (CCDataObject)myControlPointObject;
			Add(createPoint(myControlPointData));
		}
		
	}
	
	public void insert(CCDataObject theData, float theTime) {
		CCDataArray myPointDataArray = theData.getArray("points");
		for (Object myControlPointObject:myPointDataArray) {
			CCDataObject myControlPointData = (CCDataObject)myControlPointObject;
			ControlPoint myPoint = createPoint(myControlPointData);
			myPoint.time(myPoint.Time + theTime);
			Add(myPoint);
		}
		
	}*/
}
}