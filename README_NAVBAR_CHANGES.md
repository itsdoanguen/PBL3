# Navbar Unification Changes

## Overview
This document outlines the changes made to unify the navigation bar (navbar) across all views in the application, ensuring consistent behavior and appearance for all user roles.

## Changes Made

### 1. Main Layout Update (`Views/Shared/_Layout.cshtml`)
- **Enhanced navbar structure**: Updated the navigation bar to support role-based functionality
- **Management dropdown**: Added a dedicated "Quản lí" dropdown for Admin and Moderator users
  - Moderators see: "Quản lí nội dung"
  - Admins see: Both "Quản lí nội dung" and "Quản lí hệ thống"
- **Improved user menu**: Enhanced the user dropdown with all necessary user functions
- **Consistent styling**: Applied unified styling across all navigation elements

### 2. Layout Standardization
- **Default layout**: Updated `Views/_ViewStart.cshtml` to use `_Layout.cshtml` as the default
- **View cleanup**: Removed explicit layout declarations from individual views where they used `_UserLayout.cshtml`
- **Maintained special layouts**: Kept specialized layouts for specific functions:
  - `_AdminLayout.cshtml` for admin dashboard pages
  - `_StoryEditorLayout.cshtml` for story editing
  - `_ChapterEditorLayout.cshtml` for chapter editing

### 3. CSS Enhancements (`wwwroot/css/site.css`)
- **Navbar styling**: Added comprehensive styling for the unified navbar
- **Role-based styling**: Special styling for management dropdown and user avatars
- **Responsive design**: Enhanced mobile responsiveness for the navbar
- **Hover effects**: Added smooth transitions and visual feedback

## Navigation Structure by User Role

### 1. Unauthenticated Users
- **Navigation links**: All redirect to login page
- **Search**: Disabled with "Đăng nhập để tìm kiếm..." placeholder
- **Action buttons**: "Đăng Nhập" and "Đăng Ký" buttons

### 2. Regular Users
- **Navigation**: Access to "Khám phá" and "Bảng Xếp Hạng"
- **Search**: Functional search form
- **User menu**: Profile, Stories, History, Library, Notifications, Logout

### 3. Moderators
- **Additional access**: "Quản lí" dropdown with "Quản lí nội dung"
- **Same user features**: All regular user functionality
- **Management functions**: Access to content moderation tools

### 4. Admins
- **Full access**: Both "Quản lí nội dung" and "Quản lí hệ thống" in dropdown
- **Complete functionality**: All user and moderator features
- **System management**: Access to admin dashboard and system controls

## Files Modified

### Layout Files
- `Views/Shared/_Layout.cshtml` - Main layout with unified navbar
- `Views/_ViewStart.cshtml` - Default layout specification

### View Files Updated (removed explicit layout declarations)
- Authentication views: `Login.cshtml`, `Register.cshtml`
- User views: `Index.cshtml`, `MyProfile.cshtml`, `MyStories.cshtml`, etc.
- Moderator views: `Index.cshtml`, `ViewUser.cshtml`, `ViewStory.cshtml`, etc.
- Other views: `Home/Index.cshtml`, `Notification/Index.cshtml`, etc.

### Styling
- `wwwroot/css/site.css` - Enhanced navbar styling and responsive design

## Benefits

1. **Consistency**: All views now use the same navigation structure
2. **Role-based functionality**: Appropriate navigation options for each user role
3. **Maintainability**: Single source of truth for navigation layout
4. **User experience**: Intuitive and consistent navigation across the application
5. **Responsive design**: Works well on all device sizes

## Future Considerations

- The specialized layouts (`_AdminLayout.cshtml`, `_StoryEditorLayout.cshtml`, etc.) are maintained for their specific purposes
- New views should use the default layout unless they require specialized functionality
- Any navbar changes should be made in the main `_Layout.cshtml` file to ensure consistency 