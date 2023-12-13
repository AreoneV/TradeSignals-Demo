namespace AI.Initialization;

/// <summary>
/// Инициализирует веса и смещения рандомными значениями в диапазоне
/// </summary>
public class RandomWeights : IInitialization
{
    private readonly Random r = new();
    private readonly float min = -0.1f;
    private readonly float max = 0.1f;
    public RandomWeights()
    {

    }
    public RandomWeights(float max, float min)
    {
        this.min = min;
        this.max = max;
    }

    /// <summary>
    /// Производит инициализацию весов и смещений
    /// </summary>
    /// <param name="b">Смещения которые нужно инициализировать</param>
    /// <param name="w">Веса которые нужно инициализировать</param>
    public void Initialize(float[] b, float[,] w)
    {
        //вычисляем дельту
        var p = max - min;
        for(int i = 0; i < b.Length; i++)
        {
            //вычисляем рандом в диапазоне и присваеваем смещению
            b[i] = min + (float)r.NextDouble() * p;
        }
        for(int i = 0; i < w.GetLength(0); i++)
        {
            for(int j = 0; j < w.GetLength(1); j++)
            {
                //вычисляем рандом в диапазоне и присваеваем весу
                w[i, j] = min + (float)r.NextDouble() * p;
            }
        }
    }
}
