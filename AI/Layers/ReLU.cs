namespace AI.Layers;
/// <summary>
/// Реализация функции активации нейрона ReLU
/// </summary>
/// <param name="size"></param>
public class ReLU(int size) : Layer(size)
{
    /// <summary>
    /// Функция активации
    /// </summary>
    /// <param name="x">Значение для преобразования</param>
    /// <returns></returns>
    internal override float Activation(float x)
    {
        return Math.Max(0.0f, x);
    }
    /// <summary>
    /// Производная функции активации
    /// </summary>
    /// <param name="x">Значение для преобразования</param>
    /// <returns></returns>
    internal override float ActivationDerivative(float x)
    {
        return x > 0 ? 1 : 0;
    }
}
