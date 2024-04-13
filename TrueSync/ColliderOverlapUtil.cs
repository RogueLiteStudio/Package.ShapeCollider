using TrueSync;
namespace TShapeCollider
{
    public static class ColliderOverlapUtil
    {
        private static TVector2 ToV2(this TVector3 v)
        {
            return new TVector2(v.x, v.z);
        }

        private static TFloat ProjectRadius(TFloat radius, TFloat offsetToCenter)
        {
            return TMath.Sqrt((radius * radius) - (offsetToCenter * offsetToCenter));
        }
        public static TVector3 ClosestPoint(TVector3 start, TVector3 end, TVector3 point)
        {
            TVector3 direction = end - start;

            TFloat t = direction.sqrMagnitude;
            if (t < TFloat.EN6) return start;

            t = TMath.Clamp01(TVector3.Dot(point - start, direction) / t);
            return start + direction * t;
        }
        public static TFloat SegmentSDFSqr(TVector2 point, TVector2 from, TVector2 to)
        {
            TVector2 ap = point - from;
            TVector2 ab = to - from;
            TFloat h = TMath.Clamp01(TVector2.Dot(ap, ab) / TVector2.Dot(ab, ab));
            return (ap - h * ab).sqrMagnitude;
        }
        #region 球形
        public static bool Overlap(in ShapeSphere c1, in ShapeSphere c2)
        {
            TFloat sqrRadius = c1.Radius + c2.Radius;
            sqrRadius *= sqrRadius;
            return TVector3.SqrMagnitude(c1.Position - c2.Position) <= sqrRadius;
        }
        public static bool Overlap(in ShapeSphere c1, in ShapeRay c2, out TFloat t)
        {
            TVector3 oc = c1.Position - c2.Position;
            t = 0;
            TFloat projection = TVector3.Dot(oc, c2.Direction);
            if (projection < 0)
                return false;
            TFloat oc2 = TVector3.Dot(oc, oc);
            TFloat distance2 = oc2 - projection * projection;
            TFloat radiusSquare = c1.Radius * c1.Radius;
            if (distance2 > radiusSquare)
                return false;
            TFloat discriminant = radiusSquare - distance2;
            if (discriminant < TFloat.Epsilon)
            {
                t = projection;
            }
            else
            {
                discriminant = TMath.Sqrt(discriminant);
                t = projection - discriminant;
            }
            return t < c2.Length;
        }
        //判断圆与
        public static bool Overlap(in ShapeSphere c1, in ShapeCylinder c2)
        {
            TFloat radius = c1.Radius;
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
            TFloat sqrRadius = radius + c2.Radius;
            sqrRadius *= sqrRadius;
            return sqrRadius > TVector2.SqrMagnitude(c1.Position.ToV2() - c2.Position.ToV2());
        }
        
        public static bool Overlap(in ShapeSphere c1, in ShapeBox c2)
        {
            TFloat radius = c1.Radius;
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
            TFloat sdf = ShapeSDFUtil.OrientedBoxSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Extern.ToV2());
            return sdf <= radius;
        }
        public static bool Overlap(in ShapeSphere c1, in ShapePie c2)
        {
            TFloat radius = c1.Radius;
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
            TFloat sdf = ShapeSDFUtil.SectorSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Angle, c2.Radius);
            return sdf <= radius;
        }

        public static bool Overlap(in ShapeBox c1, in ShapeSphere c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapePie c1, in ShapeSphere c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapeRay c1, in ShapeSphere c2, out TFloat t) { return Overlap(c2, c1, out t); }
        #endregion

        #region 圆柱
        public static bool Overlap(in ShapeCylinder c1, in ShapeSphere c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapeCylinder c1, in ShapeCylinder c2)
        {
            if (c1.Position.y + c1.Height < c2.Position.y)
                return false;
            if (c1.Position.y > (c2.Position.y + c2.Height))
                return false;

            TFloat sqrRadius = c1.Radius + c2.Radius;
            sqrRadius *= sqrRadius;
            return sqrRadius >= TVector2.SqrMagnitude(c1.Position.ToV2() - c2.Position.ToV2());
        }
        //圆柱跟矩形，计算出点到一个矩形的距离，如果这个距离小于圆柱的半径，就证明是会发生碰撞的
        public static bool Overlap(in ShapeCylinder c1, in ShapeBox c2)
        {
            if (c1.Position.y + c1.Height < c2.Position.y - c2.Extern.y)
                return false;
            if (c1.Position.y > c2.Position.y + c2.Extern.y)
                return false;

            TFloat sdf = ShapeSDFUtil.OrientedBoxSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Extern.ToV2());
            return sdf <= c1.Radius;
        }
        public static bool Overlap(in ShapeCylinder c1, in ShapePie c2)
        {
            if (c1.Position.y + c1.Height < c2.Position.y)
                return false;
            if (c1.Position.y > c2.Position.y + c2.Height)
                return false;
            TFloat sdf = ShapeSDFUtil.SectorSDF(c1.Position.ToV2(), c2.Position.ToV2(), c2.YDegree, c2.Angle, c2.Radius);
            return sdf <= c1.Radius;
        }
        public static bool Overlap(in ShapeCylinder c1, in ShapeRay c2, out TFloat t)
        {
            t = TFloat.Zero;
            TFloat top = c1.Position.y + c1.Height;
            TFloat bottom = c1.Position.y;
            TVector3 end = c2.Position + c2.Direction * c2.Length;
            if (c2.Position.y < bottom && end.y < bottom)
                return false;
            if (c2.Position.y > top && end.y > top)
                return false;
            TFloat radiusSquare = c1.Radius * c1.Radius;
            TVector2 center = c1.Position.ToV2();
            TVector2 lineStart = c2.Position.ToV2();
            TVector2 lineEnd = end.ToV2();
            //如果在平面上圆和线段不相交则返回
            if (SegmentSDFSqr(center, lineStart, lineEnd) > radiusSquare)
                return false;
            TVector2 startToCenter = lineStart - center;
            TVector2 endToCenter = lineEnd - center;
            bool isStartInCircle = startToCenter.sqrMagnitude < radiusSquare;
            bool isEndInCircle = endToCenter.sqrMagnitude < radiusSquare;
            t = TFloat.MaxValue;
            {//处理和顶部或者底部相交的情况
                if (new TPlane(TVector3.up, -top).Raycast(new TRay(c2.Position, c2.Direction), out TFloat enterTop)
                    && enterTop < c2.Length && enterTop > 0)
                {
                    TVector2 pt = (c2.Position + c2.Direction * enterTop).ToV2();
                    if ((pt - center).sqrMagnitude <= radiusSquare)
                    {//交点在圆柱内
                        t = TMath.Min(t, enterTop);
                    }
                }
                if (new TPlane(TVector3.up, -bottom).Raycast(new TRay(c2.Position, c2.Direction), out TFloat enterBottom)
                    && enterBottom < c2.Length && enterBottom > 0)
                {
                    TVector2 pt = (c2.Position + c2.Direction * enterBottom).ToV2();
                    if ((pt - center).sqrMagnitude <= radiusSquare)
                    {//交点在圆柱内
                        t = TMath.Min(t, enterBottom);
                    }
                }
            }

            do
            {
                if (isStartInCircle && isEndInCircle)
                {//两个点都在圆内，要么和顶部或者底部相交，要么在圆柱内部，内部这里也算相交
                    t = TMath.Min(t, c2.Length);
                    break;
                }

                TVector2 lineDir = lineEnd - lineStart;
                TFloat sqrMagnitude = lineDir.sqrMagnitude;
                if (sqrMagnitude < TFloat.EN5)
                    break;//射线垂直方向，只与顶部或者底部相交
                lineDir /= TMath.Sqrt(sqrMagnitude);

                TFloat b = TVector2.Dot(startToCenter, lineDir);// b大于0，说明射线方向背向圆心
                TFloat c = TVector2.Dot(startToCenter, startToCenter) - radiusSquare;
                // 如果射线起点在圆外，并且方向与到圆方向相反，则不相交,前面处理过，正常这里不会判断失败
                if (c > TFloat.Zero && b > TFloat.Zero)
                    return false;
                TFloat discr = b * b - c;
                // 判别式小于0。前面处理过，正常这里不会判断失败
                if (discr < TFloat.Zero)
                    return false;
                TFloat enter = -b - TMath.Sqrt(discr); // -b-sqrt(b*b-c)表示从射线起点比较近的相交点
                if (enter < TFloat.Zero)                     // 射线的长度t值应该为正方向，最小为0
                    enter = TFloat.Zero;
                TVector3 hDir = c2.Direction;//射线水平方向朝向
                hDir.y = TFloat.Zero;
                hDir.Normalize();
                TFloat dot = TVector3.Dot(c2.Direction, hDir);
                enter /= dot;
                if (enter < c2.Length)
                {
                    TVector3 p = c2.Position + c2.Direction *enter;
                    if (p.y >= bottom && p.y <= top)
                    {
                        t = TMath.Min(t, enter);
                    }
                }

            } while (false);

            return t <= c2.Length;
        }

        public static bool Overlap(in ShapeBox c1, in ShapeCylinder c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapePie c1, in ShapeCylinder c2) { return Overlap(c2, c1); }
        public static bool Overlap(in ShapeRay c1, in ShapeCylinder c2, out TFloat t) { return Overlap(c2, c1, out t); }
        #endregion

    }
}