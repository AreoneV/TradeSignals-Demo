namespace AI.Layers;
/// <summary>
/// Реализация функции активации нейрона Sigmoid
/// </summary>
/// <param name="size"></param>
public class Sigmoid(int size) : Layer(size)
{
    /// <summary>
    /// Функция активации
    /// </summary>
    /// <param name="x">Значение для преобразования</param>
    /// <returns></returns>
    internal override float Activation(float x)
    {
        return 1 / (1 + (float)Math.Exp(-x));
    }
    /// <summary>
    /// Производная функции активации
    /// </summary>
    /// <param name="x">Значение для преобразования</param>
    /// <returns></returns>
    internal override float ActivationDerivative(float x)
    {
        var z = Activation(x);
        return z * (1 - z);
    }
}
