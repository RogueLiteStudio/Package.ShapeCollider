using UnityEngine;
namespace ShapeCollider
{
    public static class ColliderOverlapUtil
    {
        private static Vector2 ToV2(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        private static float ProjectRadius(float radius, float offsetToCenter)
        {
            return Mathf.Sqrt((radius * radius) - (offsetToCenter * offsetToCenter));
        }
        public static Vector3 ClosestPoint(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 direction = end - start;

            float t = direction.sqrMagnitude;
            if (t < 0.000001f) return start;

            t = Mathf.Clamp01(Vector3.Dot(point - start, direction) / t);
            return start + direction * t;
        }
        public static float SegmentSDFSqr(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 ap = point - from;
            Vector2 ab = to - from;
            float h = Mathf.Clamp01(Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab));
            return (ap - h * ab).sqrMagnitude;
        }
        #region 球形
        public static bool Overlap(in ShapeSphere c1, in ShapeSphere c2)
        {
            float sqrRadius = c1.Radius + c2.Radius;
            sqrRadius *= sqrRadius;
            return Vector3.SqrMagnitude(c1.Position - c2.Position) <= sqrRadius;
        }
        public static bool Overlap(in ShapeSphere c1, in ShapeRay c2, out float t)
        {
            Vector3 oc = c1.Position - c2.Position;
            t = 0;
            float projection = Vector3.Dot(oc, c2.Direction);
            if (projection < 0)
                return false;
            float oc2 = Vector3.Dot(oc, oc);
            float distance2 = oc2 - projection * projection;
            float radiusSquare = c1.Radius * c1.Radius;
            if (distance2 > radiusSquare)
                return false;
            float discriminant = radiusSquare - distance2;
            if (discriminant < float.Epsilon)
            {
                t = projection;
            }
            else
            {
                discriminant = Mathf.Sqrt(discriminant);
                t = projection - discriminant;
            }
            return t < c2.Length;
        }
        //判断圆与
        public static bool Overlap(in ShapeSphere c1, in ShapeCylinder c2)
        {
            float radius = c1.Radius;
            if (c1.Position.y < c2.Position.y)
            {//球在下方
                //不相接
                if (c1.Position.y + c1.Radius <= c2.Position.y)
                    return false;
                //计算球与底部相交的圆半径
                radius = ProjectRadius(radius, c2.Position.y - c1.Position.y);
            }
            else if (c1.Position.y > (c2.Position.y + c2.Height))
            {//在上方
                //不相接
                if (c1.Position.y - c1.Radius >= (c2.Position.y + c2.Height))
                    return false;
                //计算球与顶部相交的圆半径
                radius = ProjectRadius(radius, c1.Position.y - (c2.Position.y + c2.Height));
            }
            float sqrRadius = radius + c2.Radius;
            sqrRadius *= sqrRadius;
            return sqrRadius > Vector2.SqrMagnitude(c1.Position.ToV2() - c2.Position.ToV2());
        }
        
        public static bool Overlap(in ShapeSphere c1, in ShapeBox c2)
        {
            float radius = c1.Radius;
            if (c1.Position.y < c2.Position.y - c2.Extern.y)
            {//球在下方
                //不相接
                if (c1.Position.y + c1.Radius <= (c2.Position.y - c2.Extern.y))
                    return false;
                //计算球与底部相交的圆半径
                radius = ProjectRadius(radius, (c2.Position.y - c2.Extern.y) - c1.Position.y);
            }
            else if (c1.Position.y > (c2.Position.y + c2.Extern.y))
            {//在上方
                //不相接
                if ((c1.Position.y - c1.Radius) >= (c2.Position.y + c2.Extern.y))
                    return false;
                //计算球与顶部相交的圆半径
                radius = ProjectRadius(radius, c1.Position.y - (c2.Position.y + c2.Extern.y));
            }
            float sdf = ShapeSDFUtil.OrientedBoxSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Extern.ToV2());
            return sdf <= radius;
        }
        public static bool Overlap(in ShapeSphere c1, in ShapePie c2)
        {
            float radius = c1.Radius;
            if (c1.Position.y < c2.Position.y)
            {//球在下方
                //不相接
                if (c1.Position.y + c1.Radius <= c2.Position.y)
                    return false;
                //计算球与底部相交的圆半径
                radius = ProjectRadius(radius, c2.Position.y - c1.Position.y);
            }
            else if (c1.Position.y > (c2.Position.y + c2.Height))
            {//在上方
                //不相接
                if (c1.Position.y - c1.Radius >= (c2.Position.y + c2.Height))
                    return false;
                //计算球与顶部相交的圆半径
                radius = ProjectRadius(radius, c1.Position.y - (c2.Position.y + c2.Height));
            }
            float sdf = ShapeSDFUtil.SectorSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Angle, c2.Radius);
            return sdf <= radius;
        }

        public static bool Overlap(in ShapeBox c1, in ShapeSphere c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapePie c1, in ShapeSphere c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapeRay c1, in ShapeSphere c2, out float t) { return Overlap(c2, c1, out t); }
        #endregion

        #region 圆柱
        public static bool Overlap(in ShapeCylinder c1, in ShapeSphere c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapeCylinder c1, in ShapeCylinder c2)
        {
            if (c1.Position.y + c1.Height < c2.Position.y)
                return false;
            if (c1.Position.y > (c2.Position.y + c2.Height))
                return false;

            float sqrRadius = c1.Radius + c2.Radius;
            sqrRadius *= sqrRadius;
            return sqrRadius >= Vector2.SqrMagnitude(c1.Position.ToV2() - c2.Position.ToV2());
        }
        //圆柱跟矩形，计算出点到一个矩形的距离，如果这个距离小于圆柱的半径，就证明是会发生碰撞的
        public static bool Overlap(in ShapeCylinder c1, in ShapeBox c2)
        {
            if (c1.Position.y + c1.Height < c2.Position.y - c2.Extern.y)
                return false;
            if (c1.Position.y > c2.Position.y + c2.Extern.y)
                return false;

            float sdf = ShapeSDFUtil.OrientedBoxSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Extern.ToV2());
            return sdf <= c1.Radius;
        }
        public static bool Overlap(in ShapeCylinder c1, in ShapePie c2)
        {
            if (c1.Position.y + c1.Height < c2.Position.y)
                return false;
            if (c1.Position.y > c2.Position.y + c2.Height)
                return false;
            float sdf = ShapeSDFUtil.SectorSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Angle, c2.Radius);
            return sdf <= c1.Radius;
        }
        public static bool Overlap(in ShapeCylinder c1, in ShapeRay c2, out float t)
        {
            t = 0;
            float top = c1.Position.y + c1.Height;
            float bottom = c1.Position.y;
            Vector3 end = c2.Position + c2.Direction * c2.Length;
            if (c2.Position.y < bottom && end.y < bottom)
                return false;
            if (c2.Position.y > top && end.y > top)
                return false;
            float radiusSquare = c1.Radius * c1.Radius;
            Vector2 center = c1.Position.ToV2();
            Vector2 lineStart = c2.Position.ToV2();
            Vector2 lineEnd = end.ToV2();
            //如果在平面上圆和线段不相交则返回
            if (SegmentSDFSqr(center, lineStart, lineEnd) > radiusSquare)
                return false;
            Vector2 startToCenter = lineStart - center;
            Vector2 endToCenter = lineEnd - center;
            bool isStartInCircle = startToCenter.sqrMagnitude < radiusSquare;
            bool isEndInCircle = endToCenter.sqrMagnitude < radiusSquare;
            t = float.MaxValue;
            {//处理和顶部或者底部相交的情况
                if (new Plane(Vector3.up, -top).Raycast(new Ray(c2.Position, c2.Direction), out float enterTop)
                    && enterTop < c2.Length && enterTop > 0)
                {
                    Vector2 pt = (c2.Position + c2.Direction * enterTop).ToV2();
                    if ((pt - center).sqrMagnitude <= radiusSquare)
                    {//交点在圆柱内
                        t = Mathf.Min(t, enterTop);
                    }
                }
                if (new Plane(Vector3.up, -bottom).Raycast(new Ray(c2.Position, c2.Direction), out float enterBottom)
                    && enterBottom < c2.Length && enterBottom > 0)
                {
                    Vector2 pt = (c2.Position + c2.Direction * enterBottom).ToV2();
                    if ((pt - center).sqrMagnitude <= radiusSquare)
                    {//交点在圆柱内
                        t = Mathf.Min(t, enterBottom);
                    }
                }
            }

            do
            {
                if (isStartInCircle && isEndInCircle)
                {//两个点都在圆内，要么和顶部或者底部相交，要么在圆柱内部，内部这里也算相交
                    t = Mathf.Min(t, c2.Length);
                    break;
                }

                Vector2 lineDir = lineEnd - lineStart;
                float sqrMagnitude = lineDir.sqrMagnitude;
                if (sqrMagnitude < 1E-05f)
                    break;//射线垂直方向，只与顶部或者底部相交
                lineDir /= Mathf.Sqrt(sqrMagnitude);

                float b = Vector2.Dot(startToCenter, lineDir);// b大于0，说明射线方向背向圆心
                float c = Vector2.Dot(startToCenter, startToCenter) - radiusSquare;
                // 如果射线起点在圆外，并且方向与到圆方向相反，则不相交,前面处理过，正常这里不会判断失败
                if (c > 0 && b > 0)
                    return false;
                float discr = b * b - c;
                // 判别式小于0。前面处理过，正常这里不会判断失败
                if (discr < 0f)
                    return false;
                float enter = -b - Mathf.Sqrt(discr); // -b-sqrt(b*b-c)表示从射线起点比较近的相交点
                if (enter < 0f)                     // 射线的长度t值应该为正方向，最小为0
                    enter = 0f;
                Vector3 hDir = c2.Direction;//射线水平方向朝向
                hDir.y = 0;
                hDir.Normalize();
                float dot = Vector3.Dot(c2.Direction, hDir);
                enter /= dot;
                if (enter < c2.Length)
                {
                    Vector3 p = c2.Position + c2.Direction *enter;
                    if (p.y >= bottom && p.y <= top)
                    {
                        t = Mathf.Min(t, enter);
                    }
                }

            } while (false);

            return t <= c2.Length;
        }

        public static bool Overlap(in ShapeBox c1, in ShapeCylinder c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapePie c1, in ShapeCylinder c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapeRay c1, in ShapeCylinder c2, out float t) { return Overlap(c2, c1, out t); }
        #endregion

    }
}