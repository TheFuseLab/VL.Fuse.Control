namespace Fuse.Controls
{
    public class HandleControlPoint : ControlPoint
    {
        private HandleType _myHandleType;

        public HandleControlPoint(ControlPoint theParent, HandleType theHandleType) : base(ControlPointType.HANDLE) {
            Parent = theParent;
            _myHandleType = theHandleType;
        }

        public HandleControlPoint(ControlPoint theParent, HandleType theHandleType, float theTime, float theValue) : base(theTime, theValue, ControlPointType.HANDLE){
            Parent = theParent;
            _myHandleType = theHandleType;
        }
	
        public ControlPoint Parent
        {
            get;
            set;
        }
	
        public HandleType handleType() {
            return _myHandleType;
        }
        
        public override ControlPoint Clone() {
            return new HandleControlPoint(Parent, _myHandleType, _myTime, _myValue);
        }
    }
}