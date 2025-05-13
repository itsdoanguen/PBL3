using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class StyleModel
    {
        [Key]
        public int StyleID { get; set; }
        [ForeignKey("User")]
        public int UserID { get; set; }

        public FontFamily FontFamily { get; set; } = FontFamily.Arial;
        public int FontSize { get; set; } = 16; 

        public BackgroundColor BackgroundColor { get; set; } = BackgroundColor.White;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        public UserModel? User { get; set; }

        public string GetBackgroundColorHex()
        {
            return BackgroundColor switch
            {
                BackgroundColor.White => "#FDFDFD",
                BackgroundColor.Black => "#121212",
                BackgroundColor.Blue => "#DDEEFF",
                BackgroundColor.Yellow => "#FFF9DC",
                BackgroundColor.Pink => "#FFE4F1",
                BackgroundColor.Gray => "#ECECEC",
                _ => "#FFFFFF"
            };
        }

        public string GetTextColorHex()
        {
            return BackgroundColor switch
            {
                BackgroundColor.Black => "#EEEEEE",
                _ => "#1A1A1A"
            };
        }
    }

    public enum FontFamily
    {
        Arial,
        TimesNewRoman,
        Verdana,
        Georgia,
        Tahoma,
        CourierNew,
        ComicSansMS,
        Impact,
        Calibri,
        Helvetica
    }

    public enum BackgroundColor
    {
        White,
        Black,
        Blue,
        Yellow,
        Pink,
        Gray
    }
}
