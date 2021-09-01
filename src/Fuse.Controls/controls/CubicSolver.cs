using System;

namespace Fuse.Controls
{
    public static class CubicSolver
    {
        // cubic equation solver example using Cardano's method

        private const double Third = 0.333333333333333;
        private const double RootThree = 1.73205080756888;

        // this function returns the cube root if x were a negative number as well
	
	/**
	 * Returns the cube root of the given Value, also for negative x values
	 */
	private static double CubeRoot(double theX) {
		if (theX < 0)
			return -Math.Pow(-theX, Third);
		else
			return Math.Pow(theX, Third);
	}

	public static float[] SolveCubic(double a, double b, double c, double d) {
		var myResult = new float[3];
		
		// find the discriminant
		var f = (3 * c / a - Math.Pow(b, 2) / Math.Pow(a, 2)) / 3;
		var g = (2 * Math.Pow(b, 3) / Math.Pow(a, 3) - 9 * b * c / Math.Pow(a, 2) + 27 * d / a) / 27;
		var h = Math.Pow(g, 2) / 4 + Math.Pow(f, 3) / 27;

		// evaluate discriminant
		if (f == 0 && g == 0 && h == 0) {
			// 3 equal roots
			// when f, g, and h all equal 0 the roots can be found by the following line
			var x = -CubeRoot(d / a);
			myResult[0] = (float)x;
			myResult[1] = (float)x;
			myResult[2] = (float)x;
			return myResult;
		}

		double p;
		if (h <= 0) {
			// 3 real roots
			// complicated math making use of the method
			var i = Math.Pow(Math.Pow(g, 2) / 4 - h, 0.5);
			var j = CubeRoot(i);
			var k = Math.Acos(-(g / (2 * i)));
			var m = Math.Cos(k / 3);
			var n = RootThree * Math.Sin(k / 3);
			p = -(b / (3 * a));
			
			myResult[0] = (float)(2 * j * m + p);
			myResult[1] = (float)(-j * (m + n) + p);
			myResult[2] = (float)(-j * (m - n) + p);
			return myResult;
		}

		if (!(h > 0)) return myResult;
		
		// 1 real root and 2 complex roots
		// complicated maths making use of the method
		var r = -(g / 2) + Math.Pow(h, 0.5);
		var s = CubeRoot(r);
		var t = -(g / 2) - Math.Pow(h, 0.5);
		var u = CubeRoot(t);
		p = -(b / (3 * a));
			
		myResult[0] = (float)(s + u + p);
		myResult[1] = (float)(-(s + u) / 2 + p);
		myResult[2] = (float)(-(s + u) / 2 + p);
		
		return myResult;
	}
    }
}