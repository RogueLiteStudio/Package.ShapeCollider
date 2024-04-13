using TrueSync;
namespace TShapeCollider
{
    [System.Serializable]
    public struct ShapeBox
    {
        public TVector3 Position;
        public TVector3 Extern;
        public TFloat YDegree;
    }

    //圆柱
    [System.Serializable]
    public struct ShapeCylinder
    {
        public TVector3 Position;
        public TFloat Radius;
        public TFloat Height;
    }

    //球形
    [System.Serializable]
    public struct ShapeSphere
    {
        public TVector3 Position;
        public TFloat Radius;
    }

    //饼形，带高度的3D扇形
    [System.Serializable]
    public struct ShapePie
    {
        public TVector3 Position;
        public TFloat YDegree;
        public TFloat Angle;//夹角
        public TFloat Radius;
        public TFloat Height;
    }

    [System.Serializable]
    public struct ShapeRay
    {
        public TVector3 Position;
        public TVector3 Direction;
        public TFloat Length;
    }

}