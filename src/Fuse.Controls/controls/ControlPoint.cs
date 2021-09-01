using System;

namespace Fuse.Controls
{
    public class ControlPoint
    {
        public enum ControlPointType {
		STEP, 
		LINEAR, 
		BEZIER, 
		HANDLE, 
		MARKER, 
		TIMED_EVENT, 
		TIMED_DATA_START,
		TIMED_DATA_END
	}

    public enum HandleType{
		BEZIER_IN_HANDLE, BEZIER_OUT_HANDLE, TIME_END
	}
	
	

	/**
	 * Type of the curve after this point
	 */
	private ControlPointType _myType;
	
	protected ControlPoint _myPrevious;
	protected ControlPoint _myNext;

	public ControlPoint() :this(0, 0, ControlPointType.LINEAR){
		
	}
	
	public ControlPoint(ControlPointType theControlPointType) :this(0, 0, theControlPointType){
		
	}

	public ControlPoint(float theTime, float theValue): this(theTime, theValue, ControlPointType.LINEAR) {
		
	}
	
	public ControlPoint(float theTime, float theValue, ControlPointType theControlPointType) {
		Time = theTime;
		Value = theValue;
		
		_myType = theControlPointType;
		_myPrevious = null;
		_myNext = null;
	}
	/*
	protected CCBlendable<?> _myBlendable = null;
	
	public CCBlendable<?> blendable(){
		return _myBlendable;
	}
	
	public void blendable(CCBlendable<?> theBlendable){
		_myBlendable = theBlendable;
	}*/
	
	public virtual bool hasHandles() {
		return false;
	}

	/**
	 * @return the _myType
	 */
	public ControlPointType getType() {
		return _myType;
	}

	/**
	 * @param myType the _myType to set
	 */
	public void setType(ControlPointType theType) {
		_myType = theType;
	}
	
	public ControlPoint getPrevious() {
		return _myPrevious;
	}
	
	public void setPrevious( ControlPoint thePoint ) {
		if (thePoint == this) {
			return;
		}
		_myPrevious = thePoint;
	}
	
	public ControlPoint getNext() {
		return _myNext;
	}
	
	public void setNext( ControlPoint thePoint ) {
		if (thePoint == this) {
			return;
		}
		_myNext = thePoint;
	}
	
	public void append( ControlPoint thePoint) {
		if (thePoint == this) {
			return;
		}
		if (thePoint != null) {
			thePoint.setPrevious(this);
			if (_myNext != null) {
				_myNext.setPrevious(thePoint);
				thePoint.setNext(_myNext);
			}
		}
		_myNext = thePoint;
	}
	
	public void prepend( ControlPoint thePoint ) {
		if (thePoint == this) {
			return;
		}
		if (thePoint != null) {
			thePoint.setNext(this);
			if (_myPrevious != null) {
				_myPrevious.setNext( thePoint );
				thePoint.setPrevious(_myPrevious);
			}
		}
		_myPrevious = thePoint;
	}
	
	public bool hasNext() {
		return _myNext != null;
	}
	
	public bool hasPrevious() {
		return _myPrevious != null;
	}
	
	public virtual float Time
	{
		get;
		set;
	}

	public virtual float InterpolateValue(float theTime, AnimationCurve theData) {
		return Value;
	}
	
	public float Value
	{
		get;
		set;
	}

    public float distance(ControlPoint theOtherPoint) {
        var myTimeDistance = Time - theOtherPoint.Time;
        var myValueDistance = Value - theOtherPoint.Value;
        return (float)Math.Sqrt(myTimeDistance*myTimeDistance + myValueDistance*myValueDistance);
    }
	
	public bool isPrevious(ControlPoint thePoint) {
		if (thePoint.Time > Time) {
			return true;
		} else if (thePoint.Time < Time) {
			return false;
		}
		var myCurrent = _myNext;
		while ( myCurrent != null ) {
			if (myCurrent == thePoint) {
				return true;
			}
			myCurrent = myCurrent.getNext();
		}
		return false;
	}
	
	public bool IsNext(ControlPoint thePoint) {
		if (thePoint.Time < Time) {
			return true;
		} else if (thePoint.Time > Time) {
			return false;
		}
		var myCurrent = _myPrevious;
		while( myCurrent != null ) {
			if (myCurrent == thePoint) {
				return true;
			}
			myCurrent = myCurrent.getPrevious();
		}
		return false;
	}
	
	public void CutLoose() {
		_myNext = null;
		_myPrevious = null;
	}
	
	public virtual ControlPoint Clone() {
		return new ControlPoint(Time, Value);
	}
	

	public override bool Equals(object theObj) {
		if ((theObj == null) || GetType() != theObj.GetType())
		{
			return false;
		}
		return ((ControlPoint)theObj).Time == Time && ((ControlPoint)theObj).Value == Value;
	}
	
	public override string ToString() {
		return "type: " + _myType + " time: " + Time + " Value:" + Value;
	}
	
	
	
	private bool _myIsSelected = false;

	public void setSelected(bool theIsSelected) {
		_myIsSelected = theIsSelected;
	}
	
	public bool isSelected(){
		return _myIsSelected;
	}

	public void toggleSelection() {
		_myIsSelected = !_myIsSelected;
	}
	
	protected static  string CONTROLPOINT_ELEMENT = "ControlPoint";

	public  static  string CONTROL_POINT_TYPE_ATTRIBUTE = "type";

	protected static  string TIME_ATTRIBUTE = "time";
	protected static  string VALUE_ATTRIBUTE = "Value";
	protected static  string BLENDABLE_ATTRIBUTE = "blendable";
	
	/*
	public CCDataObject data(float theStartTime, float theEndTime) {
		CCDataObject myResult = new CCDataObject();
		myResult.put(CONTROL_POINT_TYPE_ATTRIBUTE, _myType.toString());
		myResult.put(TIME_ATTRIBUTE, Time - theStartTime);
		myResult.put(VALUE_ATTRIBUTE, Value);
		if(_myBlendable != null) {
			myResult.put(BLENDABLE_ATTRIBUTE, _myBlendable.data());
		}
		return myResult;
	}
	
	protected static final String  COLOR_TYPE = CCColor.class.getName();
	
	public void data(CCDataObject theData) {
		Time = theData.getfloat(TIME_ATTRIBUTE);
		Value = theData.getfloat(VALUE_ATTRIBUTE);
		if(!theData.containsKey(BLENDABLE_ATTRIBUTE))return;
		CCDataObject myBlendableData = theData.getObject(BLENDABLE_ATTRIBUTE);
		if(myBlendableData.getString(CCBlendable.BLENDABLE_TYPE_ATTRIBUTE).equals(CCColor.class.getName())) {
			_myBlendable = new CCColor();
		}else if(myBlendableData.getString(CCBlendable.BLENDABLE_TYPE_ATTRIBUTE).equals(CCGradient.class.getName())) {
			_myBlendable = new CCGradient();
		}
		_myBlendable.data(myBlendableData);
		
	}*/
    }
}