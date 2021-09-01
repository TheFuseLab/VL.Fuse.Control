using System;
using System.Collections.Generic;

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
    public class AnimationCurve : TreeSet<ControlPoint>
    {
        private int _mySize = 0;
        private bool _myDirtyFlag = false;

		public AnimationCurve():base(new ControlPointComparer()) {
			
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
		
			foreach(var myPoint in myRangeCopy){
				var myNewTime = theEndTime - myPoint.Time + theStartTime;
				myPoint.Time = myNewTime;
				Add(myPoint);
			}
		}
	

		public new void Add(ControlPoint thePoint) {

			_mySize += 1;
			_myDirtyFlag = true;
		
			if (Contains(thePoint)) {
				var myTreeLeaf = Floor(thePoint);
				if (myTreeLeaf.Equals(thePoint)) {
					_mySize -=1;
					_myDirtyFlag = false;
					return;
				}
				if (thePoint.isPrevious(myTreeLeaf)) {
					InsertBefore(myTreeLeaf, thePoint);
				} else {
					myTreeLeaf = getLastOnSamePosition(myTreeLeaf);
					// do not insert into the tree, just update the float linked list
					myTreeLeaf.append(thePoint);
				}
			} else { // new leaf
				base.Add(thePoint);
				thePoint.CutLoose();
				var myLower = Lower(thePoint);
				if (myLower != null) {
					myLower = getLastOnSamePosition(myLower);
					myLower.append(thePoint);
				}
				var myHigher = Higher(thePoint);
				if (myHigher != null) {
					myHigher.prepend(thePoint);
				}
			}
		}

		public static ControlPoint CreateSamplePoint(float theTime) {
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

		private const float TOLERANCE = 0.0001f;

		public ControlPoint getLastOnSamePosition(ControlPoint thePoint)
		{
			if (!Contains(thePoint)) {
				return thePoint;
			}

			var myTreeLeaf = Floor(thePoint);
			if(myTreeLeaf == null)return thePoint;
			while (myTreeLeaf.hasNext())
			{
				if (Math.Abs(myTreeLeaf.getNext().Time - thePoint.Time) > TOLERANCE) {
					return myTreeLeaf;
				}

				myTreeLeaf = myTreeLeaf.getNext();
			}
			return myTreeLeaf;
		}

		public ControlPoint GetLastPointAt(float theTime) {
			return getLastOnSamePosition(Ceiling(new ControlPoint(theTime, 0)));
		}

		public ControlPoint GetLastPoint() {
			try {
				var myLast = last();
				return getLastOnSamePosition(myLast);
			} catch (Exception e) {
				return null;
			}
		}
	
		/*
		public float GetLastTime{
			var myLastPoint = GetLastPoint();
			if (myLastPoint != null) {
				return myLastPoint.Time;
			}
			return 0;
		}
*/
	    /**
	     * Returns the accumulated value for the given time
	     * @param theTime
	     * @return
	     */
	    public float GetAccumulatedValue(float theTime) {
    		return Value(theTime);
	    }

		public float Value(float theTime) {
			if (Count() == 0) {
				return 0.0f;
			}
			
			var mySample = CreateSamplePoint(theTime);
			var myLower = Lower(mySample);
			var myCeiling = Ceiling(mySample);

			if (myLower == null)
			{
				return myCeiling?.Value ?? 0.0f;
			}

			myLower = getLastOnSamePosition(myLower);

			if (myCeiling != null) {
				return myCeiling.InterpolateValue(theTime, this);
			} else {
				if (myLower != null) {
					return myLower.Value;
				}
			}
			return 0.0f;
		}

		private void InsertBefore(ControlPoint theLocation, ControlPoint theInsertion) {
			theLocation.prepend(theInsertion);
			// the first element in the float linked list at the same x position
			// becomes the new tree leaf
			if (!(Math.Abs(theLocation.Time - theInsertion.Time) < TOLERANCE)) return;
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

			myMaxPoint = getLastOnSamePosition(myMaxPoint);
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

			foreach (var myControlPoint in myRange)
			{
				var myNext = myControlPoint.Clone();
				myNext.CutLoose();
				myControlPoint.setNext(myNext);
				myNext.setPrevious(myControlPoint);
				myCopy.Add(myNext);
			}

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

		
		public void Remove(ControlPoint thePoint) {
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
				myLower = getLastOnSamePosition(myLower);
				myLower.setNext(myHigher);
				if (myHigher != null) {
					myHigher.setPrevious(myLower);
				}
			} else if (myHigher != null) {
				myHigher.setPrevious(null);
			}
			foreach (ControlPoint myCurrentPoint in myRange) {
				Remove(myCurrentPoint);
			}
		}

		public void ReplaceAll(float theKey, float theRange, List<ControlPoint> theArray) {
			if (theArray.Count == 0) {
				return;
			}
			
			RemoveAll(theKey, theKey + theRange);
			foreach (var myPoint in theArray) {
				myPoint.CutLoose();
				var myClone = myPoint.Clone();
				myClone.Time = myPoint.Time + theKey;
				Add(myClone);
			}
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
			while (myCurrentPoint != null) {
				myCurrentPoint.Time = myCurrentPoint.Time + theTime;
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
				var myPoint = myCurrentPoint.Clone();
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
			
			foreach(var myControlPoint in myRange) {
				myTime = myControlPoint.Time - theStartTime;
				myScaledTime = myTime * theScale;
				myMoveRange = myScaledTime - myTime;
				
				Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myMoveRange, myCurrentPoint.Value));
				myCurrentPoint = myCurrentPoint.getNext();
			}
		}
		
		public void ScaleRange(float theStartTime1, float theEndTime1, float theStartTime2, float theEndTime2) {
			if(theEndTime2 - theStartTime2 > theEndTime1 - theStartTime1){
				//move points at the end
				var myCurrentPoint = Ceiling(new ControlPoint(theEndTime1, 0));
				var myEndMove = theEndTime2 - theEndTime1;
				while (myCurrentPoint != null) {
					Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myEndMove, myCurrentPoint.Value));
					myCurrentPoint = myCurrentPoint.getNext();
				}
				
				//move points at the start
				myCurrentPoint = Floor(new ControlPoint(theStartTime1, 0));
				var myStartMove = theStartTime2 - theStartTime1;
				while (myCurrentPoint != null) {
					Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myStartMove, myCurrentPoint.Value));
					myCurrentPoint = myCurrentPoint.getPrevious();
				}
				
				var myRange = RangeList(theStartTime1, theEndTime1);
				
				foreach(var myControlPoint in myRange) {
					float myNewTime = CCMath.map(myControlPoint.Time, theStartTime1, theEndTime1, theStartTime2, theEndTime2);
					Move(myControlPoint, new ControlPoint(myNewTime, myControlPoint.Value));
					//var controlPoint = myControlPoint;
					//myControlPoint = myControlPoint.getNext();
				}
			}else{
				var myRange = RangeList(theStartTime1, theEndTime1);
				
				foreach(var myControlPoint in myRange) {
					float myNewTime = CCMath.map(myControlPoint.Time, theStartTime1, theEndTime1, theStartTime2, theEndTime2);
					Move(myControlPoint, new ControlPoint(myNewTime, myControlPoint.Value));
				}
				
				//move points at the end
				var myCurrentPoint = Ceiling(new ControlPoint(theEndTime1, 0));
				var myEndMove = theEndTime2 - theEndTime1;
				while (myCurrentPoint != null) {
					Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myEndMove, myCurrentPoint.Value));
					myCurrentPoint = myCurrentPoint.getNext();
				}
				
				//move points at the start
				myCurrentPoint = Floor(new ControlPoint(theStartTime1, 0));
				var myStartMove = theStartTime2 - theStartTime1;
				while (myCurrentPoint != null) {
					Move(myCurrentPoint, new ControlPoint(myCurrentPoint.Time + myStartMove, myCurrentPoint.Value));
					myCurrentPoint = myCurrentPoint.getPrevious();
				}
			}
			
		}

		public bool IsLeaf(ControlPoint thePoint) {
			if (!Contains(thePoint)) return false;
			ControlPoint myTreeLeaf = Floor(thePoint);
			return myTreeLeaf.Equals(thePoint);
		}
	}
}