namespace Fuse.Controls
{
    public class StepControlPoint : ControlPoint
    {
        public StepControlPoint() : base(ControlPointType.STEP) {
        }

        public StepControlPoint(float theTime, float theValue) : base(theTime, theValue, ControlPointType.STEP){
            ;
        }
	
        public StepControlPoint(ControlPoint theControlPoint): this(theControlPoint.Time, theControlPoint.Value) {
            ;
        }
	
        public override float InterpolateValue(float theTime, AnimationCurve theData) {
            var myPrevious = getPrevious();
		
            return myPrevious?.Value ?? Value;
        }
	
        public override ControlPoint Clone() {
            var myCopy = new StepControlPoint(Time, Value);
            //if(_myBlendable!= null)myCopy._myBlendable = (CCBlendable<?>)_myBlendable.clone();
            return myCopy;
        }
    }
}