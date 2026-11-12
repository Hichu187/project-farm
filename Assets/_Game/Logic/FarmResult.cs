namespace VeQue.Logic
{
    public enum FarmResult
    {
        Ok = 0,
        UnknownZone,
        UnknownPlot,
        UnknownCrop,
        InvalidAmount,
        NotEnoughCoins,
        NotEnoughSeeds,
        NotEnoughItems,
        PlotLocked,
        PlotNotEmpty,
        NothingToHarvest,
        NotRipe,
        ZoneLocked,
        ZoneFull,
        NothingToWater,
        AlreadyWatered,
        AlreadyRipe,
        CropLocked,
        LevelTooLow,
        OrdersLocked,
        UnknownOrder,
        OrderNotReady,
        // Đơn khẩn cấp đã hết hạn (ô đã trống).
        OrderExpired,
        // Ô đã có việc trong hàng việc của nhân vật.
        AlreadyQueued,
        // Tên nhân vật trống hoặc quá dài.
        InvalidName,
        // Câu cá (Phần 10).
        FishingLocked,
        UnknownRod,
        UnknownBait,
        NoRod,
        RodBroken,
        RodNotDamaged,
        // Đã sửa quá nhiều (độ bền tối đa dưới mức tối thiểu): phải mua lại cần.
        RodWornOut,
        NotEnoughBait,
        BasketFull,
        BasketMaxed,
        AlreadyOwned,
        FishingNotFinished,
        FishingAlreadyApplied,
    }
}
