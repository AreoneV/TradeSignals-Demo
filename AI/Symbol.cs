using System.Collections.ObjectModel;
using AI.Initialization;
using AI.Layers;
using MarketInfo;

namespace AI;

/// <summary>
/// Хранит нейронные сети для символа по разным временным рамкам
/// </summary>
internal class Symbol
{
    //нейронные сети для предсказания покупок по всем рамкам
    private readonly Dictionary<TimeFrame, BasicNetwork> buyNetworks = [];
    //нейронные сети для предсказания продаж по всем рамкам
    private readonly Dictionary<TimeFrame, BasicNetwork> sellNetworks = [];

    public Symbol(string name)
    {
        Name = name;
        //загружаем готовые нейронки для демонстрации
        Load();
    }


    public string Name { get; }

    /// <summary>
    /// Предсказать сигнал покупки и продажи по временной рамке
    /// </summary>
    /// <param name="timeFrame">Рамка</param>
    /// <param name="inputs">Входящие данные о рынке должен быть 200 значений</param>
    /// <returns>Возвращает предсказание на покупку и продажу</returns>
    public (float buy, float sell) Predict(TimeFrame timeFrame, float[] inputs)
    {
        //если длинна не под нейронку то возвращаем нули
        if (inputs.Length != Service.InputsBars * 4) return (0, 0);
        //ищем нужную рамку и возвращаем предсказание, если ее нет то нуль
        float buy = 0;
        if (buyNetworks.TryGetValue(timeFrame, out BasicNetwork nnBuy))
        {
            buy = nnBuy.Predict(inputs)[0];
        }
        //ищем нужную рамку и возвращаем предсказание, если ее нет то нуль
        float sell = 0;
        if(sellNetworks.TryGetValue(timeFrame, out BasicNetwork nnSell))
        {
            sell = nnSell.Predict(inputs)[0];
        }
        return (buy, sell);
    }
    /// <summary>
    /// Загрузка готовых весов для нейронных сетей
    /// </summary>
    private void Load()
    {
        //проверка папки
        if(!Directory.Exists("Weights")) return;
        //бегаем по папке
        foreach (string file in Directory.GetFiles("Weights"))
        {
            //берем каждый файл и парсим его
            var fi = new FileInfo(file);
            var arr = fi.Name.Split('-');

            if(arr.Length != 3 || arr[0] != Name) continue;

            if(!Enum.TryParse(typeof(TimeFrame), arr[2], out object otf)) continue;

            var tf = (TimeFrame)otf;
            //создаем нейронную сеть и загружаем в нее тренированные веса
            BasicNetwork nn = new BasicNetwork(Service.InputsBars * 4);
            nn.AddLayer(new ReLU(32), new RandomWeights());
            nn.AddLayer(new ReLU(16), new RandomWeights());
            nn.AddLayer(new ReLU(8), new RandomWeights());
            nn.AddLayer(new Sigmoid(1), new RandomWeights());

            nn.LoadWeights(file);

            switch (arr[1])
            {
                case "Buy":
                    buyNetworks.Add(tf, nn);
                    continue;
                case "Sell":
                    sellNetworks.Add(tf, nn);
                    break;
            }
        }
    }
}