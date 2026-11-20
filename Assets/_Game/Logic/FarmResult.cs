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
        NotEnoughBait,
        BasketFull,
        BasketMaxed,
        AlreadyOwned,
        FishingNotFinished,
        FishingAlreadyApplied,
        // Thả cá: vị trí trong giỏ không có.
        UnknownFish,
        // Hộp thư (Đợt 2 Phần 3).
        UnknownMail,
        MailAlreadyClaimed,
        MailExpired,
        // Không có quà để nhận (thư không kèm quà / Nhận tất cả khi không còn thư nào).
        NoMailToClaim,
    }
}
