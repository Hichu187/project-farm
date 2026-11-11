namespace VeQue.View.Map
{
    // Thứ tự vẽ theo trục Y dùng chung cho cây và nhân vật: ô càng thấp (y nhỏ) càng vẽ sau, nằm trước.
    public static class SortingOrders
    {
        public const int ByYBase = 100;

        public static int ByCellY(int cellY) => ByYBase - cellY;
    }
}
