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
        // Ô đã có việc trong hàng việc của nhân vật.
        AlreadyQueued,
    }
}
