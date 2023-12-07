namespace Controller;

public enum ServiceStatus
{
    /// <summary>
    /// Запускается
    /// </summary>
    Starting,
    /// <summary>
    /// Останавливается
    /// </summary>
    Stopping,
    /// <summary>
    /// Работает и все хорошо
    /// </summary>
    Ok,
    /// <summary>
    /// Работает но есть на что обратить внимание
    /// </summary>
    Warning,
    /// <summary>
    /// Не работает из-за ошибки
    /// </summary>
    Error,
    /// <summary>
    /// Не работает без ошибок, не был запущен или по другим подобным причинам
    /// </summary>
    NotWorking,
    /// <summary>
    /// Не отвечает на запросы, завис
    /// </summary>
    NotResponding
}