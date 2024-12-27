using System.Text.RegularExpressions;

namespace TelegramMonitor;

/// <summary>
/// 提供通用工具方法的静态类，包含电话号码验证、日志记录、关键词处理等功能。
/// </summary>
public static class StringExtensions
{
    //验证电话号码格式的正则表达式
    private static readonly Regex PhoneNumberRegex = new(@"^\+\d{10,15}$", RegexOptions.Compiled);

    //验证电话号码是否符合E.164国际标准格式
    public static bool IsPhoneNumberValid(string? phoneNumber) =>
        !string.IsNullOrEmpty(phoneNumber) && PhoneNumberRegex.IsMatch(phoneNumber);

    //循环请求用户输入电话号码，直到格式合法
    public static string PromptForPhoneNumber()
    {
        string phoneNumber;
        do
        {
            LogExtensions.Prompts("请输入telegram绑定的电话号码: ");

            // 去除用户输入中空白字符，避免格式不统一（如在输入中不小心敲入空格）
            phoneNumber = (Console.ReadLine() ?? string.Empty).Replace(" ", "");

            if (!IsPhoneNumberValid(phoneNumber))
            {
                LogExtensions.Error("电话号码格式无效，请重新输入。正确格式示例：+8613812345678");
            }
        } while (!IsPhoneNumberValid(phoneNumber));

        return phoneNumber;
    }
}