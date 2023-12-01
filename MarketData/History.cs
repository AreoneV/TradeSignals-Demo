using System.Collections.ObjectModel;
using MarketInfo;

namespace MarketData;

public class History(TimeFrame timeFrame)
{
    private readonly List<Bar> barsList = new List<Bar>();


    /// <summary>
    /// Временная рамка истории баров
    /// </summary>
    public TimeFrame TimeFrame { get; } = timeFrame;

    /// <summary>
    /// Количество баров в истории
    /// </summary>
    public int Count
    {
        get { return barsList.Count; }
    }

    /// <summary>
    /// Первый бар в истории
    /// </summary>
    public Bar FirstBar
    {
        get { return barsList.FirstOrDefault(); }
    }

    /// <summary>
    /// Последний бар в истории
    /// </summary>
    public Bar LastBar
    {
        get { return barsList.LastOrDefault(); }
    }

    /// <summary>
    /// Время следующего будущего бара
    /// </summary>
    public DateTime FutureBarDate
    {
        get
        {
            //если история пуста то вернем следующий от текущего времени ина от последнего бара
            var lastBarTime = LastBar.Date;

            if (lastBarTime.Year == 1) return lastBarTime;

            DateTime dt = lastBarTime;

            //просто ко времени прибавляем значение минут данной временной рамки и возвращаем его начало
            switch(TimeFrame)
            {
                case TimeFrame.MN1:
                    dt = lastBarTime.AddMonths(1);
                    return new DateTime(dt.Year, dt.Month, 1);
                case TimeFrame.W1:
                    dt = lastBarTime.AddDays((int)dt.DayOfWeek * -1).AddDays(7);
                    return new DateTime(dt.Year, dt.Month, dt.Day);
                case TimeFrame.D1:
                    dt = lastBarTime.AddDays(1);
                    return new DateTime(dt.Year, dt.Month, dt.Day);
                case TimeFrame.H4:
                    dt = lastBarTime.AddHours(4);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                case TimeFrame.H1:
                    dt = lastBarTime.AddHours(1);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                case TimeFrame.M30:
                    dt = lastBarTime.AddMinutes(30);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
                case TimeFrame.M15:
                    dt = lastBarTime.AddMinutes(15);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
                case TimeFrame.M5:
                    dt = lastBarTime.AddMinutes(5);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
                case TimeFrame.M1:
                default:
                    dt = lastBarTime.AddMinutes(1);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            }
        }
    }

    public ReadOnlyCollection<Bar> Bars
    {
        get { return barsList.AsReadOnly(); }
    }

    /// <summary>
    /// Добавляет бары в историю, данные должны быть последовательны и после даты последнего бара
    /// </summary>
    /// <param name="arr"></param>
    public void Add(params Bar[] arr)
    {
        Add((IEnumerable<Bar>)arr);
    }
    /// <summary>
    /// Добавляет бары в историю, данные должны быть последовательны и после даты последнего бара
    /// </summary>
    /// <param name="bars"></param>
    public void Add(IEnumerable<Bar> bars)
    {
        foreach(var bar in bars)
        {

            //если бар не дальше нашего временного окна то просто сравним и изменим при необходимости
            if(bar.Date < FutureBarDate)
            {
                var low = LastBar.Low;
                var high = LastBar.High;
                if(bar.High > high)
                {
                    high = bar.High;
                }

                if(bar.Low < low)
                {
                    low = bar.Low;
                }

                var b = barsList[^1];
                b.High = high;
                b.Low = low;
                b.Close = bar.Close;

                barsList[^1] = b;
                continue;
            }

            var cb = bar;
            cb.Date = NormalizeDateTime(bar.Date);
            barsList.Add(cb);
        }
    }


    /// <summary>
    /// Бинарный поиск индекса бара по времени
    /// </summary>
    /// <param name="date">Время поиска</param>
    /// <param name="nearest">Определяет какой индекс вернуть если бар не найден, если True вернет ближайший если False вернет -1</param>
    /// <returns>Индекс бара, если бар не найден то возврат зависит он параметра nearest. Если он True то вернет индекс ближайшего, иначе вернет -1. Если найдет вернет индекс искомого в любом случае.</returns>
    public int IndexOf(DateTime date, bool nearest)
    {
        int left = 0, right = barsList.Count - 1;
        var index = right / 2;
        date = NormalizeDateTime(date);
        while(true)
        {
            if(right - left <= 1)
            {
                if(barsList[left].Date == date)
                {
                    return left;
                }
                if(barsList[right].Date == date)
                {
                    return right;
                }

                if(nearest)
                {
                    return right;
                }
                return -1;
            }
            if(barsList[index].Date == date)
            {
                return index;
            }
            if(barsList[index].Date < date)
            {
                left = index;
                index = left + (right - left) / 2;
            }
            else
            {
                right = index;
                index = left + (right - left) / 2;
            }
        }
    }


    /// <summary>
    /// Нормализация времени во время текущего TimeFrame истории, например TF == H4 то время 17:38 преобразуется во время 16:00, потому что именно в этом диапазоне(16:00-20:00) будет наш бар
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public DateTime NormalizeDateTime(DateTime date)
    {
        switch(TimeFrame)
        {
            //если месяц то возвращаем его начало
            case TimeFrame.MN1:
                return new DateTime(date.Year, date.Month, 1);
            //если неделя то возвращаем начало недели(Воскресенье)
            case TimeFrame.W1:
                var d = date.AddDays((int)date.DayOfWeek * -1);
                return new DateTime(d.Year, d.Month, d.Day);
            //Если день то возвращаем его начало
            case TimeFrame.D1:
                return new DateTime(date.Year, date.Month, date.Day);
            //если 4 часа то то возвращаем то время в какой интервал мы попадаем
            case TimeFrame.H4:
                {
                    int h = date.Hour switch
                    {
                        >= 0 and < 4 => 0,
                        >= 4 and < 8 => 4,
                        >= 8 and < 12 => 8,
                        >= 12 and < 16 => 12,
                        >= 16 and < 20 => 16,
                        _ => 20
                    };
                    return new DateTime(date.Year, date.Month, date.Day, h, 0, 0);
                }
            //если час то возвращаем его начало
            case TimeFrame.H1:
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            //если 30 минут но возвращаем ту часть часа куда мы попадаем
            case TimeFrame.M30:
                {
                    int h = date.Minute is >= 0 and < 30 ? 0 : 30;
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, h, 0);
                }
            //если 15 минут но возвращаем ту часть часа куда мы попадаем
            case TimeFrame.M15:
                {
                    int h;
                    if(date.Minute is >= 0 and < 15)
                        h = 0;
                    else
                    {
                        h = (date.Minute / 15) * 15;
                    }
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, h, 0);
                }
            //если 5 минут но возвращаем ту часть часа куда мы попадаем
            case TimeFrame.M5:
                {
                    int h;
                    if(date.Minute is >= 0 and < 5)
                        h = 0;
                    else
                    {
                        h = (date.Minute / 5) * 5;
                    }
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, h, 0);
                }
            //если 1 минута но возвращаем ту часть часа куда мы попадаем
            case TimeFrame.M1:
            default:
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }
    }
}