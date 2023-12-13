using AI.Initialization;

namespace AI.Layers;
public abstract class Layer
{
    internal float[] b;
    internal float[] x;
    internal float[,] w;

    protected Layer(int size)
    {
        Size = size;
    }


    internal int Size { get; private protected set; }


    internal abstract float Activation(float x);
    internal abstract float ActivationDerivative(float x);




    internal void InitializeWeights(int inputs, IInitialization initialization)
    {
        b = new float[Size];
        x = new float[Size];
        w = new float[inputs, Size];
        initialization.Initialize(b, w);
    }
    internal virtual float[] FeedForward(float[] input)
    {
        for (int i = 0; i < x.Length; i++)
        {
            float z = 0;
            for (int n = 0; n < input.Length; n++)
            {
                z += input[n] * w[n, i];
            }
            z += b[i];
            x[i] = Activation(z);
        }

        return x;
    }

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
