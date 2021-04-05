using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geo : MonoBehaviour
{


    public static float ToAng(Vector2 a) {
        return Degreed(Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg);
    }

    public static float ToAng(Vector2 a, Vector2 b) { 
        return ToAng(b - a);
    }

    public static float Degreed(float a)
    {
        return Geo.Mod(a, 360f);
    }

    public static float Mod(float a, float b)
    {
        if (b < 0)
        {
            Debug.Log("MOD IS BEING GIVEN A NEGATIVE");
        }
        while (a < 0)
        {
            a += b;
        }
        while (a >= b)
        {
            a -= b;
        }
        return a;
    }

    
    /*

    static def AddDegrees(a as Vector2, b as single):
        return ToVect(ToAng(a) + b)


    static def ToVect(a as single):
        return Vector2(Mathf.Cos(a* Mathf.Deg2Rad), Mathf.Sin(a* Mathf.Deg2Rad))

    static def AngDist(ang0 as single, ang1 as single):
        ang0 = Geo.Degreed(ang0)
        ang1 = Geo.Degreed(ang1)
        tmp = Mathf.Abs(ang1 - ang0)
        if (tmp > 180):
            tmp = Mathf.Abs(tmp - 360)
        return tmp

    static def AngDelta(ang0 as single, ang1 as single):
        ang0 = Geo.Degreed(ang0)
        ang1 = Geo.Degreed(ang1)
        tmp = ang1 - ang0
        if Mathf.Abs(tmp) > 180:
            if tmp > 0:
                tmp -= 360
            else:
                tmp += 360
        return tmp

    static def MidPoint(a as Vector2, b as Vector2):
        return b + ((a - b).normalized* ((a - b).magnitude / 2))


    static def ToV3(v2 as Vector2):
        return Vector3(v2.x, v2.y, 0)

    static def ToV2(v3 as Vector3):
        return Vector2(v3.x, v3.y)

    static def Angle(v0 as Vector2, v1 as Vector2):
        return Degreed(Mathf.Atan2((v1 - v0).y, (v1-v0).x) * Mathf.Rad2Deg)

    static def Angle(a as GameObject, b as GameObject):
        return Angle(a.transform.position, b.transform.position)

    static def IsBetween(a as single, b as single, c as single):
        a = Degreed(a)
        b = Degreed(b)
        c = Degreed(c)

        if Mathf.Abs(b - c) > 180:
            return a <= Mathf.Min(b, c) or a >= Mathf.Max(b, c)
        else:
            return a >= Mathf.Min(b, c) and a <= Mathf.Max(b, c)
    

    static def IsLeft(a as Vector2, b as Vector2, c as Vector2):
        return ((b.x - a.x)*(c.y - a.y) - (b.y - a.y)*(c.x - a.x)) > 0

    static def PerpVectL(a as Vector2, b as Vector2):
        //Returns Vector facing left
        v0 = ToV3(a)
        v1 = ToV3(b)
        v3 = Vector3(0, 0, -1)
        return ToV2(Vector3.Cross((v1 - v0), v3)).normalized

    static def PerpVectR(a as Vector2, b as Vector2):
        //Returns Vector facing left
        v0 = ToV3(a)
        v1 = ToV3(b)
        v3 = Vector3(0, 0, 1)

        return ToV2(Vector3.Cross((v1 - v0), v3)).normalized
    static def PerpVect(v as Vector2, right as bool):
        if right:
            return Vector3.Cross(v, Vector3(0, 0, 1)).normalized
        else:
            return Vector3.Cross(v, Vector3(0, 0, -1)).normalized

    static def PerpVect(a as Vector2, b as Vector2, right as bool):
        if right:
            return(PerpVectR(a, b))
        else:
            return(PerpVectL(a, b))

    static def PerpVect(v0 as Vector2, v1 as Vector2, myPos as Vector2):
        if v0.x == v1.x:
            if myPos.x > v0.x:
                return Vector2(-1, 0)
            else:
                return Vector2(1, 0)
        elif v0.y == v1.y:
            if myPos.y > v0.y:
                return Vector2(0, -1)
            else:
                return Vector2(0, 1)

        tmp = (v0 - v1).normalized
        slope = tmp.y / tmp.x
        nrecip = - (1 / slope)
        y1 = nrecip
        tmp = Vector2(1, y1).normalized
        if IsLeft(v0, v1, myPos) == IsLeft(v0, v1, v0 + tmp) :
            return Vector2(tmp.x* -1, tmp.y* -1)
        else:
            return tmp

    static def SideMost(vts as (Vector3), viewPos as Vector2, myPos as Vector2, right as bool):
        ang = Geo.ToAng(myPos - viewPos)
        if right:
            num as single = -999
            for vt in vts:
                if Geo.AngDelta(ang, Geo.ToAng(vt - viewPos)) > num:
                    pt = vt
                    num = Geo.AngDelta(ang, Geo.ToAng(vt - viewPos))
        else:
            num = 999
            for vt in vts:
                if Geo.AngDelta(ang, Geo.ToAng(vt - viewPos)) < num:
                    pt = vt
                    num = Geo.AngDelta(ang, Geo.ToAng(vt - viewPos))
        return pt


    static def SideMost(vts as (Vector2), viewPos as Vector2, myPos as Vector2, right as bool):
        ang = Geo.ToAng(myPos - viewPos)
        if right:
            num as single = -999
            for vt in vts:
                if Geo.AngDelta(ang, Geo.ToAng(vt - viewPos)) > num:
                    pt = vt
                    num = Geo.AngDelta(ang, Geo.ToAng(vt - viewPos))
        else:
            num = 999
            for vt in vts:
                if Geo.AngDelta(ang, Geo.ToAng(vt - viewPos)) < num:
                    pt = vt
                    num = Geo.AngDelta(ang, Geo.ToAng(vt - viewPos))
        return pt

    static def ClosestPoint(v0 as Vector2, v1 as Vector2, v2  as Vector2):
        if (v0.x == v1.x):
            return Vector2(v0.x, v2.y)
        elif(v0.y == v1.y) :
            return Vector2(v2.x, v0.y)
        v02 = v2 - v0
        v01 = v1 - v0
        v01sq = (v01.x* v01.x) + (v01.y* v01.y)

        v02dotv01 = Vector2.Dot(v02, v01)

        t = v02dotv01 / v01sq

        return Vector2(v0.x + (v01.x* t), v0.y + (v01.y* t))




    static def LineLineIntersection(ref intersection as Vector3,
                                    linePoint1 as Vector3,
                                    lineVec1 as Vector3,
                                    linePoint2 as Vector3,
                                    lineVec2 as Vector3):


        intersection = Vector3.zero


        lineVec3 as Vector3 = linePoint2 - linePoint1
        crossVec1and2 as Vector3 = Vector3.Cross(lineVec1, lineVec2)
        crossVec3and2 as Vector3 = Vector3.Cross(lineVec3, lineVec2)

        planarFactor = Vector3.Dot(lineVec3, crossVec1and2)
 
        //Lines are not coplanar. Take into account rounding errors.
        if ((planarFactor >= 0.00001f) or(planarFactor <= -0.00001f)):
            Debug.Log("NOT COPLANAR")
            return false
            
 
        //Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
        s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude
 
        if ((s >= 0.0f) and(s <= 1.0f)):
            intersection = linePoint1 + (lineVec1* s)
            return true
        else:
            //Debug.Log("Not THE OTHER THING")
            return false     
            */
        

}
