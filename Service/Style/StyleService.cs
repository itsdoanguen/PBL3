using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Chapter;

namespace PBL3.Service.Style
{
    public class StyleService : IStyleService
    {
        private readonly ApplicationDbContext _context;
        public StyleService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<StyleModel> InitStyleForUserAsync(int userId)
        {
            var style = new StyleModel
            {
                UserID = userId,
                FontFamily = FontFamily.Arial,
                FontSize = 16,
                BackgroundColor = BackgroundColor.White,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Styles.AddAsync(style);
            await _context.SaveChangesAsync();
            return style;
        }

        public async Task<StyleViewModel> GetStyleByUserIdAsync(int userId)
        {
            var style = _context.Styles.FirstOrDefault(s => s.UserID == userId);
            if (style == null)
            {
                style = await InitStyleForUserAsync(userId);
            }

            var styleViewModel = new StyleViewModel
            {
                StyleID = style.StyleID,
                UserID = style.UserID,
                FontFamily = style.FontFamily,
                FontSize = style.FontSize,
                BackgroundColor = style.BackgroundColor
            };

            return styleViewModel;
        }
        public async Task<StyleModel> UpdateStyleAsync(StyleViewModel styleViewModel)
        {
            var style = await _context.Styles.FindAsync(styleViewModel.StyleID);
            if (style == null)
            {
                style = await InitStyleForUserAsync(styleViewModel.UserID);
            }

            style.FontFamily = styleViewModel.FontFamily;
            style.FontSize = styleViewModel.FontSize;
            style.BackgroundColor = styleViewModel.BackgroundColor;
            style.UpdatedAt = DateTime.Now;

            _context.Styles.Update(style);
            await _context.SaveChangesAsync();
            return style;
        }
    }
}
