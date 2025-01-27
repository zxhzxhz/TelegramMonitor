namespace TelegramMonitor;

/// <summary>
/// 字符串扩展工具类,提供电话号码格式验证和处理功能
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 电话号码格式正则：+开头,6-15位数字
    /// </summary>
    private static readonly Regex PhoneNumberRegex = new(@"^\+\d{6,15}$", RegexOptions.Compiled);

    /// <summary>
    /// 验证电话号码格式是否有效
    /// </summary>
    public static bool IsPhoneNumberValid(string? phoneNumber) =>
        !string.IsNullOrEmpty(phoneNumber) && PhoneNumberRegex.IsMatch(phoneNumber);

    /// <summary>
    /// 提示用户输入电话号码,并验证格式
    /// </summary>
    public static string PromptForPhoneNumber()
    {
        string phoneNumber;
        do
        {
            LogExtensions.Prompts("请输入telegram绑定的电话号码: ");
            phoneNumber = (Console.ReadLine() ?? string.Empty).Replace(" ", "");

            if (!IsPhoneNumberValid(phoneNumber))
            {
                LogExtensions.Error("电话号码格式无效，请重新输入。正确格式示例：+8613812345678");
            }
        } while (!IsPhoneNumberValid(phoneNumber));

        return phoneNumber;
    }
}