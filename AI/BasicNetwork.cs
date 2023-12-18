using AI.Initialization;
using AI.Layers;

namespace AI;
/// <summary>
/// Простая нейронная сеть
/// </summary>
/// <param name="inputs">Количество входных данных</param>
internal class BasicNetwork(int inputs)
{
    internal readonly int Inputs = inputs;
    internal List<Layer> Layers = [];
    /// <summary>
    /// Добавить слой в сеть
    /// </summary>
    /// <param name="layer">Слой</param>
    /// <param name="initialization">Функция инициализации</param>
    public void AddLayer(Layer layer, IInitialization initialization)
    {
        layer.InitializeWeights(Layers.Count == 0 ? Inputs : Layers.Last().Size, initialization);
        Layers.Add(layer);
    }


    /// <summary>
    /// Сохранить веса нейронной сети
    /// </summary>
    /// <param name="file">Путь к файлу</param>
    public void SaveWeights(string file)
    {
        var fs = new FileStream(file, FileMode.Create, FileAccess.Write);
        BinaryWriter w = new(fs);

        foreach(var layer in Layers)
        {
            layer.SaveWeights(w);
        }
        w.Close();
    }
    /// <summary>
    /// Загрузить веса для нейронной сети
    /// </summary>
    /// <param name="file"></param>
    public void LoadWeights(string file)
    {
        var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        BinaryReader w = new(fs);

        foreach(var layer in Layers)
        {
            layer.LoadWeights(w);
        }
        w.Close();
    }

    /// <summary>
    /// Предсказать результат по входным данным
    /// </summary>
    /// <param name="inputData"></param>
    /// <returns></returns>
    public float[] Predict(float[] inputData)
    {
        return Layers.Aggregate(inputData, (current, t) => t.FeedForward(current));
    }


}
