namespace OPH.Collision.QuadTree {
    /// <summary>
    /// 创建人：一个人心
    /// 功能说明：范围接口
    /// </summary>
    public interface IRect {
        /// <summary>
        /// 矩形中心X
        /// </summary>
        float X { get; set; }
        /// <summary>
        /// 矩形中心Y
        /// </summary>
        float Y { get; set; }
        /// <summary>
        /// 矩形宽
        /// </summary>
        float Width { get; set; }
        /// <summary>
        /// 矩形高
        /// </summary>
        float Height { get; set; }
    }
}
