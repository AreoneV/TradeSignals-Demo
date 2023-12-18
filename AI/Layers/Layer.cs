using AI.Initialization;

namespace AI.Layers;
/// <summary>
/// Слой нейронной сети
/// </summary>
public abstract class Layer
{
    //смещения
    internal float[] b;
    //значения
    internal float[] x;
    //веса
    internal float[,] w;

    protected Layer(int size)
    {
        Size = size;
    }

    /// <summary>
    /// Размер слоя
    /// </summary>
    internal int Size { get; private protected set; }

    /// <summary>
    /// Функция активации
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    internal abstract float Activation(float x);
    /// <summary>
    /// Производная функции активации
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    internal abstract float ActivationDerivative(float x);



    /// <summary>
    /// Инициализация весов
    /// </summary>
    /// <param name="inputs">Количество нейронов на предыдущем слое</param>
    /// <param name="initialization">Функция инициализации</param>
    internal void InitializeWeights(int inputs, IInitialization initialization)
    {
        b = new float[Size];
        x = new float[Size];
        w = new float[inputs, Size];
        //инициализируем
        initialization.Initialize(b, w);
    }
    /// <summary>
    /// Прямое распространение входящей информации
    /// </summary>
    /// <param name="input">Входящая информация</param>
    /// <returns></returns>
    internal virtual float[] FeedForward(float[] input)
    {
        for (int i = 0; i < x.Length; i++)
        {
            float z = 0;
            for (int n = 0; n < input.Length; n++)
            {
                //главная функция нейрона
                z += input[n] * w[n, i];
            }
            z += b[i];
            x[i] = Activation(z);
        }

        return x;
    }
    /// <summary>
    /// Расчет следующего градиента ошибки
    /// </summary>
    /// <param name="lastGradient">Предыдущей градиент ошибки</param>
    /// <param name="nextLayer">Следующий слой</param>
    /// <returns></returns>
    internal virtual float[] NextGradient(float[] lastGradient, Layer nextLayer)
    {
        float[] hiddenGradient = new float[Size];
        for (int j = 0; j < Size; j++)
        {
            // Градиент ошибки = производная функции активации * сумма взвешенных градиентов ошибки следующего слоя
            float sum = 0;
            for (int k = 0; k < nextLayer.Size; k++)
            {
                sum += lastGradient[k] * nextLayer.w[j, k];
            }
            hiddenGradient[j] = ActivationDerivative(x[j]) * sum;
        }

        return hiddenGradient;
    }
    /// <summary>
    /// Изменение весовых коэффициентов 
    /// </summary>
    /// <param name="gradients">Градиент ошибки</param>
    /// <param name="learningRate">Скорость обучения</param>
    /// <param name="inputs">Входные данные</param>
    internal virtual void ChangeWeights(float[] gradients, in float learningRate, float[] inputs)
    {
        for (int j = 0; j < Size; j++)
        {
            // Смещение = смещение + скорость обучения * градиент ошибки
            b[j] += learningRate * gradients[j];
            // Вес = вес + скорость обучения * градиент ошибки * значение предыдущего слоя
            for (int k = 0; k < inputs.Length; k++)
            {
                w[k, j] += learningRate * gradients[j] * inputs[k];
            }
        }
    }

    /// <summary>
    /// Сохранить веса слоя
    /// </summary>
    /// <param name="writer"></param>
    internal void SaveWeights(BinaryWriter writer)
    {
        writer.Write(Size);
        writer.Write(w.GetLength(0));
        foreach (var f in x)
        {
            writer.Write(f);
        }
        foreach (var f in b)
        {
            writer.Write(f);
        }

        for (int i = 0; i < w.GetLength(0); i++)
        {
            for (int j = 0; j < w.GetLength(1); j++)
            {
                writer.Write(w[i, j]);
            }
        }
    }
    /// <summary>
    /// Загрузить веса слоя
    /// </summary>
    /// <param name="reader"></param>
    internal void LoadWeights(BinaryReader reader)
    {
        Size = reader.ReadInt32();
        var num = reader.ReadInt32();
        b = new float[Size];
        x = new float[Size];
        w = new float[num, Size];

        for (int i = 0; i < Size; i++)
        {
            x[i] = reader.ReadSingle();
        }
        for (int i = 0; i < Size; i++)
        {
            b[i] = reader.ReadSingle();
        }
        for (int i = 0; i < w.GetLength(0); i++)
        {
            for (int j = 0; j < w.GetLength(1); j++)
            {
                w[i, j] = reader.ReadSingle();
            }
        }
    }
}
