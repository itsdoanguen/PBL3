namespace PBL3.Service.History
{
    public interface IHistoryService
    {
        /// <summary>
        /// Cập nhật hoặc thêm mới lịch sử đọc của user cho một chapter trong story.
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="storyId">ID truyện</param>
        /// <param name="chapterId">ID chương</param>
        /// <returns></returns>
        Task UpdateHistoryAsync(int userId, int storyId, int chapterId);

        /// <summary>
        /// Lấy danh sách lịch sử đọc của user, sắp xếp theo thời gian cập nhật mới nhất.
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Danh sách HistoryItemViewModel</returns>
        Task<List<PBL3.ViewModels.History.HistoryItemViewModel>> GetUserHistoryAsync(int userId);

        /// <summary>
        /// Xóa lịch sử đọc của user cho một truyện hoặc toàn bộ.
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="storyId">ID truyện (nếu null sẽ xóa toàn bộ)</param>
        /// <returns></returns>
        Task DeleteHistoryAsync(int userId, int? storyId = null);
    }
}
