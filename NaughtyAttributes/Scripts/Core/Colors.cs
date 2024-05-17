using System.Collections.Generic;
using UnityEngine;

namespace Unity.Attributes.NaughtyAttributes.Scripts.Core
{
    public enum ColorEnum
    {
        Maroon,
        RedDark,
        Brown,
        Firebrick,
        Crimson,
        Red,
        Tomato,
        Coral,
        RedIndian,
        CoralLight,
        SalmonDark,
        Salmon,
        SalmonLight,
        RedOrange,
        OrangeDark,
        Orange,
        Gold,
        GoldenRodDark,
        RodGolden,
        GoldenRodPale,
        KhakiDark,
        Khaki,
        Olive,
        Yellow,
        GreenYellow,
        OliveGreenDark,
        OliveDrab,
        GreenLawn,
        ReuseChart,
        YellowGreen,
        GreenDark,
        Green,
        GreenForest,
        Lime,
        GreenLime,
        GreenLight,
        GreenPale,
        GreenSeaDark,
        GreenSpringMedium,
        GreenSpring,
        GreenSea,
        MarineAquaMedium,
        GreenSeaMedium,
        GreenSeaLight,
        GreySlateDark,
        Teal,
        CyanDark,
        Aqua,
        Cyan,
        CyanLight,
        TurquoiseDark,
        Turquoise,
        TurquoiseMedium,
        TurquoisePale,
        MarineAqua,
        BluePowder,
        BlueCadet,
        BlueSteel,
        BlueCornFlower,
        BlueDeepSky,
        BlueDodger,
        BlueLight,
        BlueSky,
        BlueSkyLight,
        BlueMidnight,
        Navy,
        BlueDark,
        BlueMedium,
        Blue,
        BlueRoyal,
        VioletBlue,
        Indigo,
        BlueSlateDark,
        BlueSlate,
        BlueSlateMedium,
        PurpleMedium,
        MagentaDark,
        VioletDark,
        OrchidDark,
        OrchidMedium,
        Purple,
        Thistle,
        Plum,
        Violet,
        Magenta,
        Orchid,
        VioletRedMedium,
        VioletRedPale,
        PinkDeep,
        PinkHot,
        PinkLight,
        Pink,
        WhiteAntique,
        Beige,
        Bisque,
        AlmondBlanched,
        Wheat,
        SilkCorn,
        ChiffonLemon,
        GoldenRodYellowLight,
        YellowLight,
        BrownSaddle,
        Sienna,
        Chocolate,
        Peru,
        BrownSandy,
        WoodBurly,
        Tan,
        BrownRosy,
        Moccasin,
        WhiteNavajo,
        PeachPuff,
        MistyRose,
        LavenderBlush,
        Linen,
        OldLace,
        PapayaWhip,
        SeaShell,
        MintCream,
        SlateGrey,
        SlateGreyLight,
        SteelBlueLight,
        Lavender,
        WhiteFloral,
        AliceBlue,
        WhiteGhost,
        Honeydew,
        Ivory,
        Azure,
        Snow,
        Black,
        GreyDim,
        Grey,
        GreyDark,
        Silver,
        GreyLight,
        Gainsboro,
        WhiteSmoke,
        White,
        BlackAlmost
    }

    public static class Colors
    {
        public static Color Maroon { get; } = new Color32(128, 0, 0, 255);
        public static Color RedDark { get; } = new Color32(139, 0, 0, 255);
        public static Color Brown { get; } = new Color32(165, 42, 42, 255);
        public static Color Firebrick { get; } = new Color32(178, 34, 34, 255);
        public static Color Crimson { get; } = new Color32(220, 20, 60, 255);
        public static Color Red { get; } = new Color32(255, 0, 0, 255);
        public static Color Tomato { get; } = new Color32(255, 99, 71, 255);
        public static Color Coral { get; } = new Color32(255, 127, 80, 255);
        public static Color RedIndian { get; } = new Color32(205, 92, 92, 255);
        public static Color CoralLight { get; } = new Color32(240, 128, 128, 255);
        public static Color SalmonDark { get; } = new Color32(233, 150, 122, 255);
        public static Color Salmon { get; } = new Color32(250, 128, 114, 255);
        public static Color SalmonLight { get; } = new Color32(255, 160, 122, 255);
        public static Color RedOrange { get; } = new Color32(255, 69, 0, 255);
        public static Color OrangeDark { get; } = new Color32(255, 140, 0, 255);
        public static Color Orange { get; } = new Color32(255, 165, 0, 255);
        public static Color Gold { get; } = new Color32(255, 215, 0, 255);
        public static Color GoldenRodDark { get; } = new Color32(184, 134, 11, 255);
        public static Color RodGolden { get; } = new Color32(218, 165, 32, 255);
        public static Color GoldenRodPale { get; } = new Color32(238, 232, 170, 255);
        public static Color KhakiDark { get; } = new Color32(189, 183, 107, 255);
        public static Color Khaki { get; } = new Color32(240, 230, 140, 255);
        public static Color Olive { get; } = new Color32(128, 128, 0, 255);
        public static Color Yellow { get; } = new Color32(255, 255, 0, 255);
        public static Color GreenYellow { get; } = new Color32(154, 205, 50, 255);
        public static Color OliveGreenDark { get; } = new Color32(85, 107, 47, 255);
        public static Color OliveDrab { get; } = new Color32(107, 142, 35, 255);
        public static Color GreenLawn { get; } = new Color32(124, 252, 0, 255);
        public static Color ReuseChart { get; } = new Color32(127, 255, 0, 255);
        public static Color YellowGreen { get; } = new Color32(173, 255, 47, 255);
        public static Color GreenDark { get; } = new Color32(0, 100, 0, 255);
        public static Color Green { get; } = new Color32(0, 128, 0, 255);
        public static Color GreenForest { get; } = new Color32(34, 139, 34, 255);
        public static Color Lime { get; } = new Color32(0, 255, 0, 255);
        public static Color GreenLime { get; } = new Color32(50, 205, 50, 255);
        public static Color GreenLight { get; } = new Color32(144, 238, 144, 255);
        public static Color GreenPale { get; } = new Color32(152, 251, 152, 255);
        public static Color GreenSeaDark { get; } = new Color32(143, 188, 143, 255);
        public static Color GreenSpringMedium { get; } = new Color32(0, 250, 154, 255);
        public static Color GreenSpring { get; } = new Color32(0, 255, 127, 255);
        public static Color GreenSea { get; } = new Color32(46, 139, 87, 255);
        public static Color MarineAquaMedium { get; } = new Color32(102, 205, 170, 255);
        public static Color GreenSeaMedium { get; } = new Color32(60, 179, 113, 255);
        public static Color GreenSeaLight { get; } = new Color32(32, 178, 170, 255);
        public static Color GreySlateDark { get; } = new Color32(47, 79, 79, 255);
        public static Color Teal { get; } = new Color32(0, 128, 128, 255);
        public static Color CyanDark { get; } = new Color32(0, 139, 139, 255);
        public static Color Aqua { get; } = new Color32(0, 255, 255, 255);
        public static Color Cyan { get; } = new Color32(0, 255, 255, 255);
        public static Color CyanLight { get; } = new Color32(224, 255, 255, 255);
        public static Color TurquoiseDark { get; } = new Color32(0, 206, 209, 255);
        public static Color Turquoise { get; } = new Color32(64, 224, 208, 255);
        public static Color TurquoiseMedium { get; } = new Color32(72, 209, 204, 255);
        public static Color TurquoisePale { get; } = new Color32(175, 238, 238, 255);
        public static Color MarineAqua { get; } = new Color32(127, 255, 212, 255);
        public static Color BluePowder { get; } = new Color32(176, 224, 230, 255);
        public static Color BlueCadet { get; } = new Color32(95, 158, 160, 255);
        public static Color BlueSteel { get; } = new Color32(70, 130, 180, 255);
        public static Color BlueCornFlower { get; } = new Color32(100, 149, 237, 255);
        public static Color BlueDeepSky { get; } = new Color32(0, 191, 255, 255);
        public static Color BlueDodger { get; } = new Color32(30, 144, 255, 255);
        public static Color BlueLight { get; } = new Color32(173, 216, 230, 255);
        public static Color BlueSky { get; } = new Color32(135, 206, 235, 255);
        public static Color BlueSkyLight { get; } = new Color32(135, 206, 250, 255);
        public static Color BlueMidnight { get; } = new Color32(25, 25, 112, 255);
        public static Color Navy { get; } = new Color32(0, 0, 128, 255);
        public static Color BlueDark { get; } = new Color32(0, 0, 139, 255);
        public static Color BlueMedium { get; } = new Color32(0, 0, 205, 255);
        public static Color Blue { get; } = new Color32(0, 0, 255, 255);
        public static Color BlueRoyal { get; } = new Color32(65, 105, 225, 255);
        public static Color VioletBlue { get; } = new Color32(138, 43, 226, 255);
        public static Color Indigo { get; } = new Color32(75, 0, 130, 255);
        public static Color BlueSlateDark { get; } = new Color32(72, 61, 139, 255);
        public static Color BlueSlate { get; } = new Color32(106, 90, 205, 255);
        public static Color BlueSlateMedium { get; } = new Color32(123, 104, 238, 255);
        public static Color PurpleMedium { get; } = new Color32(147, 112, 219, 255);
        public static Color MagentaDark { get; } = new Color32(139, 0, 139, 255);
        public static Color VioletDark { get; } = new Color32(148, 0, 211, 255);
        public static Color OrchidDark { get; } = new Color32(153, 50, 204, 255);
        public static Color OrchidMedium { get; } = new Color32(186, 85, 211, 255);
        public static Color Purple { get; } = new Color32(128, 0, 128, 255);
        public static Color Thistle { get; } = new Color32(216, 191, 216, 255);
        public static Color Plum { get; } = new Color32(221, 160, 221, 255);
        public static Color Violet { get; } = new Color32(238, 130, 238, 255);
        public static Color Magenta { get; } = new Color32(255, 0, 255, 255);
        public static Color Orchid { get; } = new Color32(218, 112, 214, 255);
        public static Color VioletRedMedium { get; } = new Color32(199, 21, 133, 255);
        public static Color VioletRedPale { get; } = new Color32(219, 112, 147, 255);
        public static Color PinkDeep { get; } = new Color32(255, 20, 147, 255);
        public static Color PinkHot { get; } = new Color32(255, 105, 180, 255);
        public static Color PinkLight { get; } = new Color32(255, 182, 193, 255);
        public static Color Pink { get; } = new Color32(255, 192, 203, 255);
        public static Color WhiteAntique { get; } = new Color32(250, 235, 215, 255);
        public static Color Beige { get; } = new Color32(245, 245, 220, 255);
        public static Color Bisque { get; } = new Color32(255, 228, 196, 255);
        public static Color AlmondBlanched { get; } = new Color32(255, 235, 205, 255);
        public static Color Wheat { get; } = new Color32(245, 222, 179, 255);
        public static Color SilkCorn { get; } = new Color32(255, 248, 220, 255);
        public static Color ChiffonLemon { get; } = new Color32(255, 250, 205, 255);
        public static Color GoldenRodYellowLight { get; } = new Color32(250, 250, 210, 255);
        public static Color YellowLight { get; } = new Color32(255, 255, 224, 255);
        public static Color BrownSaddle { get; } = new Color32(139, 69, 19, 255);
        public static Color Sienna { get; } = new Color32(160, 82, 45, 255);
        public static Color Chocolate { get; } = new Color32(210, 105, 30, 255);
        public static Color Peru { get; } = new Color32(205, 133, 63, 255);
        public static Color BrownSandy { get; } = new Color32(244, 164, 96, 255);
        public static Color WoodBurly { get; } = new Color32(222, 184, 135, 255);
        public static Color Tan { get; } = new Color32(210, 180, 140, 255);
        public static Color BrownRosy { get; } = new Color32(188, 143, 143, 255);
        public static Color Moccasin { get; } = new Color32(255, 228, 181, 255);
        public static Color WhiteNavajo { get; } = new Color32(255, 222, 173, 255);
        public static Color PeachPuff { get; } = new Color32(255, 218, 185, 255);
        public static Color MistyRose { get; } = new Color32(255, 228, 225, 255);
        public static Color LavenderBlush { get; } = new Color32(255, 240, 245, 255);
        public static Color Linen { get; } = new Color32(250, 240, 230, 255);
        public static Color OldLace { get; } = new Color32(253, 245, 230, 255);
        public static Color PapayaWhip { get; } = new Color32(255, 239, 213, 255);
        public static Color SeaShell { get; } = new Color32(255, 245, 238, 255);
        public static Color MintCream { get; } = new Color32(245, 255, 250, 255);
        public static Color SlateGrey { get; } = new Color32(112, 128, 144, 255);
        public static Color SlateGreyLight { get; } = new Color32(119, 136, 153, 255);
        public static Color SteelBlueLight { get; } = new Color32(176, 196, 222, 255);
        public static Color Lavender { get; } = new Color32(230, 230, 250, 255);
        public static Color WhiteFloral { get; } = new Color32(255, 250, 240, 255);
        public static Color AliceBlue { get; } = new Color32(240, 248, 255, 255);
        public static Color WhiteGhost { get; } = new Color32(248, 248, 255, 255);
        public static Color Honeydew { get; } = new Color32(240, 255, 240, 255);
        public static Color Ivory { get; } = new Color32(255, 255, 240, 255);
        public static Color Azure { get; } = new Color32(240, 255, 255, 255);
        public static Color Snow { get; } = new Color32(255, 250, 250, 255);
        public static Color Black { get; } = new Color32(0, 0, 0, 255);
        public static Color GreyDim { get; } = new Color32(105, 105, 105, 255);
        public static Color Grey { get; } = new Color32(128, 128, 128, 255);
        public static Color GreyDark { get; } = new Color32(169, 169, 169, 255);
        public static Color Silver { get; } = new Color32(192, 192, 192, 255);
        public static Color GreyLight { get; } = new Color32(211, 211, 211, 255);
        public static Color Gainsboro { get; } = new Color32(220, 220, 220, 255);
        public static Color WhiteSmoke { get; } = new Color32(245, 245, 245, 255);
        public static Color White { get; } = new Color32(255, 255, 255, 255);
        public static Color BlackAlmost { get; } = new Color32(29, 29, 29, 255);

        public static readonly IReadOnlyDictionary<ColorEnum, Color> ColorDict = new Dictionary<ColorEnum, Color>
        {
            { ColorEnum.Maroon, new Color32(128, 0, 0, 255) },
            { ColorEnum.RedDark, new Color32(139, 0, 0, 255) },
            { ColorEnum.Brown, new Color32(165, 42, 42, 255) },
            { ColorEnum.Firebrick, new Color32(178, 34, 34, 255) },
            { ColorEnum.Crimson, new Color32(220, 20, 60, 255) },
            { ColorEnum.Red, new Color32(255, 0, 0, 255) },
            { ColorEnum.Tomato, new Color32(255, 99, 71, 255) },
            { ColorEnum.Coral, new Color32(255, 127, 80, 255) },
            { ColorEnum.RedIndian, new Color32(205, 92, 92, 255) },
            { ColorEnum.CoralLight, new Color32(240, 128, 128, 255) },
            { ColorEnum.SalmonDark, new Color32(233, 150, 122, 255) },
            { ColorEnum.Salmon, new Color32(250, 128, 114, 255) },
            { ColorEnum.SalmonLight, new Color32(255, 160, 122, 255) },
            { ColorEnum.RedOrange, new Color32(255, 69, 0, 255) },
            { ColorEnum.OrangeDark, new Color32(255, 140, 0, 255) },
            { ColorEnum.Orange, new Color32(255, 165, 0, 255) },
            { ColorEnum.Gold, new Color32(255, 215, 0, 255) },
            { ColorEnum.GoldenRodDark, new Color32(184, 134, 11, 255) },
            { ColorEnum.RodGolden, new Color32(218, 165, 32, 255) },
            { ColorEnum.GoldenRodPale, new Color32(238, 232, 170, 255) },
            { ColorEnum.KhakiDark, new Color32(189, 183, 107, 255) },
            { ColorEnum.Khaki, new Color32(240, 230, 140, 255) },
            { ColorEnum.Olive, new Color32(128, 128, 0, 255) },
            { ColorEnum.Yellow, new Color32(255, 255, 0, 255) },
            { ColorEnum.GreenYellow, new Color32(154, 205, 50, 255) },
            { ColorEnum.OliveGreenDark, new Color32(85, 107, 47, 255) },
            { ColorEnum.OliveDrab, new Color32(107, 142, 35, 255) },
            { ColorEnum.GreenLawn, new Color32(124, 252, 0, 255) },
            { ColorEnum.ReuseChart, new Color32(127, 255, 0, 255) },
            { ColorEnum.YellowGreen, new Color32(173, 255, 47, 255) },
            { ColorEnum.GreenDark, new Color32(0, 100, 0, 255) },
            { ColorEnum.Green, new Color32(0, 128, 0, 255) },
            { ColorEnum.GreenForest, new Color32(34, 139, 34, 255) },
            { ColorEnum.Lime, new Color32(0, 255, 0, 255) },
            { ColorEnum.GreenLime, new Color32(50, 205, 50, 255) },
            { ColorEnum.GreenLight, new Color32(144, 238, 144, 255) },
            { ColorEnum.GreenPale, new Color32(152, 251, 152, 255) },
            { ColorEnum.GreenSeaDark, new Color32(143, 188, 143, 255) },
            { ColorEnum.GreenSpringMedium, new Color32(0, 250, 154, 255) },
            { ColorEnum.GreenSpring, new Color32(0, 255, 127, 255) },
            { ColorEnum.GreenSea, new Color32(46, 139, 87, 255) },
            { ColorEnum.MarineAquaMedium, new Color32(102, 205, 170, 255) },
            { ColorEnum.GreenSeaMedium, new Color32(60, 179, 113, 255) },
            { ColorEnum.GreenSeaLight, new Color32(32, 178, 170, 255) },
            { ColorEnum.GreySlateDark, new Color32(47, 79, 79, 255) },
            { ColorEnum.Teal, new Color32(0, 128, 128, 255) },
            { ColorEnum.CyanDark, new Color32(0, 139, 139, 255) },
            { ColorEnum.Aqua, new Color32(0, 255, 255, 255) },
            { ColorEnum.Cyan, new Color32(0, 255, 255, 255) },
            { ColorEnum.CyanLight, new Color32(224, 255, 255, 255) },
            { ColorEnum.TurquoiseDark, new Color32(0, 206, 209, 255) },
            { ColorEnum.Turquoise, new Color32(64, 224, 208, 255) },
            { ColorEnum.TurquoiseMedium, new Color32(72, 209, 204, 255) },
            { ColorEnum.TurquoisePale, new Color32(175, 238, 238, 255) },
            { ColorEnum.MarineAqua, new Color32(127, 255, 212, 255) },
            { ColorEnum.BluePowder, new Color32(176, 224, 230, 255) },
            { ColorEnum.BlueCadet, new Color32(95, 158, 160, 255) },
            { ColorEnum.BlueSteel, new Color32(70, 130, 180, 255) },
            { ColorEnum.BlueCornFlower, new Color32(100, 149, 237, 255) },
            { ColorEnum.BlueDeepSky, new Color32(0, 191, 255, 255) },
            { ColorEnum.BlueDodger, new Color32(30, 144, 255, 255) },
            { ColorEnum.BlueLight, new Color32(173, 216, 230, 255) },
            { ColorEnum.BlueSky, new Color32(135, 206, 235, 255) },
            { ColorEnum.BlueSkyLight, new Color32(135, 206, 250, 255) },
            { ColorEnum.BlueMidnight, new Color32(25, 25, 112, 255) },
            { ColorEnum.Navy, new Color32(0, 0, 128, 255) },
            { ColorEnum.BlueDark, new Color32(0, 0, 139, 255) },
            { ColorEnum.BlueMedium, new Color32(0, 0, 205, 255) },
            { ColorEnum.Blue, new Color32(0, 0, 255, 255) },
            { ColorEnum.BlueRoyal, new Color32(65, 105, 225, 255) },
            { ColorEnum.VioletBlue, new Color32(138, 43, 226, 255) },
            { ColorEnum.Indigo, new Color32(75, 0, 130, 255) },
            { ColorEnum.BlueSlateDark, new Color32(72, 61, 139, 255) },
            { ColorEnum.BlueSlate, new Color32(106, 90, 205, 255) },
            { ColorEnum.BlueSlateMedium, new Color32(123, 104, 238, 255) },
            { ColorEnum.PurpleMedium, new Color32(147, 112, 219, 255) },
            { ColorEnum.MagentaDark, new Color32(139, 0, 139, 255) },
            { ColorEnum.VioletDark, new Color32(148, 0, 211, 255) },
            { ColorEnum.OrchidDark, new Color32(153, 50, 204, 255) },
            { ColorEnum.OrchidMedium, new Color32(186, 85, 211, 255) },
            { ColorEnum.Purple, new Color32(128, 0, 128, 255) },
            { ColorEnum.Thistle, new Color32(216, 191, 216, 255) },
            { ColorEnum.Plum, new Color32(221, 160, 221, 255) },
            { ColorEnum.Violet, new Color32(238, 130, 238, 255) },
            { ColorEnum.Magenta, new Color32(255, 0, 255, 255) },
            { ColorEnum.Orchid, new Color32(218, 112, 214, 255) },
            { ColorEnum.VioletRedMedium, new Color32(199, 21, 133, 255) },
            { ColorEnum.VioletRedPale, new Color32(219, 112, 147, 255) },
            { ColorEnum.PinkDeep, new Color32(255, 20, 147, 255) },
            { ColorEnum.PinkHot, new Color32(255, 105, 180, 255) },
            { ColorEnum.PinkLight, new Color32(255, 182, 193, 255) },
            { ColorEnum.Pink, new Color32(255, 192, 203, 255) },
            { ColorEnum.WhiteAntique, new Color32(250, 235, 215, 255) },
            { ColorEnum.Beige, new Color32(245, 245, 220, 255) },
            { ColorEnum.Bisque, new Color32(255, 228, 196, 255) },
            { ColorEnum.AlmondBlanched, new Color32(255, 235, 205, 255) },
            { ColorEnum.Wheat, new Color32(245, 222, 179, 255) },
            { ColorEnum.SilkCorn, new Color32(255, 248, 220, 255) },
            { ColorEnum.ChiffonLemon, new Color32(255, 250, 205, 255) },
            { ColorEnum.GoldenRodYellowLight, new Color32(250, 250, 210, 255) },
            { ColorEnum.YellowLight, new Color32(255, 255, 224, 255) },
            { ColorEnum.BrownSaddle, new Color32(139, 69, 19, 255) },
            { ColorEnum.Sienna, new Color32(160, 82, 45, 255) },
            { ColorEnum.Chocolate, new Color32(210, 105, 30, 255) },
            { ColorEnum.Peru, new Color32(205, 133, 63, 255) },
            { ColorEnum.BrownSandy, new Color32(244, 164, 96, 255) },
            { ColorEnum.WoodBurly, new Color32(222, 184, 135, 255) },
            { ColorEnum.Tan, new Color32(210, 180, 140, 255) },
            { ColorEnum.BrownRosy, new Color32(188, 143, 143, 255) },
            { ColorEnum.Moccasin, new Color32(255, 228, 181, 255) },
            { ColorEnum.WhiteNavajo, new Color32(255, 222, 173, 255) },
            { ColorEnum.PeachPuff, new Color32(255, 218, 185, 255) },
            { ColorEnum.MistyRose, new Color32(255, 228, 225, 255) },
            { ColorEnum.LavenderBlush, new Color32(255, 240, 245, 255) },
            { ColorEnum.Linen, new Color32(250, 240, 230, 255) },
            { ColorEnum.OldLace, new Color32(253, 245, 230, 255) },
            { ColorEnum.PapayaWhip, new Color32(255, 239, 213, 255) },
            { ColorEnum.SeaShell, new Color32(255, 245, 238, 255) },
            { ColorEnum.MintCream, new Color32(245, 255, 250, 255) },
            { ColorEnum.SlateGrey, new Color32(112, 128, 144, 255) },
            { ColorEnum.SlateGreyLight, new Color32(119, 136, 153, 255) },
            { ColorEnum.SteelBlueLight, new Color32(176, 196, 222, 255) },
            { ColorEnum.Lavender, new Color32(230, 230, 250, 255) },
            { ColorEnum.WhiteFloral, new Color32(255, 250, 240, 255) },
            { ColorEnum.AliceBlue, new Color32(240, 248, 255, 255) },
            { ColorEnum.WhiteGhost, new Color32(248, 248, 255, 255) },
            { ColorEnum.Honeydew, new Color32(240, 255, 240, 255) },
            { ColorEnum.Ivory, new Color32(255, 255, 240, 255) },
            { ColorEnum.Azure, new Color32(240, 255, 255, 255) },
            { ColorEnum.Snow, new Color32(255, 250, 250, 255) },
            { ColorEnum.Black, new Color32(0, 0, 0, 255) },
            { ColorEnum.GreyDim, new Color32(105, 105, 105, 255) },
            { ColorEnum.Grey, new Color32(128, 128, 128, 255) },
            { ColorEnum.GreyDark, new Color32(169, 169, 169, 255) },
            { ColorEnum.Silver, new Color32(192, 192, 192, 255) },
            { ColorEnum.GreyLight, new Color32(211, 211, 211, 255) },
            { ColorEnum.Gainsboro, new Color32(220, 220, 220, 255) },
            { ColorEnum.WhiteSmoke, new Color32(245, 245, 245, 255) },
            { ColorEnum.White, new Color32(255, 255, 255, 255) },
            { ColorEnum.BlackAlmost, new Color32(29, 29, 29, 255) }
        };

        public static string ToRichTextColor(this string str, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
        }
        
        public static string ToRichTextBold(this string str)
        {
            return $"<b>{str}</b>";
        }

        public static Color Multiply(this Color color, float multiplayer)
        {
            return new Color(color.r * multiplayer, color.g * multiplayer, color.b * multiplayer, color.a);
        }

        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public static Color FromVector3(this Vector3 vector3)
        {
            return new Color(vector3.x, vector3.y, vector3.z, 1);
        }
    }
}