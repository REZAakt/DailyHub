using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Otd
{
    public record OnThisDayItemDto(
        string Type,          // events | births | deaths | holidays | selected
        int Year,
        string Text,          // متن خلاصه‌ی خود آیتم (رویداد/بیوگرافی/تعطیلات)
        string? Title,        // عنوان صفحه مرتبط (اگر باشد)
        string? PageUrl,      // لینک ویکی‌پدیا (desktop)
        string? Thumbnail,    // تصویر بندانگشتی
        string Language,      // زبان درخواست (fa/en/...)
        int Month,            // MM
        int Day               // DD
    );
}
