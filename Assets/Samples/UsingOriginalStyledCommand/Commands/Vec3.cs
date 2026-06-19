namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 简单的三维数据：两个 int 坐标和一个 bool 标志。
    /// </summary>
    public class Vec3
    {
        public int X;
        public int Y;
        public bool Flag;

        public Vec3(int x, int y, bool flag)
        {
            X = x;
            Y = y;
            Flag = flag;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Flag})";
        }
    }
}
