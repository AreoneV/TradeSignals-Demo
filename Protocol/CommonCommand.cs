namespace Protocol;
public enum CommonCommand
{
    /// <summary>
    /// Проверка скорости соединения
    /// </summary>
    Ping = 0,
    /// <summary>
    /// Специальная команда для каждого сервиса
    /// </summary>
    SpecialCommand,
    /// <summary>
    /// Все хорошо
    /// </summary>
    Ok,
    /// <summary>
    /// Ошибка
    /// </summary>
    Error,
    /// <summary>
    /// Запросить логи
    /// </summary>
    Logs,
    /// <summary>
    /// Команда на выключение
    /// </summary>
    Shutdown = 128
}
