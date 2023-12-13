namespace AI.Initialization;
public interface IInitialization
{
    /// <summary>
    /// Производит инициализацию весов и смещений
    /// </summary>
    /// <param name="b">Смещения которые нужно инициализировать</param>
    /// <param name="w">Веса которые нужно инициализировать</param>
    void Initialize(float[] b, float[,] w);
}
