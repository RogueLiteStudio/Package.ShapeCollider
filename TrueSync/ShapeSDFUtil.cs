using TrueSync;
namespace TShapeCollider
{
    public static class ShapeSDFUtil
    {
        public static TVector2 Rotate(TVector2 v, TFloat degree)
        {
            TFloat radians = degree * TMath.Deg2Rad;
            var ca = TMath.Cos(radians);
            var sa = TMath.Sin(radians);
            return new TVector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }

        public static TVector2 Abs(this TVector2 vector)
        {
            return new TVector2(TMath.Abs(vector.x), TMath.Abs(vector.y));
        }
        /// <summary>
        /// 计算BoxSDF
        /// </summary>
        /// <param name="point">点位置</param>
        /// <param name="center">box中心点</param>
        /// <param name="yDegree">相对Y轴的旋转角度</param>
        /// <param name="halfSize"></param>
        /// <returns></returns>
        public static TFloat OrientedBoxSDF(TVector2 point, TVector2 center, TFloat yDegree, TVector2 halfSize)
        {
            //将point转换到目标坐标系
            point -= center;
            point = Rotate(point, -yDegree);
            TVector2 d = point.Abs() - halfSize;
            return TVector2.Max(d, TVector2.zero).magnitude + TMath.Min(TMath.Max(d.x, d.y), TFloat.Zero);
        }
        /// <summary>
        /// 计算扇形的SDF
        /// </summary>
        /// <param name="point">点位置</param>
        /// <param name="pos">扇形原点位置</param>
        /// <param name="yDegree">相对Y轴旋转角度</param>
        /// <param name="angle">扇形夹角</param>
        /// <param name="radius">扇形半径</param>
        /// <returns>点到扇形最近边缘的距离，小于0在扇形内部，大于0在扇形外部</returns>
        public static TFloat SectorSDF(TVector2 point, TVector2 pos, TFloat yDegree, TFloat angle, TFloat radius)
        {
            //将point转换到目标坐标系
            point -= pos;
            point = Rotate(point, yDegree);
            //计算sdf https://zhuanlan.zhihu.com/p/427587359
            //原点在圆心图形以y轴为对称轴
            TFloat d = (angle /2) * TMath.Deg2Rad;
            TVector2 c = new TVector2(TMath.Sin(d), TMath.Cos(d));
            point.x = TMath.Abs(point.x);
            //点到外围圆弧的距离
            TFloat qp1 = point.magnitude - radius;
            //点到直线
            TFloat qp2 = (point - c * TMath.Clamp(TVector2.Dot(point, c), TFloat.Zero, radius)).magnitude * TMath.Sign(c.y * point.x - c.x * point.y);
            return TMath.Max(qp1, qp2);
        }
    }

}